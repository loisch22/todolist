using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;

namespace ToDoList.Models
{
  public class Category
  {
    private string _name;
    private int _id;
    // private static string _sortType = "date_ascending";

    public Category(string name, int id = 0)
    {
      _name = name;
      _id = id;
    }
    public override bool Equals (System.Object otherCategory)
    {
      if (!(otherCategory is Category))
      {
        return false;
      }
      else
      {
        Category newCategory = (Category) otherCategory;
        return this.GetId().Equals(newCategory.GetId());
      }
    }
    public override int GetHashCode()
    {
      return this.GetId().GetHashCode();
    }
    public string GetName()
    {
      return _name;
    }
    public int GetId()
    {
      return _id;
    }
    // public static void SetSortType(string sortType)
    // {
    //   _sortType = sortType;
    // }
    public void Save()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"INSERT INTO categories (name) VALUES (@name);";
      MySqlParameter name = new MySqlParameter();
      name.ParameterName = "@name";
      name.Value = this._name;
      cmd.Parameters.Add(name);

      cmd.ExecuteNonQuery();
      _id = (int) cmd.LastInsertedId;
    }

    public static void DeleteCategory(int id)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"DELETE FROM categories WHERE id = @thisId;";

      MySqlParameter categoryId = new MySqlParameter();
      categoryId.ParameterName = "@thisId";
      categoryId.Value = id;
      cmd.Parameters.Add(categoryId);

