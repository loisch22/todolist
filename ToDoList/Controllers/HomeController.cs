using Microsoft.AspNetCore.Mvc;
using ToDoList.Models;
using System.Collections.Generic;

namespace ToDoList.Controllers
{
  public class HomeController : Controller
  {
    [HttpGet("/")]
    public ActionResult Index()
    {
      return View();
    }

    [HttpGet("/tasks")]
    public ActionResult Tasks()
    {
      List<Task> allTasks = Task.GetAll();
      return View(allTasks);
    }

    [HttpGet("/categories")]
    public ActionResult Categories()
    {
      List<Category> allCategories = Category.GetAll();
      return View(allCategories);
    }

    [HttpGet("/tasks/new")]
    public ActionResult TaskForm()
    {
      return View();
    }

    [HttpPost("/tasks/new")]
    public ActionResult TaskCreate()
    {
      Task newTask = new Task(Request.Form["task-description"]);
      newTask.Save();

      return View("Success");
    }

    [HttpGet("/categories/new")]
    public ActionResult CategoryForm()
    {
      return View();
    }

    [HttpPost("/categories/new")]
    public ActionResult CategoryCreate()
    {
      Category newCategory = new Category(Request.Form["category-name"]);
      newCategory.Save();

      return View("Success");
    }

//ONE TASK
    [HttpGet("/tasks/{id}")]
    public ActionResult TaskDetail(int id)
    {
        Dictionary<string, object> model = new Dictionary<string, object>();

        Task selectedTask = Task.Find(id);
        List<Category> TaskCategories = selectedTask.GetCategories();

        List<Category> AllCategories = Category.GetAll();
        model.Add("task", selectedTask);
        model.Add("taskCategories", TaskCategories);
        model.Add("allCategories", AllCategories); //Why do you need all categories? So user can select which category the new task belongs to

        return View(model);
    }

//ONE CATEGORY
    [HttpGet("/categories/{id}")]
    public ActionResult CategoryDetail(int id)
    {
      Dictionary<string, object> model = new Dictionary<string, object>();

      Category SelectedCategory = Category.Find(id);
      List<Task> CategoryTasks = SelectedCategory.GetTasks();
      List<Task> AllTasks = Task.GetAll();
      model.Add("category", SelectedCategory);
      model.Add("categoryTasks", CategoryTasks);
      model.Add("allTasks", AllTasks);
      return View(model);
    }

//ADD CATEGORY TO TASK
    [HttpPost("task/add_category")]
    public ActionResult TaskAddCategory()
    {
      Category category = Category.Find(int.Parse(Request.Form["category-id"]));
      //Hidden input gives above value as category.GetId()
      Task task = Task.Find(int.Parse(Request.Form["task-id"]));
      //Shows user description but value is the task.GetId()
      task.AddCategory(category);
      return View("Success");
    }

//ADD TASK TO CATEGORY
    [HttpPost("category/add_task")]
    public ActionResult CategoryAddTask()
    {
      Category category = Category.Find(int.Parse(Request.Form["category-id"]));
      Task task = Task.Find(int.Parse(Request.Form["task-id"]));
      category.AddTask(task);
      return View("Success");
    }

    // [HttpPost("/tasks")]
    // public ActionResult AddTask()
    // {
    //   Task newTask = new Task(Request.Form["new-task"]);
    //   List<Task> allTasks = Task.GetAll();
    //   return View("Tasks", allTasks);
    // }
    //
  }
}
