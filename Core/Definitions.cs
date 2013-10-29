namespace Mohid
{
   namespace Core
   {
      public enum DataTypes
      {
         UNKNOWN,
         INT,
         LONG,
         FLOAT,
         DOUBLE,
         STRING,
         DATETIME,
         OBJECT
      }

      public enum TimeUnits
      {
         UNKNOWN,
         YEARS,
         MONTHS,
         DAYS,
         HOURS,
         MINUTES,
         SECONDS
      }

      public enum IntervalType
      {
         UNKNOWN,
         ROW,
         RELATIVE,
         DATETIME
      }

      public enum SearchType
      {
         UNKNOWN,
         FIND_EXACTLY,
         FIND_EXACTLY_OR_PRIOR,
         FIND_EXACTLY_OR_NEXT,
         FIND_PRIOR,
         FIND_NEXT,
         FIND_NEAREST
      }

      public enum Result
      {
         UNKNOWN,
         OK,
         ERROR,
         EXCEPTION,
         TRUE,
         FALSE
      }
   }
}