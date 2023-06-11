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
        new Program().BotMain().GetAwaiter().GetResult();
    }
    
    public async Task BotMain()
    {
        // SetBasePath는 구성 정보의 기본 디렉터리 설정
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yml")
            .Build();

        // 의존성 주입 이게 머노
        // Net Core 호스팅 환경 구성 및 애플리케이션 실행과 생명주기 관리 담당
        Util.GlobalHost = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            services
            .AddSingleton(config)
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = Discord.GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true,
            }))
            .AddSingleton<SQLManager>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>())
            .Build();

        await RunAsync();
    }

    public async Task RunAsync()
    {
        using IServiceScope serviceScope = Util.GlobalHost.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        var _client = provider.GetRequiredService<DiscordSocketClient>();
        var sCommands = provider.GetRequiredService<InteractionService>();
        // 읽어본 Yaml 파일에 대한 구성 정보를 가져오는 코드
        var config = provider.GetRequiredService<IConfigurationRoot>();

        await provider.GetRequiredService<SQLManager>().InitializeAsync();

        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

        _client.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); await Task.CompletedTask; };
        sCommands.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); await Task.CompletedTask; };

        _client.Ready += async () =>
        {
            await _client.SetGameAsync("Please Use / Command");

            if (IsDebug())
            { 
                foreach (var guild in _client.Guilds)
                {
                    ulong id = guild.Id;
                    await sCommands.RegisterCommandsToGuildAsync(id);
                }
            }
            else
            {
                await sCommands.RegisterCommandsGloballyAsync();
            }
        };

        await _client.LoginAsync(TokenType.Bot, config["tokens:discord"]);
        await _client.StartAsync();

        Util.dayTimer = new DayTimer();
        Util.dayTimer.Start(() => { _ = Util.DoTimer(); });

        await Task.Delay(-1);
    }

    static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}