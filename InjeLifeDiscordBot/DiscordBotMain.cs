using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Timers;

public class DiscordBotMain
{
    public async Task BotMain()
    {
        // SetBasePath는 구성 정보의 기본 디렉터리 설정
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yml")
            .Build();
        
        // 의존성 주입 이게 머노
        // Net Core 호스팅 환경 구성 및 애플리케이션 실행과 생명주기 관리 담당
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            services
            .AddSingleton(config)
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = Discord.GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true,
            }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
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

        // 싱글톤 가져오기
        var _client = provider.GetRequiredService<DiscordSocketClient>();
        var sCommands = provider.GetRequiredService<InteractionService>();

        // 읽어본 Yaml 파일에 대한 구성 정보를 가져오는 코드
        var config = provider.GetRequiredService<IConfigurationRoot>();

        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

        _client.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); await Task.CompletedTask; };
        sCommands.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); await Task.CompletedTask; };

        // Read 상태일 때 실행할 람다 함수
        _client.Ready += async () =>
        {
            // 글로벌 정리
            // await _client.Rest.DeleteAllGlobalCommandsAsync();
            await _client.SetGameAsync("Please Use / Command");

            if (IsDebug())
            {
                FileUtils.ReadSelectChannel(_client, config);
                foreach (var guild in _client.Guilds)
                {
                    ulong id = guild.Id;
                    await sCommands.RegisterCommandsToGuildAsync(id);
                }

                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 1000;
                timer.AutoReset = true;

                // 정상 작동이 안됨. 수정 필요
                timer.Elapsed += async (sender, e) =>
                {
                    DateTime today = DateTime.Today;
                    if (today.Hour == 9)
                    {
                        if (!(today.DayOfWeek == DayOfWeek.Sunday || today.DayOfWeek == DayOfWeek.Saturday))
                        {
                            //수행할 타이머 이벤트
                            await DoTimer(host);
                        }
                    }
                };
                timer.Enabled = true;
            }
            else
            {
                await sCommands.RegisterCommandsGloballyAsync();
            }
        };

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

    // 타이머 내부 함수로, 다음날이 되면 실행한다. 1분마다 확인하게 됨.   
    public static async Task DoTimer(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        var client = provider.GetRequiredService<DiscordSocketClient>();
        var sqlManager = provider.GetRequiredService<SQLManager>();

        foreach (var guild in client.Guilds)
        {
            Console.WriteLine("DoTimer");

            if (InteractionModule.SelectChannel.ContainsKey(guild.Id) == true && sqlManager.IsWeekday(DateTime.Now.DayOfWeek))
            {
                Console.WriteLine(guild.Id);

                var channel = guild.GetTextChannel(InteractionModule.SelectChannel[guild.Id]);

                var myEmbed = sqlManager.TodayCafeteria();

                await channel.SendMessageAsync(embed: myEmbed);
            }
        }
    }
}