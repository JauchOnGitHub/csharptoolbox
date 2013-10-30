using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

using Mohid.Core;
using Mohid.Files;

namespace Mohid
{
   namespace MohidTimeSeries
   {
      public struct Interval
      {
         public int Start;
         public int End;

         public Interval(int start, int end)
         {
            Start = start;
            End = end;
         }
      }

      public class TimeSeries
      {
         #region DATA

         protected string name;
         protected List<string> extraLines;
         protected Column instants;
         protected List<Column> dataColumns;
         protected DateTime startInstant;
         protected TimeUnits timeUnits;
         protected string residual;
         protected bool saveResidual;

         #endregion DATA

         #region CONSTRUCT

         public virtual void Init()
         {
            saveResidual = false;
            extraLines = new List<string>();
            instants = new Column(typeof(double));
            instants.DataFormat = "0.0000";
            dataColumns = new List<Column>();
            startInstant = DateTime.Now;
         }

         public TimeSeries()
         {
            Init();
         }

         #endregion CONSTRUCT

         #region INFO

         public string Name
         {
            get { return name; }
            set { name = value; }
         }

         public List<Column> Columns { get { return dataColumns; } }

         public DateTime StartInstant
         {
            get
            {
               return startInstant;
            }

            set
            {
               DateTime oldValue = startInstant;                                
               startInstant = value;

               if (instants.Count > 0 && oldValue != value)
                  RefreshInstants(oldValue, value);
            }
         }

         protected void RefreshInstants(DateTime oldValue, DateTime newValue)
         {
            for(int i = 0; i < instants.Count; i++)
            {               
               instants[i] = Conversions.InstantToRelative(InstantAsDateTime(i, oldValue), newValue, timeUnits);                
            }
         }

         public TimeUnits TimeUnits
         {
            get
            {
               return timeUnits;
            }

            set
            {
               timeUnits = value;
            }
         }

         public int NumberOfInstants
         {
            get { return instants.Count; }
         }

         public int NumberOfColumns
         {
            get { return dataColumns.Count + 1; }
         }

         public int NumberOfDataColumns
         {
            get { return dataColumns.Count; }
         }

         public Interval Interval(IntervalType type, object start, object end)
         {
            Interval i;

            switch (type)
            {
               case IntervalType.ROW:
                  i.Start = (int)start;
                  i.End = (int)end;
                  break;
               case IntervalType.DATETIME:
                  i.Start = Index((DateTime)start, SearchType.FIND_EXACTLY_OR_NEXT);
                  i.End = Index((DateTime)end, SearchType.FIND_EXACTLY_OR_PRIOR);
                  break;
               case IntervalType.RELATIVE:
                  i.Start = Index((double)start, SearchType.FIND_EXACTLY_OR_NEXT);
                  i.End = Index((double)end, SearchType.FIND_EXACTLY_OR_PRIOR);
                  break;
               default:
                  throw new Exception("Invalid Interval Type.");
            }

            return i;
         }

         #endregion INFO

         #region INPUT

         public int AddInstant(DateTime instant)
         {
            return AddInstant(Conversions.InstantToRelative(instant, startInstant, timeUnits));
         }

         public int AddInstant(double instant)
         {
            int index;
            bool exist = false;

            for (index = 0; index < instants.Count; index++)
            {
               if ((double)(instants[index]) == instant)
               {
                  exist = true;
                  break;
               }
               else if ((double)(instants[index]) > instant)               
                  break;
            }

            if (!exist)
            {
               instants.Insert(index, instant);

               foreach (Column col in dataColumns)
               {
                  col.Insert(index, col.DefaultValue);
               }
            }

            return index;
         }

         public bool AddTimeSeries(TimeSeries tsToAdd)
         {
            if (tsToAdd != null)
            {
               int row, col, index;

               for (row = 0; row < tsToAdd.NumberOfInstants; row++)
               {
                  index = AddInstant(tsToAdd.InstantAsDateTime(row));

                  for (col = 0; col < tsToAdd.NumberOfDataColumns; col++)
                     this[col, index] = (double)tsToAdd[col, row];
               }
            }

            return true;
         }

         public bool AddColumn(Column column)
         {
            try
            {
               dataColumns.Add(column);
               return true;
            }
            catch
            {
               return false;
            }
         }

         #endregion INPUT

         #region GET-SET

         public object this[int column, int index]
         {
            get { return dataColumns[column][index]; }
            set 
            {
               CheckType(value, dataColumns[column].ColumnType);
               dataColumns[column][index] = value;
            }
         }

         public DateTime InstantAsDateTime(int index)
         {
            return Conversions.InstantToDateTime((double)instants[index], startInstant, timeUnits);
         }

         public DateTime InstantAsDateTime(int index, DateTime start)
         {
            return Conversions.InstantToDateTime((double)instants[index], start, timeUnits);
         }

         public double InstantAsRelative(int index)
         {
            return (double)instants[index];
         }
         
         public int AsInt(int column, int index)
         {
            CheckType(dataColumns[column][index], typeof(int));
            return (int)dataColumns[column][index];
         }

