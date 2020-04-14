using System;
using System.IO;
using System.Text.Json;

namespace vpn_crash
{
    class Program
    {
        static void Main(string[] args)
        {
            var jsonString = File.ReadAllText("config.json");
            var options = JsonSerializer.Deserialize<Option>(jsonString);
            var bot = new Bot();
            bot.MainAsync(options.Token, options.ChannelId, options.FirstMessageId).GetAwaiter().GetResult();
        }
    }

    class Option
    {
        public string Token { get; set; }
        public ulong ChannelId { get; set; }
        public ulong FirstMessageId { get; set; }
    }
}
