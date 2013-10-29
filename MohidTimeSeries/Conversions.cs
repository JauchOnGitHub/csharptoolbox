using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid.Core;

namespace Mohid
{
   namespace MohidTimeSeries
   {
      public class Conversions
      {
         public static DateTime InstantToDateTime(double instant, DateTime start, TimeUnits units = TimeUnits.SECONDS)
         {
            DateTime instantAsDate = start;

            switch (units)
            {
               case TimeUnits.YEARS:
                  instantAsDate = start.AddYears((int)instant);
                  break;
               case TimeUnits.MONTHS:
                  instantAsDate = start.AddMonths((int)instant);
                  break;
               case TimeUnits.DAYS:
                  instantAsDate = start.AddDays(instant);
                  break;
               case TimeUnits.HOURS:
                  instantAsDate = start.AddHours(instant);
                  break;
               case TimeUnits.MINUTES:
                  instantAsDate = start.AddMinutes(instant);
                  break;
               case TimeUnits.SECONDS:
                  instantAsDate = start.AddSeconds(instant);
                  break;
            }

            return instantAsDate;
         }

         public static double InstantToRelative(DateTime instant, DateTime start, TimeUnits units = TimeUnits.SECONDS)
         {
            TimeSpan tspan;
            double instantAsRelative = 0.0;

            tspan = instant.Subtract(start);

            //Years and Months are not exactly

            switch (units)
            {
               case TimeUnits.YEARS:
                  instantAsRelative = tspan.TotalDays / 365.0;
                  break;
               case TimeUnits.MONTHS:
                  instantAsRelative = tspan.TotalDays / 30.0;
                  break;
               case TimeUnits.DAYS:
                  instantAsRelative = tspan.TotalDays;
                  break;
               case TimeUnits.HOURS:
                  instantAsRelative = tspan.TotalHours;
                  break;
               case TimeUnits.MINUTES:
                  instantAsRelative = tspan.TotalMinutes;
                  break;
               case TimeUnits.SECONDS:
                  instantAsRelative = tspan.TotalSeconds;
                  break;
            }

            return instantAsRelative;
         }
      }
   }
}