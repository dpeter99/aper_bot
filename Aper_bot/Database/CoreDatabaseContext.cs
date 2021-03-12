﻿using Aper_bot.Database.Model;

using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aper_bot.Hosting.Database;

namespace Aper_bot.Database
{
    public class CoreDatabaseContext : DatabaseContext
    {
        public const string Schema = "Aper_Bot"; 
        
        public DbSet<User> Users => Set<User>();

        public DbSet<Guild> Guilds => Set<Guild>();

        public DbSet<Quote> Quotes => Set<Quote>();
        

        public CoreDatabaseContext(IOptions<DatabaseSettings> options):base(Schema, options)
        {
            //settings = options;
            //Database.Migrate();
        }
        
        
/*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            

            var connectionString = $"server={settings.Value.Address};port=3306;user={settings.Value.User};password={settings.Value.Password};database={settings.Value.Database_Name}";

            optionsBuilder.UseMySql(
                    connectionString,
                    MariaDbServerVersion.FromString("10.4.12-MariaDB-1:10.4.12+maria~bionic"),
                    MysqlOptions)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        void MysqlOptions(MySqlDbContextOptionsBuilder options)
        {
            options.CharSetBehavior(CharSetBehavior.NeverAppend);
            options.SchemaBehavior(MySqlSchemaBehavior.Translate, Translator);
        }

        private string Translator(string schemaname, string objectname)
        {
            return $"{schemaname}.{objectname}";
        }
*/
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>(entity =>
                {
                    entity.Property(e => e.Name)
                        .HasMaxLength(255);
                });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);

        }



        public User GetOrCreateUserFor(DiscordUser discordUser)
        {
            var user = (from u in Users
                        where u.UserID == discordUser.Id.ToString()
                        select u)
                           .FirstOrDefault();

            if(user == null)
            {
               //user = ;
               user = Add(new User(discordUser.Username, discordUser.Id.ToString())).Entity;
               SaveChanges();
            }

            return user;
        }

        public Guild? GetGuildFor(DiscordGuild discordGuild)
        {
            var guild = (from u in Guilds
                        where u.GuildID == discordGuild.Id.ToString()
                        select u)
                           .FirstOrDefault();

            return guild;
        }

    }
}
