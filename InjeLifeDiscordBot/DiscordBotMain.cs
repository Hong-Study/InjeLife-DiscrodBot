using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InjeLifeDiscordBot.Logger;
using System.Timers;

public class DiscordBotMain
{
    private DiscordSocketClient? _client;
public async Task BotMain()
{
    // SetBasePath는 구성 정보의 기본 디렉터리 설정
    var config = new ConfigurationBuilder()
        // this will be used more later on
        .SetBasePath(AppContext.BaseDirectory)
        // I chose using YML files for my config data as I am familiar with them
        .AddYamlFile("config.yml")
        .Build();

    using IHost host = Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
    services
    // Add the configuration to the registered services
    .AddSingleton(config)
    // Add the DiscordSocketClient, along with specifying the GatewayIntents and user caching
    .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
    {
        GatewayIntents = Discord.GatewayIntents.AllUnprivileged,
        LogGatewayIntentWarnings = false,
        AlwaysDownloadUsers = true,
        LogLevel = LogSeverity.Debug
    }))
    // Adding console logging
    .AddTransient<ConsoleLogger>()
    // Used for slash commands and their registration with Discord
    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
    // Required to subscribe to the various client events used in conjunction with Interactions
    .AddSingleton<InteractionHandler>()
    .AddSingleton<SQLManager>())
    .Build();

    await RunAsync(host);
}

public async Task RunAsync(IHost host)
{
    // host의 범위 지정
    using IServiceScope serviceScope = host.Services.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    var commands = provider.GetRequiredService<InteractionService>();
    _client = provider.GetRequiredService<DiscordSocketClient>();
    var config = provider.GetRequiredService<IConfigurationRoot>();

    await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

    // Subscribe to client log events
    _client.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);
    // Subscribe to slash command log events
    commands.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);

    // Read 상태일 때 실행할 람다 함수
    _client.Ready += async () =>
    {
        // 글로벌 정리
        // await _client.Rest.DeleteAllGlobalCommandsAsync();
        await _client.SetGameAsync("Please Use / Command");
        if (IsDebug())
        {  
            foreach(var guild in _client.Guilds)
            {
                ulong id = guild.Id;
                await commands.RegisterCommandsToGuildAsync(id, true);
            }
        }
        else
        {
            await commands.RegisterCommandsGloballyAsync(deleteMissing: true);
        }
    };

        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Interval = 1000;
        timer.AutoReset = true;

        // 정상 작동이 안됨. 수정 필요
        timer.Elapsed += async (sender, e) =>
        {
            try{
                using IServiceScope serviceScope = host.Services.CreateScope();
                IServiceProvider provider = serviceScope.ServiceProvider;

                var _client = provider.GetRequiredService<DiscordSocketClient>();
                var sqlManager = provider.GetRequiredService<SQLManager>();

                DateTime today = DateTime.Today;
                if (today.Hour == 9)
                {
                    if (!(today.DayOfWeek == DayOfWeek.Sunday || today.DayOfWeek == DayOfWeek.Saturday))
                    {
                        //수행할 타이머 이벤트
                        foreach (var guild in _client.Guilds)
                        {
                            if (InjeLifeModule.SelectChannel.ContainsKey(guild.Id) == true)
                            {
                                var channel = guild.GetTextChannel(InjeLifeModule.SelectChannel[guild.Id]);

                                var myEmbed = sqlManager.TodayCafeteria();

                                await channel.SendMessageAsync(embed: myEmbed);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                System.Console.WriteLine(exception.Message);
            }
        };
        timer.Enabled = true;

        // LoginAsync 실행 결과 대기
        await _client.LoginAsync(TokenType.Bot, config["tokens:discord"]);

        // StartAsync 실행 결과 대기
        await _client.StartAsync();

        // 무한 루프
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