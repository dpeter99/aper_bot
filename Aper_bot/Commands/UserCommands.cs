﻿using System.Linq;
using System.Threading.Tasks;
using Aper_bot.Database;
using Aper_bot.Database.Model;
using Aper_bot.Events;
using Aper_bot.Modules.CommandProcessing;
using Aper_bot.Modules.CommandProcessing.Attributes;
using Aper_bot.Modules.CommandProcessing.DiscordArguments;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Microsoft.EntityFrameworkCore;

namespace Aper_bot.Commands
{
    [CommandProvider]
    class UserCommands : ChatCommands
    {
        Serilog.ILogger logger;

        //IDbContextFactory<DatabaseContext> dbContextFactory;

        public UserCommands(Serilog.ILogger log, IDbContextFactory<CoreDatabaseContext> fac)
        {
            logger = log;
            //dbContextFactory = fac;
        }

        public override LiteralArgumentBuilder<CommandExecutionContext> Register(IArgumentContext<CommandExecutionContext> l)
        {
            return l.Literal("/user")
                    .Then(
                        a =>
                        a.Literal("info")
                            .Then(
                                i =>
                                    i.Argument("user", DiscordArgumentTypes.User())
                                     .Executes(AsyncExecue(User))
                                )


                    )
                    .Then(
                        a =>
                        a.Argument("user", DiscordArgumentTypes.User())
                         .Executes(AsyncExecue(User))
                    );

        }

        public Task User(CommandContext<CommandExecutionContext> ctx, IMessageCreatedEvent messageEvent)
        {
            if (messageEvent is DiscordMessageCreatedEvent dmce)
            {
                
                DSharpPlus.EventArgs.MessageCreateEventArgs @event = dmce.@event;
                var user = (from u in ctx.Source.db.Users
                        where u.Name == @event.Author.Username
                        select u)
                    .FirstOrDefault();

                if (user != null)
                {
                    @event.Message.RespondAsync($"{user.Name} is a nice fellow");
                }
                else
                {
                    ctx.Source.db.Add(new User(@event.Author));
                    ctx.Source.db.SaveChangesAsync().Wait();
                    @event.Message.RespondAsync($"I have never see this person. Gona remember {@event.Author.Username}.");
                }
            }

            return Task.CompletedTask;
        }

        public int UserInfo(CommandContext<CommandExecutionContext> ctx, DiscordMessageCreatedEvent discordMessageEvent)
        {

            DSharpPlus.EventArgs.MessageCreateEventArgs @event = discordMessageEvent.@event;
            var user = (from u in discordMessageEvent.db.Users
                        where u.Name == @event.Author.Username
                        select u)
                       .FirstOrDefault();

            if (user != null)
            {
                @event.Message.RespondAsync($"{user.Name} is a nice fellow");
            }
            else
            {
                discordMessageEvent.db.Add(new User(@event.Author));
                discordMessageEvent.db.SaveChangesAsync().Wait();
                @event.Message.RespondAsync($"I have never see this person. Gona remember {@event.Author.Username}.");
            }

            return 0;
        }
    }
}