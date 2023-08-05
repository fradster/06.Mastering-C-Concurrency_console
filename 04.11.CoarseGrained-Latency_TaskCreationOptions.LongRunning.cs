/*
TaskCreationOptions.LongRunning - how to put long tasks in lower priority, so that short running tasks (user interaction) will have lower latnecy, meaning long tasts will run a bit slower but an UI will be more responsive
Lets sumulate the situation where we have a number of long comutational tasks, and number of short-time tasks that represents user interaction. The point is to see how Coarse grained approach influence the response latency to user input, which is sometimes more important tnhan performance. So, we create 24 tasks with long execution time and in each iterartion we measeure an average latency of running the short task.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;


//------------------------------------------------------------------
// Task for a long calculation
static class DataShareEx
{
	//------------------------------------------------------------------
	// Task for a long calculation
	static Task SleepTask (int time2Sleep)
	{
		Thread.Sleep (time2Sleep);
		return Task.CompletedTask;
	}

	//------------------------------------------------------------------
	// A short task that simulates user interaction
	static Task SpinTask (int time2Spin)
	{
		Thread.SpinWait (time2Spin);
		return Task.CompletedTask;
	}


	//---------------------------------------------------------
	//
	static void Main()
	{
		int numOfLongThreads = 24;// numbet of threads for calcualtions (long operation)
		int measureCount = 12;//

		// create long tasks, one by one, and run them simultainasly as they are created. So, number of dimultanous tasks will steadily grow from 1 to 24
		for (int longThreadCount = 0; longThreadCount< numOfLongThreads; longThreadCount++)
		{
			var longTasks = new List<Task>();//task list

			//for all threads created so far, add long calculation task, mark it as a LongRunning
			for (int i = 0; i< longThreadCount; i++)
				longTasks.Add(Task.Factory.StartNew(() => SleepTask(1000), TaskCreationOptions.LongRunning));

			//start measurement
			var sw = Stopwatch.StartNew();

			//for current batch of longThreadCount number of long tasks, create 12 fast tasks, simulating user interaction
			for (int i =0; i< measureCount; i++)
				Task.Factory.StartNew(() => SpinTask(100));

			//stop measurement
			sw.Stop();
			Console.WriteLine ("Long running threads {0}. Average latency {1:0.###} ms",	longThreadCount, (double) sw.ElapsedMilliseconds / measureCount);

			Task.WaitAll(longTasks.ToArray());
		}
	}
}