using Discord;
using Discord.WebSocket;

public class CurrentQueueCommand
{
    // Клиент Discord для взаимодействия с API
    private readonly DiscordSocketClient _client;

    // Конструктор класса, принимающий клиент Discord
    public CurrentQueueCommand(DiscordSocketClient client)
    {
        _client = client;
    }

    // Метод для выполнения команды
    public async Task Execute(SocketMessage message)
    {
        // Получаем гильдию (сервер) из текущего канала
        var guild = (_client.GetChannel(message.Channel.Id) as SocketTextChannel)?.Guild;
        // Ищем канал с названием "currentqueue"
        var queueChannel = guild?.Channels.FirstOrDefault(x => x.Name == "currentqueue") as SocketTextChannel;

        if (queueChannel != null)
        {
            // Получаем последние 10 сообщений из канала очереди
            var messages = await queueChannel.GetMessagesAsync(10).FlattenAsync();

            // Создаем встроенное сообщение
            var embed = new EmbedBuilder()
                .WithTitle("Current queue")
                .WithDescription("Status of the current queue:")
                .WithColor(Color.Gold);

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