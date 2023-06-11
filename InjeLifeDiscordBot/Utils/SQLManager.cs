using Discord;
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
    public SQLManager(IConfigurationRoot config)
    {
        _config = config;
    }

    public async Task InitializeAsync()
    {
        _host = _config["DB_HOST"];
        _port = _config["DB_PORT"];
        _database = _config["DB_DATABASE"];
        _userId = _config["DB_USERID"];
        _password = _config["DB_PASSWORD"];
        _connectionAddress = string.Format("HOST={0};PORT={1};USERNAME={2};PASSWORD={3};DATABASE={4}", _host, _port, _userId, _password, _database);

        //await ConnectDB();
        await Task.CompletedTask;
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
        // 구하기
        return ReadCafeterial("Today 학식", DateUtils.Today());
    }
    public Embed ReadCafeterial(string title, int days)
    {
        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle(title)
            .WithColor(Color.Green);

        if (days == (int)DayOfWeek.Sunday || days == (int)DayOfWeek.Saturday)
        {
            builder.WithDescription("주말이므로 존재하지 않습니다.");
        }
        else
        {
            builder.WithDescription("학식입니다.");

            try
            {
                NpgsqlConnection mysql = new NpgsqlConnection(_connectionAddress);
                mysql.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = mysql;
                    cmd.CommandText = "SELECT * FROM university_meals LIMIT 3";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 체크하는 부분 넣기
                            string[] foods = reader[1] as string[];
                            string food = "";
                            for (int i = 0; i < foods.Length; i++)
                            {
                                food += foods[i];
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

            return builder.Build();
        }
        return builder.Build();
    }
}