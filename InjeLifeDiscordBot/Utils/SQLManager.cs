using Discord;
using Discord.Rest;
using Microsoft.Extensions.Configuration;
using Npgsql;

public class SQLManager
{
    private IConfigurationRoot _config;

    private string _host = "";
    private string _port = "";
    private string _database = "";
    private string _userId = "";
    private string _password = "";
    private string _connectionAddress = "";
    public SQLManager(IConfigurationRoot configurationRoot)
    {
        _config = configurationRoot;

        _host = _config["DB_HOST"];
        _port = _config["DB_PORT"];
        _database = _config["DB_DATABASE"];
        _userId = _config["DB_USERID"];
        _password = _config["DB_PASSWORD"];
        _connectionAddress = string.Format("HOST={0};PORT={1};USERNAME={2};PASSWORD={3};DATABASE={4}", _host, _port, _userId, _password, _database);
    }

    public async Task ConnectDB()
    {
        try
        {
            NpgsqlConnection mysql = new NpgsqlConnection(_connectionAddress);
            mysql.Open();

            Console.WriteLine("Success");
            mysql.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        await Task.CompletedTask;
    }

    public Embed TodayCafeteria()
    {
        DateTime date = DateTime.Now;
        return ReadCafeterial("Today 학식", date.Year, date.Month, date.Day, date.DayOfWeek);
    }
    public bool IsWeekday(DayOfWeek week)
    {
        if (week == DayOfWeek.Sunday || week == DayOfWeek.Saturday)
        {
            return false;
        }

        return true;
    }

    public Embed TommrowCafeteria()
    {
        DateTime date = DateTime.Now.AddDays(1);
        return ReadCafeterial("내일 학식", date.Year, date.Month, date.Day, date.DayOfWeek);
    }

    public Embed WeeksCafeterial(string title, int day)
    {
        DateTime date = DateTime.Now;
        return ReadCafeterial(title, date.Year, date.Month, day, date.DayOfWeek);
    }

    public Embed ReadCafeterial(string title, int year, int month, int days, DayOfWeek dayOfWeek)
    {
        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle(title)
            .WithColor(Color.Green);
        if (IsWeekday(dayOfWeek))
        {
            builder.WithDescription("학식입니다.");
            try
            {
                NpgsqlConnection mysql = new NpgsqlConnection(_connectionAddress);
                mysql.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = mysql;
                    string format = $"'{year}-{month}-{days}'";
                    cmd.CommandText = $"SELECT * FROM university_meals WHERE published_at = {format} ORDER BY course ASC LIMIT 3";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 체크하는 부분 넣기
                            string[] foods = reader[1] as string[];
                            string food = "";
                            for (int i = 0; i < foods.Length; i++)
                            {
                                food += foods[i] + "\n";
                            }
                            builder.AddField(reader[2].ToString(), food);
                        }
                    }
                }
                mysql.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        else
        {
            builder.WithDescription("주말이므로 존재하지 않습니다.");
        }
        return builder.Build();
    }
}