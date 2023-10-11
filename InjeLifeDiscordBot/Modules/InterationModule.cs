using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Channels;

public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    public static Dictionary<ulong, List<ulong>> counts = new Dictionary<ulong, List<ulong>>();
    public static Dictionary<ulong, ulong> SelectChannel = new Dictionary<ulong, ulong>();

    [SlashCommand("ping", "Hello World")]
    public async Task HandlePingTest()
    {
        if (CheckChannel(Context))
        {
            await RespondAsync("Bot is alive");
        }
        else
        {
            await RespondAsync("설정된 채널이 아닙니다.");
        }
    }

    [SlashCommand("학식", "금일의 학식을 보여드립니다.")]
    public async Task HandleTodayFood()
    {
        if (CheckChannel(Context))
        {
            var myEmbed = SQLManager.Instacne.TodayCafeteria();

            await RespondAsync(embed: myEmbed);
        }
        else
        {
            await RespondAsync("설정된 채널이 아닙니다.");
        }
    }
    [SlashCommand("내일학식", "다음날의 학식을 보여드립니다.")]
    public async Task HandleTomorrowFood()
    {
        if(CheckChannel(Context))
        {
            var myEmbed = SQLManager.Instacne.TommrowCafeteria();

            await RespondAsync(embed: myEmbed);
        }
        else
        {
            await RespondAsync("설정된 채널이 아닙니다.");
        }
    }
    
    [SlashCommand("요일별-학식", "Todo")]
    public async Task HandleComponentCommand()
    {
        if (CheckChannel(Context))
        {
            try
            {
                if (counts.ContainsKey(Context.Channel.Id) == false)
                {
                    counts[Context.Channel.Id] = new List<ulong>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var menu = new SelectMenuBuilder()
            {
                CustomId = "menu",
                Placeholder = "Select Days"
            };

            menu.AddOption("월", "Mon");
            menu.AddOption("화", "Tue");
            menu.AddOption("수", "Wed");
            menu.AddOption("목", "Thu");
            menu.AddOption("금", "Fri");

            var component = new ComponentBuilder();
            component.WithSelectMenu(menu);

            await RespondAsync("Show Select Menu", components: component.Build());

            // 요청이 끝난 후 디스코드에 생성된 메시지 아이디를 가져옴.
            
            var message = GetOriginalResponseAsync().Result;
            counts[Context.Channel.Id].Add(message.Id);
            // FlowAsync로도 가능하기는 한데, 두번 보내야한다는 단점이 있음.
        }
        else
        {
            await RespondAsync("설정된 채널이 아닙니다.");
        }
    }

    [ComponentInteraction("menu")]
    public async Task HandleMenuSelection(string[] inputs)
    {
        Embed myembed;

        if (inputs[0] == "Mon")
        {
            myembed = SQLManager.Instacne.WeeksCafeterial("월요일 학식", DateUtils.Monday());
            await RespondAsync(embed: myembed);
        }
        else if (inputs[0] == "Tue")
        {
            myembed = SQLManager.Instacne.WeeksCafeterial("화요일 학식", DateUtils.Tusday());
            await RespondAsync(embed: myembed);
        }
        else if (inputs[0] == "Wed")
        {
            myembed = SQLManager.Instacne.WeeksCafeterial("수요일 학식", DateUtils.Wednesday());
            await RespondAsync(embed: myembed);
        }
        else if (inputs[0] == "Thu")
        {
            myembed = SQLManager.Instacne.WeeksCafeterial("목요일 학식", DateUtils.Thursday());
            await RespondAsync(embed: myembed);
        }
        else if (inputs[0] == "Fri")
        {
            myembed = SQLManager.Instacne.WeeksCafeterial("금요일 학식", DateUtils.Friday());
            await RespondAsync(embed: myembed);
        }

        try
        {
            foreach (var contain in counts[Context.Channel.Id])
            {
                await Context.Channel.DeleteMessageAsync(contain);
            }
            counts[Context.Channel.Id].Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [SlashCommand("청소", "Clear Text")]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public async Task HandleClearChat()
    {
        // 채널의 전체 메시지 정보를 구함
        var message = Context.Channel.GetMessagesAsync().FlattenAsync().Result;
        
        // 14일 이전 메시지 삭제하는 코드
        //var filteredMessages = message.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

        // Get the total amount of messages.
        var count = message.Count();

        // Check if there are any messages to delete.
        if (count == 0)
            await RespondAsync("Nothing to delete.");
        else
        {

            // 형변환 이후, 전체 메시지를 삭제하는 코드
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(message);

            await RespondAsync($"Done. Removed {count} {(count > 1 ? "messages" : "message")}.");
        }
    }

    [SlashCommand("채널고정", "원하는 채널에 고정 시킵니다.")]
    public async Task HandleSelectChannel()
    {
        FileUtils.WriteSelectChannel(Context.Guild.Id, Context.Channel.Id);
        SelectChannel[Context.Guild.Id] = Context.Channel.Id;

        await RespondAsync("채널 고정 성공 : " + Context.Channel.Name);
    }

    [SlashCommand("채널고정-해제", "고정해둔 채널을 해제합니다.")]
    public async Task handleDeleteChannel()
    {
        if (SelectChannel.ContainsKey(Context.Guild.Id) == true)
        {
            FileUtils.DeleteSelectChannel(Context.Guild.Id);
            SelectChannel.Remove(Context.Guild.Id);
            await RespondAsync($"{Context.Channel.Name} 채널 고정을 해제하였습니다.");
        }
        else
        {
            await RespondAsync("고정해둔 채널이 없습니다.");
        }
    }
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
}