using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    /// <summary>
    /// Класс для работы с клавиатурой.
    /// </summary>
    internal static class KeyboardMarkup
    {
        /// <summary>
        /// Возвращает разметку клавиатуры.
        /// </summary>
        /// <param name="back">Указывает, нужно ли ставить кнопку "Назад" или нет.</param>
        /// <param name="elements">Элементы на клавиатуре.</param>
        public static ReplyKeyboardMarkup GetKeyboardMarkup(bool back, params string[] elements)
        {
            if (elements == null)
                return null;
            KeyboardButton[] buttons = null;

            buttons = new KeyboardButton[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                buttons[i] = elements[i];

            if (back)
                return new(new[] { buttons, new KeyboardButton[] { "Назад" } }) { ResizeKeyboard = true };
            else
                return new(buttons) { ResizeKeyboard = true };
        }
    }
}

