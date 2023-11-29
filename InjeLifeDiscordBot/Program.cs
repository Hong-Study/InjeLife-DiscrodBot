using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Discord;
using Discord.Commands;
using System.Runtime.InteropServices;
using Discord.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;

class Program
{
    public static void Main(String[] args)
    {
        new DiscordBotMain().BotMain().GetAwaiter().GetResult();
    }
}