using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;

public class DayTimer
{
    private System.Timers.Timer aTimer;
    private Action _action;

    private readonly double minuteTime = 1000 * 60; // 1분
    private DayOfWeek yesterdayDate = DateTime.Today.DayOfWeek;
    
    public void Start(Action action)
    {
        _action = action;

        SetTimer();
    }

    private void SetTimer()
    {
        aTimer = new System.Timers.Timer(minuteTime);

        // 이벤트 핸들러 연결
        aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

        // Timer에서 Elapsed 이벤트를 반복해서 발생
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        DayOfWeek todayDate = DateTime.Today.DayOfWeek;

        if (todayDate != yesterdayDate)
        {
            if (!(todayDate == DayOfWeek.Sunday || todayDate == DayOfWeek.Saturday))
            {
                //수행할 타이머 이벤트
                _action.Invoke();
            }

            yesterdayDate = todayDate;
        }
    }
}

