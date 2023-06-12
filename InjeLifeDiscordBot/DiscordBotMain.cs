using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DiscordBotMain
{
    public static DayTimer dayTimer;
    
    public async Task BotMain()
    {
        // SetBasePath는 구성 정보의 기본 디렉터리 설정
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yml")
            .Build();

        // 의존성 주입 이게 머노
        // Net Core 호스팅 환경 구성 및 애플리케이션 실행과 생명주기 관리 담당
        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            services
            .AddSingleton(config)
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = Discord.GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true,
            }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>())
            .Build();

        await RunAsync(host);
    }

    public async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        var _client = provider.GetRequiredService<DiscordSocketClient>();
        var sCommands = provider.GetRequiredService<InteractionService>();
        // 읽어본 Yaml 파일에 대한 구성 정보를 가져오는 코드
        var config = provider.GetRequiredService<IConfigurationRoot>();

        SQLManager.Instacne.SetConfig(config);
        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
        
        _client.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); await Task.CompletedTask; };
        sCommands.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); await Task.CompletedTask; };
        
        _client.Ready += async () =>
        {
            // 글로벌 정리
            // await _client.Rest.DeleteAllGlobalCommandsAsync();
            await _client.SetGameAsync("Please Use / Command");

            if (IsDebug())
            {
                //await FileUtils.ReadSelectChannel(_client, config);
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

        dayTimer = new DayTimer();
        dayTimer.Start(() => { _ = DoTimer(host); });
        
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
        var client = host.Services.GetRequiredService<DiscordSocketClient>();
        foreach (var guild in client.Guilds)
        {
            if (InteractionModule.SelectChannel.ContainsKey(guild.Id) == true)
            {
                var channel = guild.GetTextChannel(InteractionModule.SelectChannel[guild.Id]);
                var sql = host.Services.GetRequiredService<SQLManager>();

                var myEmbed = sql.TodayCafeteria();

                await channel.SendMessageAsync(embed: myEmbed);
            }
        }
    }
}
