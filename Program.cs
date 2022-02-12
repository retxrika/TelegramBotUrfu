using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static TelegramBot.Homework;

namespace TelegramBot
{
    internal class Program
    {
        // Бот.
        private static TelegramBotClient bot;
        // База данных.
        private static DatabaseAPI<long> db;
        // Токен.
        private static string token = "Здесь токен :)";

        public static void Main()
        {
            bot = new(token);
            db = new();
            // Получение всех типов обновлений.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            // Для остановки бота.
            using var cts = new CancellationTokenSource();

            bot.ReceiveAsync(
               HandleUpdateAsync,
               HandleErrorAsync,
               receiverOptions,
               cts.Token
           );

            // После нажатия любой кнопки происходит остановка бота и сохранение данных.
            Console.ReadLine();
            cts.Cancel();
        }

        /// <summary>
        /// Обработка пресланных сообщений.
        /// </summary>
        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            /// Если пришедшее уведомление не сообщение => игнорируем.
            if (update.Type != UpdateType.Message)
                return;
            
            /// Если в бд нет добавленного дз => отправляем соответствующее сообщение.
            if (db.GetCountHomeworks() == 0)
            {
                await SendMessage(Responses.StartMessage(false), update.Message.Chat.Id, 
                    new ReplyKeyboardRemove());
                return;
            }

            /// Если пользователь отправил не текстовое сообщение и он есть в базе => 
            /// отправляем соответствующее сообщение.
            if (update.Message!.Type != MessageType.Text && db.ExistUser(update.Message.Chat.Id))
            {
                await SendMessage(Responses.NotExistValueErr(), update.Message.Chat.Id);
                return;
            }

            #region Variables

            /// Текстовое сообщение от пользователя.
            string text;
            /// ID пользователя.
            var id = update.Message.Chat.Id;
            /// Выбрана ли команда старта ("/start" или "начать сначала").
            bool startCmd = false;
            /// Выбрана ли команда "Назад".
            bool backCmd = false;
            /// Существование дз в бд. 
            bool existHmInDb;
            /// Обратное сообщение пользователю.
            string msgForUser = String.Empty;
            /// Ссылка на изображение дз.
            string imageUrl = String.Empty;
            /// Уникальные элементы для построения клавиатуры пользователю.
            string[] uniqueElements = null;
            
            #endregion

            /// Если пользователя нет в базе => формируем сообщение как стартовое.
            if (!db.ExistUser(update.Message.Chat.Id))
            {
                text = "/start";
            }
            else
            {
                text = update.Message.Text;
            }

            /// Обработка команды "/start" и "начать сначала".
            if (text.Equals("/start") || CompareText(text, "начать сначала"))
            {
                /// Если пользователя не было в базе => добавляем.
                if (!db.ExistUser(id))
                    db.AddUser(id, new());

                // Сбрасываем его состояние и запрос.
                db.SetState(id, State.None);
                db.SetRequest(id, new());
                startCmd = true;
            }
            /// Добавляем в историю сообщений отправленный текст.
            db.AddMsgInHistory(id, text);

            /// Если работает команда "назад" => возвращаем состояние.
            if (CompareText(text, "назад") && db.GetState(id) != State.GroupSelect
                    && db.GetState(id) != State.None)
            {
                backCmd = true;
                db.DeleteLastFieldRequest(id);
                db.SetState(id, db.GetState(id) - 2);
            }

            /// Проверка существования дз с новым полем в бд.
            if (db.GetState(id) == State.None)
                existHmInDb = db.GetCountMsgsInHistory(id) == 1;
            else
                existHmInDb = db.ExistHomework(new Homework(db.GetRequest(id), text));
            
            /// Если элемент не существует в бд и не команды отправляем соответствующее сообщение.
            if (!existHmInDb && !backCmd && !startCmd)
            {
                await SendMessage(Responses.NotExistValueErr(), id);
                return;
            }

            /// Если это не начало общение с пользователем и не команда назад 
            /// => сохраняем значение в запросе.
            if (!startCmd && !backCmd)
                db.SetValueInLastFieldRequest(id, text);

            /// Выбираем уникальные элементы для клавиатуры пользователю, если не выбор дисциплины.
            if (db.GetState(id) != State.DisciplineSelect)
            {
                uniqueElements = db.GetAllPosOptionsForEmpField(db.GetRequest(id));
            }
            else
            {
                // Уникальный элемент - команда "Начать сначала".
                uniqueElements = new string[] { "Начать сначала" };
                // Получение ссылки на изображение дз.
                imageUrl = await db.GetUrlImage(db.GetRequest(id));
            }

            /// Формирование ответа пользователю.
            msgForUser = db.GetState(id) switch
            {
                State.None => CompareText(text, "/start") ?
                        Responses.StartMessage() : Responses.GroupSelectMessage(),
                State.GroupSelect => Responses.MounthSelectMessage(),
                State.MounthSelect => Responses.DateSpanSelectMessage(),
                State.DateSpanSelect => Responses.DaySelectMessage(),
                State.DaySelect => Responses.DisciplineSelectMessage(),
                State.DisciplineSelect => db.GetDescriptionHm(id),
                _ => String.Empty
            };

            /// Генерируем кнопку "Назад", если это не начало и не конец общения с пользователем.
            bool buttonBack = db.GetState(id) != State.None 
                && db.GetState(id) != State.DisciplineSelect;
            /// Переходим к следующему состоянию.
            db.SetState(id, 
                (db.GetState(id) == State.DisciplineSelect) ? State.None : db.GetState(id) + 1);

            /// Отправка изображения с описанием.
            if (!String.IsNullOrWhiteSpace(imageUrl))
            {
                await SendImage(imageUrl, msgForUser, id,
                    KeyboardMarkup.GetKeyboardMarkup(buttonBack, uniqueElements));
            }
            /// Отправка текста пользователю.
            else
            {
                await SendMessage(msgForUser, id,
                    KeyboardMarkup.GetKeyboardMarkup(buttonBack, uniqueElements));
            }
            /// Сохраняем данные.
            await db.SaveData();
        }

        /// <summary>
        /// Обработка исключений.
        /// </summary>
        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Метод отправки сообщения.
        /// </summary>
        /// <param name="text">Текст.</param>
        /// <param name="id">ID чата.</param>
        /// <param name="km">Клавиатура с выбором для пользователя.</param>
        static async Task SendMessage(string text, long id, IReplyMarkup km = null)
        {
            await bot.SendTextMessageAsync(
                chatId: id,
                text: text,
                disableNotification: true,
                disableWebPagePreview: true,
                parseMode: ParseMode.Html,
                replyMarkup: km);
        }

        /// <summary>
        /// Метод отправки фото.
        /// </summary>
        /// <param name="image">Фото.</param>
        /// <param name="id">ID чата.</param>
        /// <param name="km">Клавиатура с выбором для пользователя.</param>
        static async Task SendImage(string image, string text, long id, IReplyMarkup km = null)
        {
            await bot.SendPhotoAsync(
                chatId: id,
                photo: image,
                caption: text,
                disableNotification: true,
                parseMode: ParseMode.Html,
                replyMarkup: km);
        }
    }
}
