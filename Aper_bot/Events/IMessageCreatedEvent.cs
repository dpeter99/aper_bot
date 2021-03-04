﻿using System;
using System.Threading.Tasks;
using Aper_bot.Database.Model;
using DSharpPlus.Entities;

namespace Aper_bot.Events
{
    public interface IMessageCreatedEvent
    {
        public Guild? Guild { get; }

        public User Author { get; }

        public string Message { get; }

        public DateTime Time { get; }
        
        Task RespondError(string text);

        Task Respond(string text);

        Task<DiscordMessage> Respond(DiscordEmbed text);
    }
}