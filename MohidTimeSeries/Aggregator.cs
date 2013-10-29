using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid.Core;
using Mohid.Files;

namespace Mohid
{
   namespace MohidTimeSeries
   {
      public enum AggregationType
      {
         AVERAGE,
         SUM,
      }

      public class Aggregator
      {      
         #region DATA

         protected TimeSeries newTS;
         protected TimeSeries oldTS;
         protected FilePath newTSFilePath;
         protected AggregationType aggType;

         #endregion DATA

         #region CONSTRUCT         

         public virtual void Init()
         {
            newTS = new TimeSeries();
            newTS.TimeUnits = Core.TimeUnits.DAYS;
            aggType = AggregationType.AVERAGE;
         }

         public Aggregator()
         {
            Init();
         }
         #endregion CONSTRUCT

         #region GET-SET

         public FilePath FilePath
         {
            get { return newTSFilePath; }
            set
            {
               newTSFilePath = value;               
            }
         }

         public TimeUnits TimeUnits
         {
            get { return newTS.TimeUnits; }
            set { newTS.TimeUnits = value; }
         }

         public TimeSeries TimeSeriesToConvert
         {
            get { return oldTS; }
            set { oldTS = value; }
         }

         public AggregationType AggregationType
         {
            get { return aggType; }
            set { aggType = value; }
         }

         #endregion GET-SET

         #region ENGINE

         public bool Aggregate()
         {
            if (!CheckTimeUnits())
               return false;

            switch (aggType)
            {
               case AggregationType.AVERAGE:
                  return AggregateByAverage();
               case AggregationType.SUM:
                  return AggregateBySum();
            }

            return true;
         }

         protected bool AggregateByAverage()
         {
            int count = 0;
            int index, column_index;
            DateTime temp;
            double v_l = 0, v_i = 0, v_f = 0, v_d = 0;
            int instant = 0, actual_instant = 0;            

            for (column_index = 0; column_index < oldTS.NumberOfDataColumns; column_index++)
            {
               DataTypes data_type;
               Type t = oldTS.Columns[column_index].ColumnType;
               if (t == typeof(int))
               {
                  data_type = DataTypes.INT;
                  v_i = 0;
               }
               else if (t == typeof(long))
               {
                  data_type = DataTypes.LONG;
                  v_l = 0;
               }
               else if (t == typeof(float))
               {
                  data_type = DataTypes.FLOAT;
                  v_f = 0;
               }
               else if (t == typeof(double))
               {
                  data_type = DataTypes.DOUBLE;
                  v_d = 0;
               }
               else
                  continue;

               newTS.AddColumn(new Column(oldTS.Columns[column_index]));

               for (index = 0; index < oldTS.NumberOfInstants; index++)
               {
                  switch (newTS.TimeUnits)
                  {
                     case Core.TimeUnits.MINUTES:                     
                        actual_instant = oldTS.InstantAsDateTime(index).Minute;
                        break;
                     case Core.TimeUnits.HOURS:
                        actual_instant = oldTS.InstantAsDateTime(index).Hour;
                        break;
                     case Core.TimeUnits.DAYS:
                        actual_instant = oldTS.InstantAsDateTime(index).Day;
                        break;
                     case Core.TimeUnits.MONTHS:
                        actual_instant = oldTS.InstantAsDateTime(index).Month;
                        break;
                     case Core.TimeUnits.YEARS:
                        actual_instant = oldTS.InstantAsDateTime(index).Year;
                        break;
                     default:
                        return false;
                  }

                  if (count == 0)
                  {
                     instant = actual_instant;
                     if (column_index == 0)
                     {
                        temp = oldTS.InstantAsDateTime(index);
                        switch (newTS.TimeUnits)
                        {
                           case Core.TimeUnits.MINUTES:                     
                              temp.AddMinutes(1.0);
                              temp.AddSeconds(-oldTS.InstantAsDateTime(index).Second);
                              break;
                           case Core.TimeUnits.HOURS:
                              temp.AddHours(1.0);
                              temp.AddMinutes(-oldTS.InstantAsDateTime(index).Minute);
                              temp.AddSeconds(-oldTS.InstantAsDateTime(index).Second);
                              break;
                           case Core.TimeUnits.DAYS:
                              temp.AddDays(1.0);
                              temp.AddHours(-oldTS.InstantAsDateTime(index).Hour);
                              temp.AddMinutes(-oldTS.InstantAsDateTime(index).Minute);
                              temp.AddSeconds(-oldTS.InstantAsDateTime(index).Second);
                              break;
                           case Core.TimeUnits.MONTHS:
                              temp.AddMonths(1);
                              temp.AddDays(-oldTS.InstantAsDateTime(index).Day);
                              temp.AddHours(-oldTS.InstantAsDateTime(index).Hour);
                              temp.AddMinutes(-oldTS.InstantAsDateTime(index).Minute);
                              temp.AddSeconds(-oldTS.InstantAsDateTime(index).Second);
                              break;
                           case Core.TimeUnits.YEARS:
                              temp.AddYears(1);
                              temp.AddMonths(-oldTS.InstantAsDateTime(index).Month);
                              temp.AddDays(-oldTS.InstantAsDateTime(index).Day);
                              temp.AddHours(-oldTS.InstantAsDateTime(index).Hour);
                              temp.AddMinutes(-oldTS.InstantAsDateTime(index).Minute);
                              temp.AddSeconds(-oldTS.InstantAsDateTime(index).Second);
                              break;
                           default:
                              return false;
                        }
                        newTS.AddInstant(temp);
                     }
                  }

                  if (actual_instant == instant)
                  {
                     switch (data_type)
                     {
                        case DataTypes.INT:
                           v_i += (int)oldTS[column_index, index];
                           break;
                        case DataTypes.LONG:
                           v_l += (long)oldTS[column_index, index];
                           break;
                        case DataTypes.FLOAT:
                           v_f += (float)oldTS[column_index, index];
                           break;
                        case DataTypes.DOUBLE:
                           v_d += (double)oldTS[column_index, index];
                           break;
                        default:
                           return false;
                     }

                     count++;
                  }
                  else
                  {
                     switch (data_type)
                     {
                        case DataTypes.INT:
                           v_i = v_i / count;
                           break;
                        case DataTypes.LONG:
                           v_l = v_l / count;
                           break;
                        case DataTypes.FLOAT:
                           v_f = v_f / count;
                           break;
                        case DataTypes.DOUBLE:
                           v_d = v_d / count;
                           break;
                        default:
                           return false;
                     }

                     count = 0;
                     index--;
                  }
               }
            }
            return true;

            //needs to be finished
         }

