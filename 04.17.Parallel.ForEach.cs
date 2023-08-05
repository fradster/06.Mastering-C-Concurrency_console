/*
	Parallel.ForEach.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;


//------------------------------------------------------------------
//
static class DataShareEx
{

	//---------------------------------------------------------
	// performs some time consuming task
	private static long Calc()
	{
		long total = 0;
		for (int i = 1; i < 1e8; i++)
			total += i;

		return total;
	}


	//---------------------------------------------------------
	//
	static void Main()
	{
		Stopwatch sw = new Stopwatch();

		///first, do the loop synchoniolsly
		Console.WriteLine("Standard Foreach Loop Started");

		var integers = Enumerable.Range(1, 10);
		List<int> taskIds = new List<int> ();

		sw.Start();
		foreach (int i in integers)
		{
			long total = Calc();
			taskIds.Add(Task.CurrentId ?? 0);// add current task to the list of tasks

			Console.WriteLine($"{i} - {total}");
		}
		sw.Stop();

		//make the list of tasks Ids unique
		var taskDistincts = taskIds.Distinct().Select(i => i != 0);
		int numOfTasks = taskDistincts.Count();

		Console.WriteLine($"Time Taken by Standard Foreach Loop {sw.ElapsedMilliseconds} ms\nNumber of deployed tasks: {numOfTasks}");

		///now, do the loop in parallel threads
		Console.WriteLine("\nParallel Foreach Loop Started");

		taskIds.Clear();

		sw.Reset();sw.Start();

		Parallel.ForEach(integers, i=> {
			long total = Calc();
			taskIds.Add(Task.CurrentId ?? 0);// add current task to the list of tasks

			Console.WriteLine($"{i} - {total}");
		});
		sw.Stop();

		taskDistincts = taskIds.Distinct().Select(i => i != 0);
		numOfTasks = taskDistincts.Count();

		Console.WriteLine($"Time Taken by Parallel Foreach Loop {sw.ElapsedMilliseconds} ms\nNumber of deployed tasks: {numOfTasks}");
	}
}