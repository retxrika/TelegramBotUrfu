using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace TelegramBot
{
    /// <summary>
    /// Информация о пользователе.
    /// </summary>
    [FirestoreData]
    internal class User
    {
        public User(Homework request, List<string> history)
        {
            Request = request;
            History = history;
        }

        public User() { }

        /// <summary>
        /// Запрос на домашнее задание.
        /// </summary>
        [FirestoreProperty]
        public Homework Request { get; set; } = new();

        /// <summary>
        /// История сообщений.
        /// </summary>
        [FirestoreProperty]
        public List<string> History { get; set; } = new();
    }
}
