/*
TPL (Task Parallel Library) - We can take advantage of different parallel programming model implementations that abstract threads and synchronization mechanics and offer some kind of a higher-level API that is much easier to use. Task-based parallelism : task, which is just a piece of synchronously executing code.
If TaskB depends on a result of a TaskA, which is not known up-front, such dependency between tasks is called future or promise. So we state (make a promise) that, at some point in the future, we will run task B as soon as we get the result from task A.
This code is quite ineffective: when task A runs, task B blocks the thread pool.
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
		Action<Task<string>> ActionBDelegate= MethodB;

		var taskA = new Task<string>(TaskADelegate);
		var taskB = new Task(()=> ActionBDelegate(taskA));

		taskA.Start();
		taskB.Start();
		taskB.Wait();//without this, the task B will not wait for the task A to finish in order to obtain the result.
	}


	static Func<string> TaskADelegate = ()=>{
		Console.WriteLine("Task A started");
		Thread.Sleep(1000);
		Console.WriteLine("Task A finished");
		return "A";
	};


	static void MethodB (Task<string> task)
	{
		Console.WriteLine("Task B started");
		Console.WriteLine($"Result of the task A: {task.Result}");//taskA.Result is "A"
		Console.WriteLine("Task B finished");
		return;
	}
}