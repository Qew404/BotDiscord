using Discord;
using Discord.WebSocket;

public class HelpCommand
{
    public static async Task Execute(SocketMessage message)
    {
        // Создаем встроенное сообщение с заголовком, описанием и оформлением
        var embed = new EmbedBuilder()
            .WithTitle("Hi!")
            .WithDescription("What should I do?")
            .WithColor(new Color(88, 101, 242))
            .WithFooter(footer => footer.Text = "Click on the button below and Katya will definitely do it. Well, maybe, maybe not…")
            .WithCurrentTimestamp()
            .Build();

        // Создаем кнопки для взаимодействия
        var components = new ComponentBuilder()
            .WithButton("📋 Commission", "commission-button", ButtonStyle.Primary)
            .WithButton("📊 Queue", "queue-button", ButtonStyle.Success)
            .WithButton("👿 Bark like a dog", "troll-button", ButtonStyle.Secondary)
            .WithButton("🚫 The board of shame", "banlist-button", ButtonStyle.Danger)
            .Build();

        // Отправляем сообщение в канал с встроенным сообщением и кнопками
        await message.Channel.SendMessageAsync(embed: embed, components: components);
    }
}