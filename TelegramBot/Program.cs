using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using Telegram.Bot;

namespace TelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            WebProxy wp = new WebProxy("r1-proxy", 3128);
            TelegramBotClient remainder = new TelegramBotClient("Here is my token", wp);
            Remainder bot = new Remainder(remainder);            
            bot.Start();
            Console.ReadLine();          
        }
        
        
    }
}