      cmd.ExecuteNonQuery();
      conn.Close();
    }

    public void Delete()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      MySqlCommand cmd = new MySqlCommand("DELETE FROM categories WHERE id = @CategoryId; DELETE FROM categories_tasks WHERE category_id = @CategoryId;", conn);

      MySqlParameter categoryIdParameter = new MySqlParameter();
      categoryIdParameter.ParameterName = "@CategoryId";
      categoryIdParameter.Value = this.GetId();
      cmd.Parameters.Add(categoryIdParameter);

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
      cmd.CommandText = @"DELETE FROM categories;";
      cmd.ExecuteNonQuery();
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public static List<Category> GetAll()
    {
      List<Category> allCategories = new List<Category>{};
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"SELECT * FROM categories;";
      var rdr = cmd.ExecuteReader() as MySqlDataReader;

      while(rdr.Read())
      {
        int CategoryId = rdr.GetInt32(0);
        string CategoryName = rdr.GetString(1);
        Category newCategory = new Category(CategoryName, CategoryId);
        allCategories.Add(newCategory);
      }
      conn.Close();
      return allCategories;
    }

    public static Category Find(int id)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"SELECT * FROM categories WHERE id = (@searchId);";

      MySqlParameter searchId = new MySqlParameter();
      searchId.ParameterName = "@searchId";
      searchId.Value = id;
      cmd.Parameters.Add(searchId);

      var rdr = cmd.ExecuteReader() as MySqlDataReader;
      int CategoryId = 0;
      string CategoryName ="";

      while (rdr.Read())
      {
        CategoryId = rdr.GetInt32(0);
        CategoryName = rdr.GetString(1);
      }
      Category newCategory = new Category(CategoryName,CategoryId);
      conn.Close();
      return newCategory;
    }

    public void AddTask(Task newTask)
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"INSERT INTO categories_tasks (category_id, task_id) VALUES (@CategoryId, @TaskId);"; //inserting into join table

      MySqlParameter category_id = new MySqlParameter();
      category_id.ParameterName = "@CategoryId";
      category_id.Value = _id;
      // just _id because we are in Category.cs and can call on its direct id
      cmd.Parameters.Add(category_id);

      MySqlParameter task_id = new MySqlParameter();
      task_id.ParameterName = "@TaskId";
      task_id.Value = newTask.GetId();
      //Adding task id for the new task
      cmd.Parameters.Add(task_id);

      cmd.ExecuteNonQuery();
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
    }

    public List<Task> GetTasks()
    {
      MySqlConnection conn = DB.Connection();
      conn.Open();
      var cmd = conn.CreateCommand() as MySqlCommand;
      cmd.CommandText = @"SELECT task_id from categories_tasks WHERE category_id = @CategoryId;";
      //Asks for any task_id for which the category_id matches the id of the Category object we're calling the method on (aka returns all tasks for search category)

      MySqlParameter categoryIdParameter = new MySqlParameter();
      categoryIdParameter.ParameterName = "@CategoryId";
      categoryIdParameter.Value = _id;
      cmd.Parameters.Add(categoryIdParameter);

      var rdr = cmd.ExecuteReader() as MySqlDataReader;

      //Stores task ids that match category
      List<int> taskIds = new List<int> {};
      //Reads through tasks that match then adds to list called taskIds
      while (rdr.Read())
      {
        int taskId = rdr.GetInt32(0);
        taskIds.Add(taskId);
      }
      rdr.Dispose();
      //Need to include this because SqlDataReader can only have on open reader

      List<Task> tasks = new List<Task> {};
      //List<Task> is how it knows it is using Task.cs
      foreach (int taskId in taskIds) //reads through the taskIds that were added to the List<int>
      {
        var taskQuery = conn.CreateCommand() as MySqlCommand;
        taskQuery.CommandText = @"SELECT * FROM tasks WHERE id = @TaskId;";
        //return task with that ID and add it to tasks list

        MySqlParameter taskIdParameter = new MySqlParameter();
        taskIdParameter.ParameterName = "@TaskId";
        taskIdParameter.Value = taskId; //Where is this task id coming from?
        taskQuery.Parameters.Add(taskIdParameter);

        var taskQueryRdr = taskQuery.ExecuteReader() as MySqlDataReader;

        while (taskQueryRdr.Read())
        {
          int thisTaskId = taskQueryRdr.GetInt32(0);
          string taskDescription = taskQueryRdr.GetString(1);
          Task foundTask = new Task(taskDescription, thisTaskId);
          tasks.Add(foundTask);
        }
        taskQueryRdr.Dispose();
      }
      conn.Close();
      if (conn != null)
      {
        conn.Dispose();
      }
      return tasks;
    }


    // public List<Task> GetTasks()
    // {
    //   List<Task> allCategoryTasks = new List<Task>{};
    //   MySqlConnection conn = DB.Connection();
    //   conn.Open();
    //   var cmd = conn.CreateCommand() as MySqlCommand;
    //
    //   if (_sortType=="date_ascending")
    //   {
    //       cmd.CommandText =@"SELECT * FROM tasks WHERE category_id = @category_id ORDER BY due_date ASC;";
    //   }
    //   else if (_sortType == "date_descending")
    //   {
    //       cmd.CommandText = @"SELECT * FROM tasks WHERE category_id = @category_id ORDER BY due_date DESC;";
    //   }
    //   else if (_sortType == "alphabetical_order")
    //   {
    //     cmd.CommandText = @"SELECT * FROM tasks WHERE category_id = @category_id ORDER BY description ASC;";
    //   }
    //   else
    //   {
    //     cmd.CommandText = @"SELECT * FROM tasks WHERE category_id = @category_id ORDER BY description DESC;";
    //   }
    //
    //
    //   MySqlParameter categoryId = new MySqlParameter();
    //   categoryId.ParameterName = "@category_Id";
    //   categoryId.Value = this._id;
    //   cmd.Parameters.Add(categoryId);
    //
    //   var rdr = cmd.ExecuteReader() as MySqlDataReader;
    //   while(rdr.Read())
    //   {
    //     int taskId = rdr.GetInt32(0);
    //     string taskDescription = rdr.GetString(1);
    //     int taskCategoryId = rdr.GetInt32(2);
    //     DateTime taskDateTime = rdr.GetDateTime(3);
    //     Task newTask = new Task(taskDescription,taskCategoryId, taskDateTime, taskId);
    //     allCategoryTasks.Add(newTask);
    //   }
    //   return allCategoryTasks;
    // }
  }
}
