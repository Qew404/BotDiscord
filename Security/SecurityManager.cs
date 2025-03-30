using Discord;
using Discord.WebSocket;
using System.Collections.Concurrent;

public class SecurityManager
{
    // Discord клиент для взаимодействия с API
    private readonly DiscordSocketClient _client;
    // Словарь для хранения предупреждений пользователей
    private readonly ConcurrentDictionary<ulong, UserWarning> _warnings;
    // Максимальное количество предупреждений до бана
    private const int MAX_WARNINGS = 2;
    // Количество сообщений для определения спама
    private const int MESSAGE_THRESHOLD = 5;
    // Временное окно в секундах для проверки спама
    private const int TIME_WINDOW_SECONDS = 5;

    public SecurityManager(DiscordSocketClient client)
    {
        _client = client;
        _warnings = new ConcurrentDictionary<ulong, UserWarning>();
        
        // Подписываемся на события
        _client.UserJoined += HandleNewUser;
        _client.MessageReceived += HandleMessage;
    }

    private async Task<bool> AnalyzeNewUser(SocketGuildUser user)
    {
        // Проверяем возраст аккаунта
        var accountAge = DateTimeOffset.UtcNow - user.CreatedAt;
        if (accountAge.TotalDays < 7) return true;

        // Проверяем имя на подозрительные паттерны
        if (user.Username.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
            user.Username.Contains("spam", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private async Task HandleNewUser(SocketGuildUser user)
    {
        // Проверяем нового пользователя на подозрительную активность
        bool isSupicious = await AnalyzeNewUser(user);
        
        if (isSupicious)
        {
            // Запрашиваем верификацию у подозрительного пользователя
            await RequestVerification(user);
            // Даём пользователю роль "Не верифицирован"
            var unverifiedRole = user.Guild.Roles.FirstOrDefault(r => r.Name == "Not verified");
            if (unverifiedRole != null)
                await user.AddRoleAsync(unverifiedRole);
        }
    }

    private async Task SendWarning(SocketGuildUser user, string message)
    {
        try
        {
            // Отправляем предупреждение в личные сообщения
            await user.SendMessageAsync($"⚠️ {message}");
        }
        catch (Exception ex)
        {
            // Логируем ошибку
            Console.WriteLine($"Couldn't send a warning to the user {user.Username}: {ex.Message}");
        
            // Отправляем предупреждение в общий канал
            var channel = user.Guild.DefaultChannel;
            if (channel != null)
            {
                await channel.SendMessageAsync($"{user.Mention} ⚠️ {message}");
            }
        }
    }

    private async Task RequestVerification(SocketGuildUser user)
    {
        // Создаем embed сообщение для верификации
        var verifyEmbed = new EmbedBuilder()
            .WithTitle("🔒 Account Confirmation")
            .WithDescription("To confirm that you are not a bot, click on the button below")
            .WithColor(Color.Orange)
            .Build();

        // Создаем кнопку верификации
        var button = new ComponentBuilder()
            .WithButton("I'm not a bot", "verify-button", ButtonStyle.Success)
            .Build();

        // Отправляем сообщение с кнопкой
        await user.SendMessageAsync(embed: verifyEmbed, components: button);
    }

    private bool IsSpamming(SocketMessage message)
    {
        // Получаем или создаем запись о предупреждениях пользователя
        var warning = _warnings.GetOrAdd(message.Author.Id, new UserWarning());
        warning.RecentMessages.Add(DateTime.UtcNow);
        
        // Удаляем старые сообщения из окна времени
        warning.RecentMessages.RemoveAll(x => (DateTime.UtcNow - x).TotalSeconds > TIME_WINDOW_SECONDS);
        
        // Проверяем превышен ли порог сообщений
        return warning.RecentMessages.Count >= MESSAGE_THRESHOLD;
    }

    private async Task<bool> DetectScam(string content)
    {
        // Простая проверка на наличие подозрительных слов
        string[] scamWords = { "free nitro", "steam gift", "free discord", "giveaway", "claim now" };
        return scamWords.Any(word => content.ToLower().Contains(word));
    }

    private async Task HandleMessage(SocketMessage message)
    {
        // Игнорируем сообщения от ботов
        if (message.Author.IsBot) return;

        // Получаем пользователя как участника сервера
        var user = message.Author as SocketGuildUser;
        if (user == null) return;

        // Проверяем на спам
        if (IsSpamming(message))
        {
            await WarnUser(user, "spam");
        }

        // Проверяем содержимое сообщения на мошенничество
        if (await DetectScam(message.Content))
        {
            await WarnUser(user, "fraud");
        }
    }

    private async Task WarnUser(SocketGuildUser user, string reason)
    {
        // Получаем или создаем запись о предупреждениях пользователя
        var warning = _warnings.GetOrAdd(user.Id, new UserWarning());
        warning.Count++;

        // Проверяем, превышено ли максимальное количество предупреждений
        if (warning.Count >= MAX_WARNINGS)
        {
            // Баним пользователя
            warning.BanDate = DateTime.UtcNow;
            await user.BanAsync(reason: $"The warning limit has been exceeded: {reason}");
            await user.SendMessageAsync("You have been banned for violating the server rules");
        }
        else
        {
            // Отправляем предупреждение пользователю
            await SendWarning(user, $"Warning {warning.Count}/{MAX_WARNINGS}: Violation - {reason}");
        }
    }
}

// Класс для хранения информации о предупреждениях пользователя
public class UserWarning
{
    public int Count { get; set; }
    public DateTime LastWarning { get; set; }
    public DateTime? BanDate { get; set; }
    public List<DateTime> RecentMessages { get; set; } = new List<DateTime>();
}