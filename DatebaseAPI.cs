using Firebase.Storage;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramBot
{
    /// <summary>
    /// Состояния пользователя.
    /// </summary>
    internal enum State
    {
        None,
        GroupSelect,
        MounthSelect,
        DateSpanSelect,
        DaySelect,
        DisciplineSelect
    }

    /// <summary>
    /// Класс для работы с бд.
    /// </summary>
    /// <typeparam name="TId">Тип данных для id.</typeparam>
    internal class DatabaseAPI<TId>
    {
        // БД.
        private FirestoreDb database;
        // Коллекция домашних заданий.
        private CollectionReference homeworksFirebase;
        // Коллекция пользователей.
        private CollectionReference usersFirebase;

        /// <summary>
        /// Доступные домашние задания.
        /// </summary>
        private Dictionary<int, Homework> homeworks { get; } = new();

        /// <summary>
        /// Состояния пользователей.
        /// </summary>
        private Dictionary<TId, State> states { get; set; } = new();

        /// <summary>
        /// Пользователи с историей сообщений и запросом.
        /// </summary>
        private Dictionary<TId, User> users { get; set; } = new();

        public DatabaseAPI()
        {
            // Настройка бд.
            string path = AppDomain.CurrentDomain.BaseDirectory + @"здесь файличек с данными :)";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            // Инициализация бд.
            database = FirestoreDb.Create("cloudhomeworks");
            // Инициализация коллекции домашних заданий.
            homeworksFirebase = database.Collection("homeworks");
            // Инициализация коллекции пользователей.
            usersFirebase = database.Collection("users");

            // Выгружаем всех пользователей.
            UploadUsersAsync();
            // Подписываемся на обновление домашних заданий в бд.
            homeworksFirebase.Listen(UpdateHomeworks);
        }

        /// <summary>
        /// Возвращает булевское значение существования дз в коллекции.
        /// </summary>
        /// <param name="hm">Искомое дз.</param>
        public bool ExistHomework(Homework homework)
            => homeworks.ContainsValue(homework);

        /// <summary>
        /// Возвращает существование пользователя.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        public bool ExistUser(TId id) => states.ContainsKey(id);

        public void AddUser(TId id, User user) => users.Add(id, user);

        /// <summary>
        /// Возвращает запрос от пользователя.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        public Homework GetRequest(TId id) => users[id].Request;

        /// <summary>
        /// Установка нового запроса у пользователя.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        /// <param name="request">Новый запрос.</param>
        public void SetRequest(TId id, Homework request) => users[id].Request = request;

        /// <summary>
        /// Возвращает состояние пользователя.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        public State GetState(TId id) => states[id];

        /// <summary>
        /// Установка нового состояния для пользователя.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        /// <param name="state">Новое значение состояния.</param>
        public void SetState(TId id, State state) => states[id] = state; 

        /// <summary>
        /// Удаление последнего поля у запроса.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        public void DeleteLastFieldRequest(TId id) => users[id].Request.DeleteLastField();

        /// <summary>
        /// Установка значения в последнее пустое поле у запроса.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        /// <param name="value">Новое значение.</param>
        public void SetValueInLastFieldRequest(TId id, string value) => 
            users[id].Request.SetValueInLastField(value);

        /// <summary>
        /// Возвращает количество сообщений в истории сообщений.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        public int GetCountMsgsInHistory(TId id) => users[id].History.Count;

        /// <summary>
        /// Добавление нового сообщения в историю сообщений.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        /// <param name="msg">Новое сообщение.</param>
        public void AddMsgInHistory(TId id, string msg) => users[id].History.Add(msg);

        /// <summary>
        /// Возвращает все доступные варианты для установки на последнее пустое поле запроса.
        /// </summary>
        /// <param name="hm">Запрос с пустым полем, для которого требуются варианты</param>
        public string[] GetAllPosOptionsForEmpField(Homework request)
        {
            IEnumerable<string> allPosOptions = null;

            switch (request)
            {
                case var r when String.IsNullOrWhiteSpace(r.Group):
                    allPosOptions = from hm in homeworks.Values
                                        where hm.Equals(request)
                                        select hm.Group;
                    break;
                case var r when String.IsNullOrWhiteSpace(r.Mounth):
                    allPosOptions = from hm in homeworks.Values
                                        where hm.Equals(request)
                                        select hm.Mounth;
                    break;
                case var r when String.IsNullOrWhiteSpace(r.Week):
                    allPosOptions = from hm in homeworks.Values
                                    where hm.Equals(request)
                                    select hm.Week;
                    break;
                case var r when String.IsNullOrWhiteSpace(r.Day):
                    allPosOptions = from hm in homeworks.Values
                                    where hm.Equals(request)
                                    select hm.Day;
                    break;
                case var r when String.IsNullOrWhiteSpace(r.Discipline):
                    allPosOptions = from hm in homeworks.Values
                                    where hm.Equals(request)
                                    select hm.Discipline;
                    break;
            }

            return allPosOptions.Distinct().ToArray();
        }

        /// <summary>
        /// Возвращает описание домашнего задания по запросу пользователя.
        /// </summary>
        public string GetDescriptionHm(TId id) => 
            homeworks.Values.FirstOrDefault(hm => GetRequest(id).Equals(hm)).Description;

        /// <summary>
        /// Возвращает количество домашних заданий.
        /// </summary>
        /// <returns></returns>
        public int GetCountHomeworks() => homeworks.Count;

        /// <summary>
        /// Сохранение всех данных.
        /// </summary> 
        public async Task SaveData()
        {
            for (int i = 0; i < states.Count; i++)
            {
                var user = new Dictionary<string, object>
                {
                    { "State", states.ElementAt(i).Value },
                    { "History", users.ElementAt(i).Value.History },
                    { "Request", users.ElementAt(i).Value.Request }
                };

                await usersFirebase.Document($"{states.ElementAt(i).Key}").SetAsync(user);
            }
        }

        /// <summary>
        /// Возвращает ссылку на изображение домашнего задания.
        /// </summary>
        /// <param name="request">Запрос от пользователя.</param>
        public async Task<string> GetUrlImage(Homework request)
        {
            // Access token базы данных.
            string token = "Здесь токен :)";
            // Ссылка на storage.
            string bucket = "Здесь ссылочка :)";
            // Ссылка на изображение.
            string downloadUrl;

            try
            {
                var task = new FirebaseStorage(
                         bucket,
                         new FirebaseStorageOptions
                         {
                             AuthTokenAsyncFactory = () => Task.FromResult(token),
                             ThrowOnCancel = true,
                         })
                        .Child("Homeworks")
                        .Child(request.Group)
                        .Child(request.Mounth)
                        .Child(request.Week)
                        .Child(request.Day)
                        .Child(request.Discipline + ".jpg")
                        .GetDownloadUrlAsync();

                downloadUrl = await task;
            }
            catch
            {
                return String.Empty;
            }

            return downloadUrl;
        }

        /// <summary>
        /// Обработка обновлений данных.
        /// </summary>
        private void UpdateHomeworks(QuerySnapshot snapshot)
        {
            foreach (DocumentChange change in snapshot.Changes)
            {
                if (Int32.TryParse(change.Document.Id, out int id))
                {
                    var newHm = change.Document.ConvertTo<Homework>();

                    if (change.ChangeType == DocumentChange.Type.Added)
                    {
                        homeworks.Add(id, newHm);
                    }
                    else if (change.ChangeType == DocumentChange.Type.Modified)
                    {
                        homeworks[id] = newHm;
                    }
                    else if (change.ChangeType == DocumentChange.Type.Removed)
                    {
                        homeworks.Remove(id);
                    }
                }
            }
        }

        /// <summary>
        /// Выгрузка пользователей из бд.
        /// </summary>
        private async void UploadUsersAsync()
        {
            /// Выгружаем информацию о пользователях.
            QuerySnapshot allUsersQuerySnapshot = await usersFirebase.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in allUsersQuerySnapshot)
            {
                Dictionary<string, object> user = documentSnapshot.ToDictionary();

                TId id;
                try
                {
                    id = (TId)Convert.ChangeType(documentSnapshot.Id, typeof(TId));
                }
                catch
                {
                    id = default(TId);
                }

                if (!id.Equals(default(TId)))
                {
                    states.Add(id, State.None);
                    users.Add(id, new());

                    foreach (var pair in user)
                    {
                        switch (pair.Key)
                        {
                            case "State":
                                states[id] = (State)Enum.Parse(typeof(State), pair.Value.ToString());
                                break;
                            case "History":
                                var history = (List<object>)pair.Value;
                                foreach (var msg in history)
                                    users[id].History.Add(msg.ToString());
                                break;
                            case "Request":
                                var request = (Dictionary<string, object>)pair.Value;
                                users[id].Request = new(request["Group"].ToString(),
                                                        request["Mounth"].ToString(),
                                                        request["Week"].ToString(),
                                                        request["Day"].ToString(),
                                                        request["Discipline"].ToString());
                                break;
                        }
                    }
                }
            }
        }
    }
}
