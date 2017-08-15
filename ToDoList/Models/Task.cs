using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;

namespace ToDoList
{
  public class Task
  {
    private string _description;
    private int _id;

    public Task (string Description, int Id = 0)
    {
      _description = Description;
      _id = Id;
    }
    public string GetDescription()
    {
      return _description;
    }
    public void SetDescription(string newDescription)
    {
      _description = newDescription;
    }
    public int GetId()
    {
      return _id;
    }
    public static List<Task> GetAll()
    {
      List<Task> allTasks = new List<Task> {};
      MySqlConnection conn = DB.Connection();
      conn.Open(); //MySqlConnection represents the database using the connection information that we set it to - instantiate MySqlConnection to object named conn and set it to the connection string DB.Connection stored in StartUp class
      var cmd = conn.CreateCommand() as MySqlCommand; //casts our command into a MySqlCommand object
      cmd.CommandText = @"SELECT * FROM tasks;";
      var rdr = cmd.ExecuteReader() as MySqlDataReader;
      //the 'data reader object' aka rdr; interacts directly with our database; rdr represents actual reading of SQL database, MySqlDataReader class is its 'type' - this type contains a Read() method that sends the SQL Commands to the database and collects the output of the database

      while(rdr.Read()) //takes each row of data from the database and perform actions on it
      {
        int taskId = rdr.GetInt32(0); //correspond to the index positions within each row of data within the table that we want to collect - this is row 1 or index 0
        string taskName = rdr.GetString(1); //this is row 2 of the table or index 1 
        Task newTask = new Task(taskName, taskId);
        allTasks.Add(newTask);
      }
      return allTasks;
    }

    public static void ClearAll()
    {
      _instances.Clear();
    }
    public static Task Find(int searchId)
    {
      return _instances[searchId-1];
    }
  }
}