         protected bool AggregateBySum()
         {
            return true;
         }

         protected bool CheckTimeUnits()
         {
            switch (newTS.TimeUnits)
            {
               case Core.TimeUnits.SECONDS:
                  return false;
               case Core.TimeUnits.MINUTES:
                  switch (oldTS.TimeUnits)
                  {
                     case Core.TimeUnits.SECONDS:
                        return true;
                     default:
                        return false;
                  }                  
               case Core.TimeUnits.HOURS:
                  switch (oldTS.TimeUnits)
                  {
                     case Core.TimeUnits.SECONDS:
                     case Core.TimeUnits.MINUTES:
                        return true;
                     default:
                        return false;
                  }                  
               case Core.TimeUnits.DAYS:
                  switch (oldTS.TimeUnits)
                  {
                     case Core.TimeUnits.SECONDS:
                     case Core.TimeUnits.MINUTES:
                     case Core.TimeUnits.HOURS:
                        return true;
                     default:
                        return false;
                  }
               case Core.TimeUnits.MONTHS:
                  switch (oldTS.TimeUnits)
                  {
                     case Core.TimeUnits.SECONDS:
                     case Core.TimeUnits.MINUTES:
                     case Core.TimeUnits.HOURS:
                     case Core.TimeUnits.DAYS:
                        return true;
                     default:
                        return false;
                  }
            case Core.TimeUnits.YEARS:
                  switch (oldTS.TimeUnits)
                  {
                     case Core.TimeUnits.SECONDS:
                     case Core.TimeUnits.MINUTES:
                     case Core.TimeUnits.HOURS:
                     case Core.TimeUnits.DAYS:
                     case Core.TimeUnits.MONTHS:
                        return true;
                     default:
                        return false;
                  }
            default:
                  return false;
            }
         }

         #endregion ENGINE
      }
   }
}
