using Discord.WebSocket;

namespace DBot.Commands
{
    public class Troll
    {
        // Список возможных сообщений для ответа
        private static readonly List<string> Messages = new()
        {
            "I won't bark",
            "I won't bark, I'm NOT A DOG!",
            "Did you want to be banned?!",
            "Where is my resignation letter…",
            "Stupid button… WHO INVENTED IT?",
        };

        // Множество использованных сообщений
        private static readonly HashSet<string> UsedMessages = new();
        // Флаг состояния сна
        private static bool isSleeping = false;
        // Счетчик сообщений во время сна
        private static int sleepMessageCount = 0;
        // Максимальное количество сообщений во время сна
        private static readonly int maxSleepMessages = 4;

        // Метод для пробуждения бота
        public static void WakeUp()
        {
            isSleeping = false;
            sleepMessageCount = 0;
            UsedMessages.Clear();
        }

        // Основной метод обработки команды
        public static async Task Execute(SocketMessage message)
        {
            // Проверка, спит ли бот
            if (isSleeping)
            {
                sleepMessageCount++;
                if (sleepMessageCount >= maxSleepMessages)
                {
                    WakeUp();
                    return;
                }
                await message.Channel.SendMessageAsync("I'm sleeping...😴");
                return;
            }

            // Проверка, были ли использованы все сообщения
            if (UsedMessages.Count >= Messages.Count)
            {
                await message.Channel.SendMessageAsync("That's it! I'm tired! She went to sleep...😴");
                isSleeping = true;
                UsedMessages.Clear();
                return;
            }

            // Выбор случайного неиспользованного сообщения
            string randomMessage;
            do
            {
                var random = new Random();
                randomMessage = Messages[random.Next(Messages.Count)];
            } while (UsedMessages.Contains(randomMessage));

            // Добавление сообщения в использованные и отправка
            UsedMessages.Add(randomMessage);
            await message.Channel.SendMessageAsync(randomMessage);
        }
    }
}