         public long AsLong(int column, int index)
         {
            CheckType(dataColumns[column][index], typeof(long));
            return (long)dataColumns[column][index];
         }

         public float AsFloat(int column, int index)
         {
            CheckType(dataColumns[column][index], typeof(float));
            return (float)dataColumns[column][index];
         }

         public double AsDouble(int column, int index)
         {
            CheckType(dataColumns[column][index], typeof(double));
            return (double)dataColumns[column][index];
         }

         public string AsString(int column, int index)
         {
            CheckType(dataColumns[column][index], typeof(string));
            return (string)dataColumns[column][index];
         }

         public DateTime AsDateTime(int column, int index)
         {
            CheckType(dataColumns[column][index], typeof(DateTime));
            return (DateTime)dataColumns[column][index];
         }

         #endregion GET-SET

         #region SEARCH

         public int Index(DateTime instant, SearchType searchType = SearchType.FIND_NEAREST)
         {
            return Index(Conversions.InstantToRelative(instant, startInstant, timeUnits), searchType);
         }

         public int Index(double instant, SearchType searchType = SearchType.FIND_NEAREST)
         {
            switch (searchType)
            {
               case SearchType.FIND_EXACTLY:
                  for (int i = 0; i < instants.Count; i++)
                     if ((double)instants[i] == instant)
                        return i;
                  break;                      

               case SearchType.FIND_EXACTLY_OR_PRIOR:
                  for (int i = 0; i < instants.Count; i++)
                  {
                     if ((double)instants[i] == instant) 
                        return i;
                     else if ((double)instants[i] > instant) 
                        return i - 1;
                  }
                  break;
               
               case SearchType.FIND_EXACTLY_OR_NEXT:
                  for (int i = 0; i < instants.Count; i++)
                     if ((double)instants[i] >= instant) 
                        return i;
                  break;            

               case SearchType.FIND_NEXT:
                  for (int i = 0; i < instants.Count; i++)
                     if ((double)instants[i] > instant) 
                        return i;
                  break;
                  
               case SearchType.FIND_PRIOR:
                  for (int i = 0; i < instants.Count; i++)
                     if ((double)instants[i] >= instant) 
                        return i - 1; //if -1, there is no prior instant
                  break;

               case SearchType.FIND_NEAREST:
                  double diff_prior = -1;
                  double diff_next = -1;

                  for (int i = 0; i < instants.Count; i++)
                  {
                     if ((double)instants[i] == instant) 
                        return i;
                     else if ((double)instants[i] < instant) 
                        diff_prior = instant - (double)instants[i];
                     else if ((double)instants[i] > instant) 
                     {
                        if (diff_prior == -1) 
                           return i;
                        else if (diff_prior <= diff_next)
                           return i - 1;
                        else
                           return i;
                     }
                  }

                  break;
            }

            return -1;
         }

         #endregion SEARCH

         #region AUX

         protected bool CheckType(object obj, Type type, bool throwException = true)
         {
            bool result = (obj.GetType() == type);
            if (!result && throwException)
               throw new Exception("Types do not match.");
            return result;
         }

         #endregion

         #region FILE OPERATIONS

