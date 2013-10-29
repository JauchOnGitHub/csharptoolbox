using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mohid
{
   namespace Core
   {
      public class Column : List<object>
      {
         protected string header;
         protected int index;
         protected object f_defaultValue;

         private Type columnType;

         public string DataFormat;

         public virtual void Init()
         {
            DataFormat = "";
            index = -1;
            header = "";
            f_defaultValue = null;
         }

         public Column()
            : base()
         {
            Init();
         }

         public Column(Type type) 
            : base()
         {
            columnType = type;

            Init();
         }

         public Column(Type type, int index, string header) 
            : base ()
         {
            columnType = type;

            Init();

            this.index = index;
            this.header = header;
         }
         public Column(Column toCopyFrom)
         {
            Init();

            this.index = toCopyFrom.index;
            this.header = toCopyFrom.header;
            this.columnType = toCopyFrom.columnType;
            this.DataFormat = toCopyFrom.DataFormat;
         }

         public string Header
         {
            get 
            {
               if (!string.IsNullOrWhiteSpace(header))
                  return header;
               else if (index >= 0)
                  return "Column " + (index + 1).ToString();
               else
                  return "";                  
            }
            set { header = value; }
         }

         public int Index
         {
            get { return index; }
            set { index = value; }
         }

         public Type ColumnType
         {
            get { return columnType; }
            set { columnType = value; }
         }

         public object DefaultValue
         {
            get { return f_defaultValue; }
            set { f_defaultValue = value; }
         }
      }
   }
}