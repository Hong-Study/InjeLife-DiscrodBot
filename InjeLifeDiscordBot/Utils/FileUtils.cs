using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class FileUtils
{
    static string line = @"{0}: {1}";

    public static void ReadSelectChannel(DiscordSocketClient client, IConfigurationRoot config)
    {
        try
        {
            foreach (var guild in client.Guilds)
            {
                var value = config.GetSection(guild.Id.ToString()).Value;
                if (value == null)
                {
                    continue;
                }
                InjeLifeModule.SelectChannel[guild.Id] = ulong.Parse(value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void WriteSelectChannel(ulong guildId, ulong chennelId)
    {
        string inputLine = string.Format(line, guildId, chennelId);
        File.AppendAllText("config.yml", inputLine + "\n");
    }

    public static void DeleteSelectChannel(ulong guildId)
    {
        string fileText = "";
        foreach(string line in File.ReadLines("config.yml"))
        {
            if (line.Contains(guildId.ToString()))
                continue;
            fileText += (line + "\n");
        }

        File.WriteAllText("config.yml", fileText);
    }
}
