using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using vpn_crash.DB;

namespace vpn_crash
{
    public class Bot
    {
        private ulong _channelId;
        private ulong _firstMessageId;
        private VPNCrashDB _db;
        private DiscordSocketClient _client;

        public async Task MainAsync(string token, ulong channelId, ulong firstMessageId)
        {
            _channelId = channelId;
            _firstMessageId = firstMessageId;

            _db = new VPNCrashDB();
            _db.Database.Migrate();

            _client = new DiscordSocketClient();
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Ready += Ready;
            _client.MessageReceived += MessageRecieved;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageRecieved(SocketMessage arg)
        {
            if (arg.Channel.Id != _channelId)
                return;

            if (arg.Content == "today")
            {
                await SendToday(arg.Channel);
            }
            else if (arg.Content == "total")
            {
                await SendTotal(arg.Channel);
            }
            else if (arg.Content == "+1")
            {
                HandleMessage(arg);
                await arg.Channel.SendMessageAsync(string.Format("C'est noté {0} !", arg.Author.Username));
            }
        }

        private async Task SendToday(ISocketMessageChannel channel)
        {
            var today = GetTodayMessage();
            await channel.SendMessageAsync(today);
        }

        private async Task SendTotal(ISocketMessageChannel channel)
        {
            var today = GetTotalMessage();
            await channel.SendMessageAsync(today);
        }

        private string GetTotalMessage()
        {
            var query = _db.CrashEntries;
            var summaries = GetSummaries(query);
            return string.Format("Voici les crash au total: ```{0}```", FormatSummaries(summaries));
        }

        private string GetTodayMessage()
        {
            var query = _db.CrashEntries.Where(x => x.Date.Date == DateTime.Now.Date);
            var summaries = GetSummaries(query);
            return string.Format("Voici les crash d'aujourd'hui: ```{0}```", FormatSummaries(summaries));
        }

        private string FormatSummaries(List<Summary> summaries)
        {
            var longestNameSize = summaries.Select(x => x.User.Length).Max();

            var sb = new StringBuilder();
            foreach (var user in summaries)
            {
                sb.AppendFormat("{0}: {1}\n", user.User.PadLeft(longestNameSize), user.Total);
            }
            sb.AppendLine();
            sb.AppendFormat("{0}: {1}\n", "Total".PadLeft(longestNameSize), summaries.Sum(x => x.Total));

            return sb.ToString();
        }

        private List<Summary> GetSummaries(IQueryable<CrashEntry> entries)
        {
            return entries
            .GroupBy(x => x.User)
            .Select(x => new Summary
            {
                User = x.Key,
                Total = x.Count()
            })
            .OrderBy(x => x.Total)
            .ToList();
        }

        private Task Ready()
        {
            ulong lastId = _firstMessageId;
            var lastEntry = _db.CrashEntries.OrderByDescending(x => x.Date).FirstOrDefault();
            if (lastEntry != null)
                lastId = lastEntry.MessageId;

            return UpdateHistory(lastId);
        }
        private async Task UpdateHistory(ulong lastId)
        {

            var channel = _client.GetChannel(_channelId) as IMessageChannel;
            var messages = await channel
            .GetMessagesAsync(lastId, Direction.After, 100, CacheMode.AllowDownload)
            .Flatten()
            .OrderBy(x => x.Timestamp)
            .ToList();

            while (messages.Count() > 0)
            {
                Console.WriteLine("Checking old missed messages...");
                foreach (var message in messages)
                    HandleMessage(message);
                lastId = messages.Last().Id;
                messages = await channel.
                GetMessagesAsync(lastId, Direction.After, 100, CacheMode.AllowDownload)
                .Flatten()
                .OrderBy(x => x.Timestamp)
                .ToList();
            }
        }

        private void HandleMessage(IMessage message)
        {
            if (message.Content != "+1")
                return;

            var entry = _db.CrashEntries.Find(message.Id);
            if (entry != null)
                return;

            entry = new CrashEntry()
            {
                MessageId = message.Id,
                Date = message.Timestamp.DateTime,
                User = message.Author.Username
            };
            _db.CrashEntries.Add(entry);
            _db.SaveChanges();
            Console.WriteLine("{0}: {1}", message.Timestamp, message.Author.Username);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }

    class Summary
    {
        public string User { get; set; }
        public int Total { get; set; }
    }
}