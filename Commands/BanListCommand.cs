using Discord;
using Discord.WebSocket;

public class BanListCommand
{
    // Клиент Discord для взаимодействия с API
    private readonly DiscordSocketClient _client;

    public BanListCommand(DiscordSocketClient client)
    {
        _client = client;
    }

    public async Task Execute(SocketMessage message)
    {
        var guildChannel = message.Channel as SocketGuildChannel;
        var guild = guildChannel?.Guild;
        if (guild == null) return;

        var bansList = await guild.GetBansAsync().FlattenAsync();

        var embed = new EmbedBuilder()
            .WithTitle("🚫 The board of shame")
            .WithColor(Color.Red)
            .WithCurrentTimestamp();

        foreach (var ban in bansList)
        {
            embed.AddField(
                $"User: {ban.User.Username}",
                $"Reason: {ban.Reason ?? "Not specified"}"
            );
        }

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }
}