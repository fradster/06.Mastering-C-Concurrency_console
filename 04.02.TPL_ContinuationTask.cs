/*
Task.ContiueWith
The 04.01 example (task A and B are running at the same time, taskB waits for task A to finish) is inefficient. Another way is to not schedule task B code execution until task A code finishes and returns its result. We need to do is to declare a dependency between tasks explicitly, so TPL will know what tasks to run first and what to delay
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
		var taskA = new Task<string> (TaskADelegate);
		taskA.ContinueWith (ContTask);

		taskA.Start();
		taskA.Wait();
	}

	static Func<string> TaskADelegate = ()=>{
		Console.WriteLine("Task A started");
		Thread.Sleep(1000);
		Console.WriteLine("Task A finished");
		return "A";
	};

	static Action<Task<string>> ContTask = (Task<string> taskA) =>
	{
		Console.WriteLine("Task B started");
		Console.WriteLine($"Result of the task A: {taskA.Result}");//taskA.Result is the result of the task A
		return;
	};
}