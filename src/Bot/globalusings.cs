﻿/*
 * i know you're supposed to use the global using functionality for very common things like LINQ, but im going to abuse it.
 * if i can avoid needing to put the same set of usings in every file, im gonna avoid it
 */


global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Reflection;
global using System.Threading.Tasks;
global using Discord.WebSocket;
global using System.Diagnostics.CodeAnalysis;
global using System.Threading;
global using System.Text;
global using Discord.Net;
global using Discord.Rest;
global using Discord;
global using Humanizer;
global using RyuBot.Commands.Text;
global using RyuBot.Entities;
global using RyuBot.Services;
global using System.Net.Http;
global using System.Text.Json;
global using Sentry;
global using System.Diagnostics;
global using Microsoft.Extensions.DependencyInjection;
global using Qmmands;
global using RyuBot.Helpers;
global using Gommon;
global using static Gommon.Lambda;
global using static Gommon.Executor;
global using static RyuBot.Helpers.Logger;
global using static RyuBot.Entities.AppStatusEventArgs;

global using Console = Colorful.Console;
global using DiscordLogMessage = Discord.LogMessage;