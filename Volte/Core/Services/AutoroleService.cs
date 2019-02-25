﻿using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Volte.Core.Discord;

namespace Volte.Core.Services
{
    internal class AutoroleService : IService
    {
        internal async Task ApplyRoleAsync(SocketGuildUser user)
        {
            var config = VolteBot.GetRequiredService<DatabaseService>().GetConfig(user.Guild.Id);
            if (!string.IsNullOrEmpty(config.Autorole))
            {
                var targetRole = user.Guild.Roles.FirstOrDefault(r => r.Name.ToLower() == config.Autorole.ToLower());
                await user.AddRoleAsync(targetRole);
            }
        }
    }
}