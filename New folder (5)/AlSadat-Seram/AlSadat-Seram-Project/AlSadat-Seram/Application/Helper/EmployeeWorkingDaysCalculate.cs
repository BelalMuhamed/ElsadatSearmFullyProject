using Domain.Entities.HR;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helper;
public class EmployeeWorkingDaysCalculate
{
    public static int CalculateWorkingDays(int year , int month ,
        WeekDays WeekHoliday1 , WeekDays? WeekHoliday2 , List<PublicHoliday> publicHolidays)
    {
        var firstDay = new DateOnly(year,month,1);//1-10-2025
        var lastDay = firstDay.AddMonths(1).AddDays(-1);//30-10-2025
        int workingDays = 0;

        DayOfWeek weeklyHoliday1 = (DayOfWeek) WeekHoliday1-1;
        DayOfWeek? weeklyHoliday2 = WeekHoliday2 != null ? (DayOfWeek) WeekHoliday2 : null;

        for(var day = firstDay;day <= lastDay;day = day.AddDays(1))
        {
            if(day.DayOfWeek == weeklyHoliday1)
                continue;
            if(weeklyHoliday2.HasValue && day.DayOfWeek == weeklyHoliday2.Value)
                continue;

            if(publicHolidays.Any(ph => ph.Date == day))
                continue;

            workingDays++;
        }

        return workingDays;
    }
}