         public void Load(FileName file)
         {
            string line;
            string[] seps = new string[1];
            string[] tokens;
            string[] header; 
            
            TextFile ts = new TextFile(file);
            ts.OpenToRead();

            try
            {
               dataColumns.Clear();
               extraLines.Clear();
               instants.Clear();
      
               //Find Header and columns
               while ((line = ts.ReadLine()) != null)
               {
                  line = line.Trim();

                  if (line == "") continue;               
                  if (line[0] == '!') 
                  {
                     extraLines.Add(line);
                     continue;
                  }

                  seps[0] = ":";
                  tokens = line.Split(seps, 2, StringSplitOptions.RemoveEmptyEntries);

                  if (tokens[0].Trim() == "SERIE_INITIAL_DATA") 
                  {                     
                     string[] tseps = { ".", " " };
                     string[] ttokens = tokens[1].Trim().Split(tseps, StringSplitOptions.RemoveEmptyEntries);
                     startInstant = new DateTime((int)float.Parse(ttokens[0]),
                                                 (int)float.Parse(ttokens[1]),
                                                 (int)float.Parse(ttokens[2]),
                                                 (int)float.Parse(ttokens[3]),
                                                 (int)float.Parse(ttokens[4]),
                                                 (int)float.Parse(ttokens[5])); //DateTime.ParseExact(temp, "yyyy M d H m s", null);
                  }
                  else if (tokens[0].Trim() == "TIME_UNITS") 
                  {
                     timeUnits = (TimeUnits)Enum.Parse(typeof(TimeUnits), tokens[1].Trim(), true);
                  }
                  else
                  {
                     seps[0] = " ";
                     tokens = line.Trim().Split(seps, StringSplitOptions.RemoveEmptyEntries);

                     if (tokens[0].Trim() == "<BeginTimeSerie>") 
                     {
                        if (extraLines.Count < 1)
                        {
                           ts.Close();               
                           throw new Exception("The '" + file.FullName + "' is not a valid time series");
                        }

                        header = extraLines.Last().Trim().Split(seps, StringSplitOptions.RemoveEmptyEntries);
                        instants.Header = header[0];
         
                        for (int i = 1; i < header.Length; i++)
                        {
                           Column column = new Column();
                           dataColumns.Add(column);
                           column.Header = header[i];
                        }

                        extraLines.Remove(extraLines.Last());
                        break;
                     }
                     else if (tokens[0].Trim() == "<BeginResidual>")
                     {
                        while ((line = ts.ReadLine()) != null)
                        {
                           if (line.Trim() == "<EndResidual>")
                              break;
                        }

                        if (line == null)
                           throw new Exception("Invalid TimeSeries file.");
                     }
                     else
                     {
                        extraLines.Add(line);
                     }
                  }
               }
      
               int index;
               seps[0] = " ";
      
               line = ts.ReadLine();
               if (line == null)
                  throw new Exception ("Invalid TimeSeries file.");

               tokens = line.Trim().Split(seps, StringSplitOptions.RemoveEmptyEntries);
               if (tokens == null) 
                  throw new Exception("Invalid TimeSeries file.");

               index = AddInstant(double.Parse(tokens[0], CultureInfo.InvariantCulture));
                  
               for (int i = 1; i < tokens.Length; i++)
               {
                  dataColumns[i - 1].ColumnType = GetColumnType(tokens[i]);

                  if (dataColumns[i - 1].ColumnType == typeof(double))
                     dataColumns[i - 1][index] = double.Parse(tokens[i], CultureInfo.InvariantCulture);
                  else
                     dataColumns[i - 1][index] = tokens[i];
               }
                    

               while ((line = ts.ReadLine()) != null)
               {
                  //line = ts.ReadLine();
                  if (line == null) 
                     throw new Exception("Invalid TimeSeries file.");

                  tokens = line.Trim().Split(seps, StringSplitOptions.RemoveEmptyEntries);

                  if (tokens == null) 
                     throw new Exception("Invalid TimeSeries file.");
             
                  if (tokens[0][0] == '<')
                     break;
                  else
                  {
                     index = AddInstant(double.Parse(tokens[0], CultureInfo.InvariantCulture));

                     for (int i = 1; i < tokens.Length; i++)
                        if (dataColumns[i - 1].ColumnType == typeof(double))
                           dataColumns[i - 1][index] = double.Parse(tokens[i], CultureInfo.InvariantCulture);
                        else
                           dataColumns[i - 1][index] = tokens[i];                        
                  }
               }

               while ((line = ts.ReadLine()) != null)
                  extraLines.Add(line);

               ts.Close();     
            }
            catch
            {
               ts.Close();
               throw;
            }
         }

         protected Type GetColumnType(string text) 
         {        
            try
            {
               double data = double.Parse(text, CultureInfo.InvariantCulture);
               return typeof(double);
            }
            catch
            {
            }

            return typeof(string);
         }

         public void Save(FileName file)
         {
            List<int> columnsToSave = new List<int>();
            for (int i = 0; i < dataColumns.Count; i++)
               columnsToSave.Add(i);

            Interval interval = new Interval(0, -1);

            Save(file, columnsToSave, interval);
         }

         public void Save(FileName file, Interval interval)
         {
            List<int> columnsToSave = new List<int>();
            for (int i = 0; i < dataColumns.Count; i++)
               columnsToSave.Add(i);

            Save(file, columnsToSave, interval);
         }

         public void Save(FileName file, List<int> columnsToSave)
         {
            Interval interval = new Interval(0, -1);

            Save(file, columnsToSave, interval);
         }

         public void Save(FileName file, List<int> columnsToSave, Interval interval)
         {
            //Check to see if the interval exists
            if (interval.Start < 0 || interval.Start >= instants.Count || interval.End >= instants.Count)
               throw new Exception("Invalid Interval");

            //Check to see if the columns chosen exists
            foreach (int col in columnsToSave)
               if (col < 0 || col >= dataColumns.Count)
                  throw new Exception("Invalid column index");

            int endRow;
            if (interval.End < 0)
               endRow = instants.Count - 1;
            else
               endRow = interval.End;

            TextFile ts = new TextFile(file);
            ts.OpenNewToWrite();

            string toSave;

            //Save TimeSeries Header
            ts.WriteLines(extraLines);
            ts.WriteLine("SERIE_INITIAL_DATA : " + startInstant.ToString("yyyy M d H m s"));
            ts.WriteLine("TIME_UNITS : " + timeUnits.ToString());
            toSave = timeUnits.ToString();
            foreach(int col in columnsToSave)
               toSave += " " + dataColumns[col].Header;
            ts.WriteLine(toSave);

            //Save Data
            ts.WriteLine("<BeginTimeSerie>");
            for (int row = interval.Start; row <= endRow; row++)
            {
               toSave = ((double)instants[row]).ToString(instants.DataFormat);
               //Save the columns to file
               foreach (int col in columnsToSave)               
                  toSave += " " + string.Format("{0" + dataColumns[col].DataFormat + "}", dataColumns[col][row]);
               ts.WriteLine(toSave);
            }
            ts.WriteLine("<EndTimeSerie>");

            ts.Close();
         }

         #endregion
      }
   }
}