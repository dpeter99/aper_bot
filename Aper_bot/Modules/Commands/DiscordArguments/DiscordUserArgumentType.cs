﻿using Aper_bot.Util;

using Brigadier.NET;
using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Exceptions;

using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Serilog;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aper_bot.Modules.Commands.DiscordArguments
{
    class DiscordUserArgumentType : ArgumentType<DiscordUser>
    {
        private static Regex UserRegex { get; }

        Config config;

        static DiscordUserArgumentType()
        {
            UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);
        }

        DiscordBot discord;

        public DiscordUserArgumentType(DiscordBot bot, IOptions<Config> conf)
        {
            config = conf.Value;
            discord = bot;
        }

        public override DiscordUser? Parse(IStringReader reader)
        {
            Task<DiscordUser>? task;

            task = Process(reader);

            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }

            return task.Result;
        }


        public async Task<DiscordUser> Process(IStringReader reader)
        {
            //reader.SkipWhitespace();
            string token = reader.ReadTillSpace();

            if (ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await discord.Client.GetUserAsync(uid).ConfigureAwait(false);

                if (result == null)
                {
                    throw new DynamicCommandExceptionType(value => new LiteralMessage($"Invalid user '{value}'")).CreateWithContext(reader, token);
                }
                return result;
            }

            var m = UserRegex.Match(token);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await discord.Client.GetUserAsync(uid).ConfigureAwait(false);
                if (result == null)
                {
                    throw new DynamicCommandExceptionType(value => new LiteralMessage($"Invalid user '{value}'")).CreateWithContext(reader, token);
                }
                return result;
            }

            var cs = config.CaseSensitive;
            if (!cs)
                token = token.ToLowerInvariant();

            var di = token.IndexOf('#');
            var un = di != -1 ? token.Substring(0, di) : token;
            var dv = di != -1 ? token.Substring(di + 1) : null;

            var us = discord.Client.Guilds.Values
                .SelectMany(xkvp => xkvp.Members.Values)
                .Where(xm => (cs ? xm.Username : xm.Username.ToLowerInvariant()) == un && ((dv != null && xm.Discriminator == dv) || dv == null));

            var usr = us.FirstOrDefault();
            

            if(usr == null)
            {
                throw new DynamicCommandExceptionType(value => new LiteralMessage($"Invalid user '{value}'")).CreateWithContext(reader, token);
            }
            return usr;

            throw new SimpleCommandExceptionType(new LiteralMessage("Expected user")).CreateWithContext(reader);
        }
    }
}
