using Google.Cloud.Firestore;
using System;

namespace TelegramBot
{
    /// <summary>
    /// Описание запроса к бд.
    /// </summary>
    [FirestoreData]
    public class Homework : ICloneable
    {
        public Homework(string group = default, 
            string mounth = default, 
            string week = default, 
            string day = default, 
            string discipline = default)
        {
            Group = group;
            Mounth = mounth;
            Week = week;
            Day = day;
            Discipline = discipline;
        }

        /// <summary>
        /// Конструктор для неполностью заполненной записи домашнего задания. 
        /// </summary>
        /// <param name="previousHomework">Предыдущее неполностью заполненное дз.</param>
        /// <param name="newData">Новые данные для незаполненного поля.</param>
        public Homework(Homework previousHomework, string newData)
        {
            if (!String.IsNullOrWhiteSpace(previousHomework.Group))
            {
                Group = previousHomework.Group;

                if (!String.IsNullOrWhiteSpace(previousHomework.Mounth))
                {
                    Mounth = previousHomework.Mounth;

                    if (!String.IsNullOrWhiteSpace(previousHomework.Week))
                    {
                        Week = previousHomework.Week;

                        if (!String.IsNullOrWhiteSpace(previousHomework.Day))
                        {
                            Day = previousHomework.Day;

                            if (!String.IsNullOrWhiteSpace(previousHomework.Discipline))
                            {
                                Discipline = previousHomework.Discipline;
                            }
                            else
                            {
                                Discipline = newData;
                            }
                        }
                        else
                        {
                            Day = newData;
                        }
                    }
                    else
                    {
                        Week = newData;
                    }
                }
                else
                {
                    Mounth = newData;
                }
            }
            else
            {
                Group = newData;
            }
        }

        public Homework() { }

        /// <summary>
        /// Группа.
        /// </summary>
        [FirestoreProperty]
        public string Group { get; set; } = String.Empty;

        /// <summary>
        /// Месяц.
        /// </summary>
        [FirestoreProperty]
        public string Mounth { get; set; } = String.Empty;

        /// <summary>
        /// Неделя.
        /// </summary>
        [FirestoreProperty]
        public string Week { get; set; } = String.Empty;

        /// <summary>
        /// День.
        /// </summary>
        [FirestoreProperty]
        public string Day { get; set; } = String.Empty;

        /// <summary>
        /// Дисциплина.
        /// </summary>
        [FirestoreProperty]
        public string Discipline { get; set; } = String.Empty;

        /// <summary>
        /// Описание.
        /// </summary>
        [FirestoreProperty]
        public string Description { get; set; } = String.Empty;

        public override bool Equals(object obj)
        {
            if (!this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Homework h = (Homework)obj;
                return (CompareText(h.Group, this.Group) || String.IsNullOrWhiteSpace(h.Group)) && 
                    (CompareText(h.Mounth, this.Mounth) || String.IsNullOrWhiteSpace(h.Mounth)) &&
                    (CompareText(h.Week, this.Week) || String.IsNullOrWhiteSpace(h.Week)) && 
                    (CompareText(h.Day, this.Day) || String.IsNullOrWhiteSpace(h.Day)) &&
                    (CompareText(h.Discipline, this.Discipline)
                        || String.IsNullOrWhiteSpace(h.Discipline));
            }
        }

        public object Clone() => new Homework(Group, Mounth, Week, Day, Discipline);

        /// <summary>
        /// Установка значение в последнее поле.
        /// </summary>
        /// <param name="value">Значение для последнего незаполненного поля.</param>
        public void SetValueInLastField(string value)
        {
            switch (this)
            {
                case var hm when String.IsNullOrWhiteSpace(hm.Group):
                    hm.Group = value;
                    break;
                case var hm when String.IsNullOrWhiteSpace(hm.Mounth):
                    hm.Mounth = value;
                    break;
                case var hm when String.IsNullOrWhiteSpace(hm.Week):
                    hm.Week = value;
                    break;
                case var hm when String.IsNullOrWhiteSpace(hm.Day):
                    hm.Day = value;
                    break;
                case var hm when String.IsNullOrWhiteSpace(hm.Discipline):
                    hm.Discipline = value;
                    break;
            }
        }

        /// <summary>
        /// Удаление последнего поля.
        /// </summary>
        public void DeleteLastField()
        {
            switch (this)
            {
                case var hm when !String.IsNullOrWhiteSpace(hm.Discipline):
                    hm.Discipline = String.Empty;
                    break;
                case var hm when !String.IsNullOrWhiteSpace(hm.Day):
                    hm.Day = String.Empty;   
                    break;
                case var hm when !String.IsNullOrWhiteSpace(hm.Week):
                    hm.Week = String.Empty;
                    break;
                case var hm when !String.IsNullOrWhiteSpace(hm.Mounth):
                    hm.Mounth = String.Empty;
                    break;
                case var hm when !String.IsNullOrWhiteSpace(hm.Group):
                    hm.Group = String.Empty;
                    break;
            }
        }

        /// <summary>
        /// Сравнение текста с игнорированием регистра.
        /// </summary>
        /// <param name="message1">Первое сообщение.</param>
        /// <param name="message2">Второе сообщение.</param>
        /// <returns>true - если сообщения одинаковы, false - нет.</returns>
        public static bool CompareText(string text1, string text2)
            => text1.Equals(text2, StringComparison.OrdinalIgnoreCase);
    }
}
