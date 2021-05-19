using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TelegramBot
{
    class Remainder
    {        
        enum actionState {ready,commandReceived,messageReceived,timeReceived};
        Dictionary<string, string> commands = new Dictionary<string, string>(2);
        private TelegramBotClient _client;
        private System.Threading.Timer _timer;
        Dictionary<long, Action> chats = new Dictionary<long, Action>();
        string _botsFullName;
        static long _requestId;

        class Action
        {
            public long requestId;
            public long chatId;
            public string user;
            public string message;
            public actionState state;
            public DateTime timeToRemind;
        }
        
        
        public void Start()
        {
            var userName = _client.GetMeAsync().Result.FirstName;            
            _client.StartReceiving();
            Console.WriteLine($"Start listening @{userName}");
        }

        void CheckReminders(Object obj)
        {
            var localChats = chats.ToArray();
            foreach (var item in localChats)
            {
                if (DateTime.Now > item.Value.timeToRemind && item.Value.state == actionState.timeReceived)
                {                    
                    _client.SendTextMessageAsync(item.Value.chatId, $"@{item.Value.user}, {item.Value.message}");
                    item.Value.state = actionState.ready;
                    chats.Remove(item.Value.requestId);
                }
            }
        }

        public Remainder(TelegramBotClient client)
        {            
           _timer = new System.Threading.Timer(CheckReminders, null, 0, 1000);
            _botsFullName = client.GetMeAsync().Result.Username;
            commands.Add("Remind", "/remind");
            commands.Add("Cancel", "/cancel");
            commands.Add("RemindGroup", $"/remind@{_botsFullName}");
            commands.Add("CancelGroup", $"/cancel@{_botsFullName}");            
            _client = client;
            client.OnMessage += BotOnMessageReceived;
        }
        
        
        void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {            
            Action action = chats.SingleOrDefault(e => e.Value.user == messageEventArgs.Message.From.Username && e.Value.state != actionState.timeReceived && e.Value.chatId == messageEventArgs.Message.Chat.Id).Value;          
            if (action == null)
            { 
                action = new Action()
                {
                    requestId = _requestId++,
                    chatId = messageEventArgs.Message.Chat.Id,
                    user = messageEventArgs.Message.From.Username
                };
                chats.Add(action.requestId, action);
                //action.chatId = messageEventArgs.Message.Chat.Id;
                //chats.Add(messageEventArgs.Message.Chat.Id,action);                
            }
            Answer(messageEventArgs.Message.Text, action);
        }
        
    #region Здесь описаны ответы Бота
    void Answer(string message, Action chat)
        {
            
            if (message == commands["Cancel"] || message == commands["CancelGroup"])
            {
                //chat.state = actionState.ready;
                chats.Remove(chat.requestId);
                _client.SendTextMessageAsync(chat.chatId, "Ты сам попросил!");
            }            
            switch (chat.state)
            {
                case actionState.ready:
                    if (message == commands["Remind"] || message == commands["RemindGroup"])
                    {
                        _client.SendTextMessageAsync(chat.chatId, "Что необходимо напомнить?");
                        chat.state = actionState.commandReceived;                        
                    };
                    break;
                case actionState.commandReceived:
                    chat.message = message;
                    _client.SendTextMessageAsync(chat.chatId, "Через сколько секунд напомнить?");
                    chat.state = actionState.messageReceived;
                    break;
                case actionState.messageReceived:
                    int intervalToRemind;
                    if (int.TryParse(message, out intervalToRemind))
                    {
                        if (intervalToRemind > 0)
                        {
                            _client.SendTextMessageAsync(chat.chatId, "Ок! Напомню через " + intervalToRemind + " секунд");
                            chat.timeToRemind = DateTime.Now.AddSeconds(intervalToRemind);
                            chat.state = actionState.timeReceived;
                        }
                        else
                        {
                            _client.SendTextMessageAsync(chat.chatId, "Число должно быть положительным");
                        }

                    }
                    else
                    {                      
                       _client.SendTextMessageAsync(chat.chatId, "Неверный формат, введи число или /отмена");                        
                    }
                    break;
            }
            Console.WriteLine("Answered");
        }
        #endregion
    }

}
