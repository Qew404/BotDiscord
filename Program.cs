using DBot.Commands;
using Discord;
using Discord.WebSocket;

// Основной класс программы
class Program
{
    // Клиент Discord для взаимодействия с API
    private DiscordSocketClient _client;
    // Команда для работы с комиссиями
    private CommissionCommand _commissionCommand;
    // Команда для работы с текущей очередью
    private CurrentQueueCommand _currentQueueCommand;
    private SecurityManager _securityManager;


    public static Task Main(string[] args) => new Program().MainAsync();

    // Основной асинхронный метод программы
    public async Task MainAsync()
    {
        // Инициализация клиента Discord с настройкой прав доступа
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.MessageContent |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.Guilds
        });

        // Инициализация команд
        _commissionCommand = new CommissionCommand(_client);
        _currentQueueCommand = new CurrentQueueCommand(_client);
        _securityManager = new SecurityManager(_client);

        _client.ButtonExecuted += HandleButtonInteraction;


        // Подписка на события
        _client.Log += Log;
        _client.MessageReceived += MessageReceived;

        // Токен бота
        string token = "Токен бота писать сюда";

        // Подключение бота к Discord
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Бесконечное ожидание
        await Task.Delay(-1);
    }

    // Метод для логирования сообщений
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    // Обработчик получения сообщений
    private async Task MessageReceived(SocketMessage message)
    {
        // Игнорируем сообщения от ботов
        if (message.Author.IsBot) return;

        var channel = message.Channel as SocketTextChannel;
        var botUser = channel?.Guild.CurrentUser;

        // Проверяем права
        Console.WriteLine($"Все гуд?: {botUser?.GetPermissions(channel).SendMessages}");

        // Обработка команд
        switch (message.Content.ToLower())
        {
            case "!go":
                await HelpCommand.Execute(message);
                break;
        }
    }

    // Обработчик нажатий на кнопки
    private async Task HandleButtonInteraction(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            // Кнопка комиссии
            case "commission-button":
                await new CommissionCommand(_client).Execute(component.Message);
                break;
            // Кнопка очереди
            case "queue-button":
                await new CurrentQueueCommand(_client).Execute(component.Message);
                break;
            // Кнопка троллинга
            case "troll-button":
                await Troll.Execute(component.Message);
                break;
                // Кнопка списка забаненных
            case "banlist-button":
                await new BanListCommand(_client).Execute(component.Message);
                break;
        }

        // Подтверждаем что кнопка обработана
        await component.DeferAsync();
    }

}