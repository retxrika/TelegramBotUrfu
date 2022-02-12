namespace TelegramBot
{
    /// <summary>
    /// Ответы пользователю.
    /// </summary>
    internal static class Responses
    {
        /// <summary>
        /// Возвращает сообщение для начала работы с ботом.
        /// </summary>
        /// <param name="existHm">true - если домашнее задание есть, false - нет.</param>
        public static string StartMessage(bool existHm = true)
        {
            string startMes = "Приветики)\n" +
                        "Я помогу тебе найти твоё домашнее задание по любой дисциплине.\n" +
                        "Если ты встретишься с какими-то проблемами или у тебя возникнут вопросы " +
                        "по работе со мной, то обращайся к этому парню 😉:\n" +
                        "<b><a href = \"https://vk.com/fuckingslaveeeee \">Олежа</a></b> (ФТ-110007)\n" +
                        "Если ты не увидел своего домашнего задания и при этом уверен, что оно есть," +
                        " то обращайся к этим зайкам 🥰:\n" +
                        "<b><a href = \"https://vk.com/dsi2210 \">Даня</a></b> (ФТ-110007)\n" +
                        "<b><a href = \"https://vk.com/kipyatup \">Саня</a></b> (ФТ-110008)\n\n";

            // В зависимости от присутствия или отсутствия дз концовка будет различаться.
            return existHm ? startMes + GroupSelectMessage() : startMes +
                "Домашних заданий пока что нет 🥳, так что " +
                "у тебя есть время заняться <b>собой</b> 😌."; ;
        }

        /// <summary>
        /// Возвращает сообщение для выбора группы.
        /// </summary>
        public static string GroupSelectMessage() => "Выбери <b>группу</b>, в которой ты учишься.";

        /// <summary>
        /// Возвращает сообщение для выбора месяца.
        /// </summary>
        public static string MounthSelectMessage() => "Выбери <b>месяц</b>.";

        /// <summary>
        /// Возвращает сообщение для выбора недели.
        /// </summary>
        public static string DateSpanSelectMessage() => "Выбери <b>неделю</b>.";

        /// <summary>
        /// Возвращает сообщения для выбора дня недели.
        /// </summary>
        public static string DaySelectMessage() => "Выбери <b>день недели</b>.";

        /// <summary>
        /// Возвращает сообщение для выбора дисциплины.
        /// </summary>
        public static string DisciplineSelectMessage() => "И последнее, осталось выбрать нужную " +
            "<b>дисциплину</b>.";

        /// <summary>
        /// Возвращает сообщение ошибки о неправильном вводе пользователем.
        /// </summary>
        public static string NotExistValueErr() => "Полезный совет, выбери лучше что-нибудь из " +
            "предложенных вариантов 😉.";

        public static string ExceptionMessage(string message)
            => $"Ошибка: {message}\nПопробуй обратиться к " +
            $"<b><a href = \"https://vk.com/fuckingslaveeeee \">Олегу</a></b> 😕.";
    }
}
