using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;

using Telegram.Bot.Types;

// для получения типа сообщения
using Telegram.Bot.Types.Enums;

using Telegram.Bot.Types.ReplyMarkups; // для создания клавиатуры
                                       //using Telegram.Bot.Types.InlineQueryResults; // ? для создания кнопок 

using ApiAiSDK;
using ApiAiSDK.Model;

namespace TelegramBot_Core_Console
{
    class Program
    {
        // 6ff45427701948208179247e794f1af6 -- DialogFlow Agent -- Client

        static TelegramBotClient Bot;
        static ApiAi apiAi;

        static void Main(string[] args)
        {
            // создать клиента бота на основе токена, который даёт Botfather
            // при создании бота
            Bot = new TelegramBotClient("628819908:AAFWj7Qd2ip3EnUBv3PQtDNEigzLTpz7tBk");
            AIConfiguration config = new AIConfiguration("6ff45427701948208179247e794f1af6", SupportedLanguage.Russian);
            apiAi = new ApiAi(config);


            // подписка на событие -- когда будет приходить сообщение,
            // будет вызываться метод BotOnMessageReceived
            Bot.OnMessage += BotOnMessageReceived;

            //
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;

            // выдаст имя бота
            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);

            // начать получение сообщений
            Bot.StartReceiving();

            Console.ReadKey(true);

            Bot.StopReceiving();
        }

        // Этот метод вызывается при вызове, например,
        // InlineKeyboardButton.WithCallbackData("Пункт 1")
        private static async void BotOnCallbackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            // текст, ассоциированный с нажатой кнопкой
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"{name} нажал кнопку {buttonText}");

            if (buttonText == "Картинка")
            {
                // e.CallbackQuery.From.Id -- id чата, в котором пользователь
                //                            взаимодействует с ботом
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://i.ytimg.com/vi/f-e9hSUSlSM/maxresdefault.jpg");
            }
            else if (buttonText == "Видео")
            {
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://www.youtube.com/watch?v=f-e9hSUSlSM");
            }

            // Отобразиться у клиента в виде всплывающего сообщения по центру чата
            // e.CallbackQuery.Id -- id кнопки, которую пользователь
            //                       нажимает при взаимодействии с ботом
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"Вы нажали кнопку {buttonText}");
        }

        // async нужен для асинхронной обработки входящих/исходящих сообщений
        // Например с помощью метода await Bot.SendTextMessageAsync(message.From.Id, text);
        private static async void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            // получить сообщение
            var message = e.Message;


            if (message == null || message.Type != MessageType.Text)
                return;

            string name = $"{message.From.FirstName} {message.From.LastName}";

            Console.WriteLine($"{name} отправил сообщение: \"{message.Text}\"");

            switch (message.Text)
            {
                case "/start":
                    string text =
@"Список команд:
/start    - запуск бота
/callback - вывод меню
/keyboard - вывод клавиатуры";

                    // message.From.Id -- id чата, в котором пользователь
                    //                    взаимодействует с ботом
                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;

                // отправка клавиатуры
                case "/keyboard":
                    var replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            // KeyboardButton -- Большая кнопка возле поля ввода,
                            // после нажатия которой вводиться текст её надписи в поле ввода

                            new KeyboardButton("Привет"),
                            new KeyboardButton("Как дела?"),
                        },
                        new[]
                        {
                            new KeyboardButton("Контакт") { RequestContact = true },
                            new KeyboardButton("Геолокация") { RequestLocation = true },
                        }
                    });
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Сообщение",
                        replyMarkup: replyKeyboard);
                    break;

                // отправка меню
                case "/callback":

                    // создаём клавиатуру
                    var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                        new[]{
                            InlineKeyboardButton.WithUrl("VK", "https://vk.com"),
                            InlineKeyboardButton.WithUrl("Telegram", "https://t.me/nislacker")
                        },
                        new[] {
                            InlineKeyboardButton.WithCallbackData("Картинка"),
                            InlineKeyboardButton.WithCallbackData("Видео")
                        }
                    });

                    await Bot.SendTextMessageAsync(message.From.Id, "Выберите пункт меню:", replyMarkup: inlineKeyboard);

                    break;

                default:
                    var response = apiAi.TextRequest(message.Text);
                    string answer = response.Result.Fulfillment.Speech;
                    if (answer == "")
                        answer = "Прости, я тебя не понял";
                    await Bot.SendTextMessageAsync(message.From.Id, answer);
                    break;
            }
        }
    }
}
