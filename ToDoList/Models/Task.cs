using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;

namespace ToDoList.Models
{
  public class Task
  {
    private string _description;
    private int _id;

    public Task (string Description, int id = 0)
    {
      _description = Description;
      _id = id;
    }

    public override bool Equals(System.Object otherTask)
    {
      if (!(otherTask is Task))
      {
        return false;
      }
      else
      {
        Task newTask = (Task) otherTask;
        bool idEquality = (this.GetId() == newTask.GetId());
        bool descriptionEquality = (this.GetDescription() == newTask.GetDescription());
        return (idEquality && descriptionEquality);
      }
    }
    public override int GetHashCode()
    {
      return this.GetDescription().GetHashCode();
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

    public void Save()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();

      //Create the SqlCommand query with parameters - like MySqlCommand - cmd.CommandText
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"INSERT INTO `tasks` (`description`) VALUES (@TaskDescription);";        //@TaskDescription is a placeholder - use placeholders whenever we enter data that a user enters

      //Declare a MySqlParameter object and assign values:
      MySqlParameter description = new MySqlParameter();
        //Create a MySqlParameter object for each parameter we use in our MySqlCommand
      description.ParameterName = "@TaskDescription";
        //ParameterName needs to match the parameter in the command string (@TaskDescription)
      description.Value = this._description;
        //Value is what will replace the parameter in the command string with it is executed
      cmd.Parameters.Add(description);
        //Add the SqlParameter object to the SqlCommand object's Parameters property
        //If there were more paramenters to add, we would need to Add each one

      cmd.ExecuteNonQuery();
      _id = (int) cmd.LastInsertedId;
        //Collect value of id that database assigns to a new entry - we want to collect that valuel and update oru 'object-in-memory' so that it will match the 'object-in-database'
        //put (int) before to force data type to become int32
        conn.Close();
        if (conn != null)
        {
          conn.Dispose();
        }
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
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return allTasks;
    }

    public static Task Find(int id)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"SELECT * FROM tasks WHERE id = @searchId;";

      MySqlParameter searchId = new MySqlParameter();
      searchId.ParameterName = "@searchId";
      searchId.Value = id;
      cmd.Parameters.Add(searchId);

      var rdr = cmd.ExecuteReader() as MySqlDataReader;
      int taskId = 0;
      string taskDescription = "";

      while (rdr.Read())
      {
        taskId = rdr.GetInt32(0);
        taskDescription = rdr.GetString(1);
      }
      Task newTask = new Task(taskDescription, taskId);

      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return newTask;
    }

    public void AddCategory(Category newCategory)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"INSERT INTO categories_tasks (category_id, task_id) VALUES (@CategoryId, @TaskId);";

      MySqlParameter category_id = new MySqlParameter();
      category_id.ParameterName = "@CategoryId";
      category_id.Value = newCategory.GetId();
      cmd.Parameters.Add(category_id);

      MySqlParameter task_id = new MySqlParameter();
      task_id.ParameterName = "@TaskId";
      task_id.Value = _id;
      cmd.Parameters.Add(task_id);

      cmd.ExecuteNonQuery();
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public List<Category> GetCategories()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"SELECT category_id FROM categories_tasks WHERE task_id = @taskId;";

      MySqlParameter taskIdParameter = new MySqlParameter();
      taskIdParameter.ParameterName = "@taskId";
      taskIdParameter.Value = _id;
      cmd.Parameters.Add(taskIdParameter);

      var rdr = cmd.ExecuteReader() as MySqlDataReader;

      List<int> categoryIds = new List<int> {};
      while(rdr.Read())
      {
        int categoryId = rdr.GetInt32(0);
        categoryIds.Add(categoryId);
      }
      rdr.Dispose();

      List<Category> categories = new List<Category> {};
      foreach (int categoryId in categoryIds)
      {
        var categoryQuery = conn.CreateCommand() as MySqlCommand;
        categoryQuery.CommandText = @"SELECT * FROM categories WHERE id = @CategoryId;";

        MySqlParameter categoryIdParameter = new MySqlParameter();
        categoryIdParameter.ParameterName = "@CategoryId";
        categoryIdParameter.Value = categoryId; //refers to category id found that matched taskid line 166, 184
        categoryQuery.Parameters.Add(categoryIdParameter);

        var categoryQueryRdr = categoryQuery.ExecuteReader() as MySqlDataReader;
        while(categoryQueryRdr.Read())
        {
          int thisCategoryId = categoryQueryRdr.GetInt32(0);
          string categoryName = categoryQueryRdr.GetString(1);
          Category foundCategory = new Category(categoryName, thisCategoryId);
          categories.Add(foundCategory);
        }
        categoryQueryRdr.Dispose();
      }
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return categories;
    }

    public void UpdateDescription(string newDescription)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"UPDATE tasks SET description = @newDescription WHERE id = @searchId;";

      MySqlParameter searchId = new MySqlParameter();
      searchId.ParameterName = "@searchId";
      searchId.Value = _id;
      cmd.Parameters.Add(searchId);

      MySqlParameter description = new MySqlParameter();
      description.ParameterName = "@newDescription";
      description.Value = newDescription;
      cmd.Parameters.Add(description);

      cmd.ExecuteNonQuery();
      _description = newDescription;
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public void Delete()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();

      MySqlCommand cmd = new MySqlCommand("DELETE FROM tasks WHERE id = @TaskId; DELETE FROM categories_tasks WHERE task_id = @TaskId;", conn);

      MySqlParameter taskIdParameter = new MySqlParameter();
      taskIdParameter.ParameterName = "@TaskId";
      taskIdParameter.Value = this.GetId();
      cmd.Parameters.Add(taskIdParameter);

      cmd.ExecuteNonQuery();
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public static void DeleteAll()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();

      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"DELETE FROM tasks;";
      cmd.ExecuteNonQuery();
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    // public static Task Find(int searchId)
    // {
    //   return _instances[searchId-1];
    // }
  }
}
