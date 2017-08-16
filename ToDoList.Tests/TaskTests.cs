using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Models;

namespace ToDoList.TestTools
{
  [TestClass]
  public class TaskTests : IDisposable
  {
    public void Dispose()
    {
      Task.DeleteAll();
    }
    public TaskTests()
    {
      DBConfiguration.ConnectionString = "server=localhost;user id =root;password=root;port=8889;database=todo_test"
    }

    [TestMethod]
    public void GetAll_DatabaseEmptyAtFirst_0()
    {
      //Arrange, Act
      int result = Task.GetAll().Count;

      //Assert
      Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void Equals_ReturnsTrueIfDescriptionsAreTheSame_Task()
    {
      Task firstTask = new Task("Mow the lawn");
      Task secondTask = new Task("Mow the lawn");

      Assert.AreEqual(firstTask, secondTask);
    }

    [TestMethod]
    public void Save_SavesToDatabase_TaskList()
    {
      Task testTask = new Task("Mow the lawn");

      testTask.Save();
      List<Task> result = Task.GetAll();
      List<Task> testList = new List<Task>{testTask};

      CollectionAssert.AreEqual(testList, result);
    }

    [TestMethod]
    public void Save_AssignsIdToObject_Id()
    {
        Task testTask = new Task("Mow the lawn");

        testTask.Save();
        Task savedTask = Task.GetAll()[0];

        int result = savedTask.GetId();
        int testId = testTask.GetId();

        Assert.AreEqual(testId, result);
    }
  }
}
