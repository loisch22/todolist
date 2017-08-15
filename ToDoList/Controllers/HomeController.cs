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

    [HttpGet("/tasks/new")]
    public ActionResult TaskForm()
    {
      return View();
    }

    [HttpPost("/tasks")]
    public ActionResult AddTask()
    {
      Task newTask = new Task(Request.Form["new-task"]);
      List<Task> allTasks = Task.GetAll();
      return View("Tasks", allTasks);
    }

    [HttpGet("/tasks/{id}")]
    public ActionResult TaskDetail(int id)
    {
        Task task = Task.Find(id);
        return View(task);
    }

  }
}
