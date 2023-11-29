using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DateUtils
{
    public static int Today() { return DateTime.Today.Day; }
    public static int Monday() { return MakeWeeksDay(DayOfWeek.Monday); }
    public static int Tusday() { return MakeWeeksDay(DayOfWeek.Tuesday); }
    public static int Wednesday() { return MakeWeeksDay(DayOfWeek.Wednesday); }
    public static int Thursday() { return MakeWeeksDay(DayOfWeek.Thursday); }
    public static int Friday() { return MakeWeeksDay(DayOfWeek.Friday); }

    static int MakeWeeksDay(DayOfWeek dayOfWeek)
    {
        DateTime dateToday = DateTime.Today;
        int todayWeeks = 0;

        if (Convert.ToInt32(dateToday.DayOfWeek) == 0)
        {
            todayWeeks = 7;
        }
        else
        {
            todayWeeks = Convert.ToInt32(dateToday.DayOfWeek);
        }

        DateTime date = dateToday.AddDays(Convert.ToInt32(dayOfWeek) - todayWeeks);

        return date.Day;
    }
}
