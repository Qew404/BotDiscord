using Discord;
using Discord.WebSocket;

public class CommissionCommand
{
    // Клиент Discord для взаимодействия с API
    private readonly DiscordSocketClient _client;

    // Конструктор класса, принимающий клиент Discord
    public CommissionCommand(DiscordSocketClient client)
    {
        _client = client;
    }

    // Метод для выполнения команды
    public async Task Execute(SocketMessage message)
    {
        // Получаем гильдию (сервер) из текущего канала
        var guild = (_client.GetChannel(message.Channel.Id) as SocketTextChannel)?.Guild;
        // Ищем канал с названием "commission"
        var commissionChannel = guild?.Channels.FirstOrDefault(x => x.Name == "commission") as SocketTextChannel;

        if (commissionChannel != null)
        {
            // Получаем последние 10 сообщений из канала commission
            var messages = await commissionChannel.GetMessagesAsync(10).FlattenAsync();

            // Создаем встроенное сообщение
            var embed = new EmbedBuilder()
                .WithTitle("Commission")
                .WithDescription("Status and price list:")
                .WithColor(Color.Green);

            // Добавляем каждое сообщение как отдельное поле во встроенное сообщение
            foreach (var msg in messages)
            {
                embed.AddField($"{msg.Author.Username}", msg.Content);
            }

            // Отправляем встроенное сообщение в канал
            await message.Channel.SendMessageAsync(embed: embed.Build());
        }
        else
        {
            // Отправляем сообщение об ошибке, если канал не найден
            await message.Channel.SendMessageAsync("Error");
        }
    }
}