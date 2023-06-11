using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Util
{
    // static 함수들을 다 모아놨는데 이게 좋은 코드가 아닌건 확실함.
    public static IHost GlobalHost { get; set; }
    public static Dictionary<ulong, List<ulong>> counts = new Dictionary<ulong, List<ulong>>();
    public static Dictionary<ulong, ulong> SelectChannel = new Dictionary<ulong, ulong>();
    public static DayTimer dayTimer;

    // 고정된 채널이 있는지 확인하고, 있다면 context의 채널과 비교하여 bool 리턴
    public static bool CheckChannel(SocketInteractionContext context)
    {
        if (SelectChannel.ContainsKey(context.Guild.Id) == true)
        {
            if (SelectChannel[context.Guild.Id] == context.Channel.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    // 타이머 내부 함수로, 다음날이 되면 실행한다. 1분마다 확인하게 됨.
    public static async Task DoTimer()
    {
        var client = GlobalHost.Services.GetRequiredService<DiscordSocketClient>();
        foreach (var guild in client.Guilds)
        {
            if (SelectChannel.ContainsKey(guild.Id) == true)
            {
                var channel = guild.GetTextChannel(SelectChannel[guild.Id]);
                var sql = GlobalHost.Services.GetRequiredService<SQLManager>();

                var myEmbed = sql.TodayCafeteria();

                await channel.SendMessageAsync(embed: myEmbed);
            }
        }
    }
}

