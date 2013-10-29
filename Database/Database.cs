using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

using Mohid.Core;

namespace Mohid
{
   namespace Databases
   {
      public enum DBCommandType
      {
         QUERY,
         INSERT,
         UPDATE,
         DELETE
      }

      public class Database
      {
         protected Exception exception;
         protected OdbcConnection dbConn;
         //protected OdbcCommand command;
         //protected OdbcDataReader reader;
         //protected int affectedRows;

         public Exception RaisedException { get { return exception; } }
         //public int AffectedRows { get { return affectedRows; } }

         public Database()
         {
            exception = null;
            dbConn = null;
            //command = null;
         }

         public Result Connect(string connectionString)
         {
            try
            {
               dbConn = new OdbcConnection(connectionString);
               dbConn.Open();

               //command = dbConn.CreateCommand();

               exception = null;
               return Result.TRUE;
            }
            catch(Exception ex)
            {
               exception = ex;
               return Result.EXCEPTION;
            }
         }

         public void Disconnect()
         {
            try
            {
               if (dbConn != null && dbConn.State != System.Data.ConnectionState.Closed)
               {
                  dbConn.Close();
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message);
            }
         }

         public OdbcDataReader ExecuteQuery(string queryStr)
         {
            try
            {
               OdbcCommand command = new OdbcCommand(queryStr, dbConn);
               command.CommandType = System.Data.CommandType.Text;               
               return command.ExecuteReader();
            }
            catch (Exception ex)
            {
               exception = ex;
               return null;
            }
         }

         public int Count(string tableName, string queryStr)
         {
            try
            {               
               string command_str = "SELECT COUNT(*) FROM " + tableName;
               if (!string.IsNullOrWhiteSpace(queryStr))
                  command_str += " WHERE " + queryStr;

               OdbcCommand command = new OdbcCommand(command_str, dbConn);
               command.CommandType = System.Data.CommandType.Text;               
               int count = (int)command.ExecuteScalar();
               command = null;
               return count;
            }
            catch (Exception ex)
            {
               exception = ex;
               return -1;
            }
         }

         public OdbcDataReader ExecuteQuerySingleRow(string queryStr)
         {
            try
            {
               OdbcCommand command = new OdbcCommand(queryStr, dbConn);
               command.CommandType = System.Data.CommandType.Text;
               
               return command.ExecuteReader(System.Data.CommandBehavior.SingleRow);
            }
            catch (Exception ex)
            {
               exception = ex;
               return null;
            }
         }

         public int ExecuteCommand(string cmdStr)
         {
            try
            {
               OdbcCommand command = new OdbcCommand(cmdStr, dbConn);
               command.CommandType = System.Data.CommandType.Text;
               
               return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
               exception = ex;
               return -1;
            }
         }
      }
   }
}