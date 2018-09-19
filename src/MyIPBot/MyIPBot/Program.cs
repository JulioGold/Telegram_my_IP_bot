using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyIPBot
{
    class Program
    {
        private static IConfiguration _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        private static readonly TelegramBotClient _bot = new TelegramBotClient(_config["TelegramToken"]);

        public static void Main(string[] args)
        {
            var me = _bot.GetMeAsync().Result;
            Console.Title = me.Username;

            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnMessageEdited += BotOnMessageReceived;

            _bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            _bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text)
            {
                return;
            }

            switch (message.Text.Split(' ').First())
            {
                case "/myip":
                    await _bot.SendTextMessageAsync(message.Chat.Id, $"Your IP address is: {GetExternalIPAddress()}");
                    break;

                case "/raport":
                    await _bot.SendTextMessageAsync(message.Chat.Id, "Use port 3389 to remote access.");
                    break;

                default:
                    const string usage = @"
Usage:
/myip   - return your IP address
/raport - return the default remote access port
";

                    await _bot.SendTextMessageAsync(message.Chat.Id, usage, replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }

        public static string GetExternalIPAddress()
        {
            string externalIP = String.Empty;

            externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");

            externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();

            return externalIP;
        }
    }
}
