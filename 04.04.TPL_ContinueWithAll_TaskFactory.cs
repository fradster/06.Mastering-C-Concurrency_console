/*
Task.Factory.ContinueWhenAll
In case we have several dependecies (A1, A2) for given task (B), we use ContinueWhenAll method. Here, task B needs results both from the A1 and A2.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


//------------------------------------------------------------------
//
static class DataShareEx
{
	//---------------------------------------------------------
	//
	static void Main()
	{
		//method for Task A1
		string ActionA1 ()
		{
			Console.WriteLine("Task A1 started");
			Thread.Sleep(1000);
			Console.WriteLine("Task A1 finished");
			return "A1";
		}

		//Method for the task A2
		string ActionA2()
		{
			Console.WriteLine("Task A2 started");
			Thread.Sleep(500);
			Console.WriteLine("Task A2 finished");
			return "A2";
		}

		//still couldnt find the way to define method for the task where method is void. Thus, defining of the task
		Task taskB(Task<string>[] tsks)
		{
			Console.WriteLine($"Result of the task A1: {tsks[0].Result}\nResult of the task A2: {tsks[1].Result}");
			return Task.CompletedTask;
		}

		var taskA1 = new Task<string>(ActionA1);
		var taskA2 = new Task<string>(ActionA2);
		taskA1.Start(); taskA2.Start();

		var taskArr = new [] {taskA1, taskA2};

		Task.Factory.ContinueWhenAll (taskArr, tasks => taskB(tasks));
		taskA1.Wait();
	}
}