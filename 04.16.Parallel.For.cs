/*
	Parallel.For - create parallel loops. It shows how many threads TPL will reserve for this parallel calculation depending on number of parallel tasks required. Results:
	When required less than 6 (on a 6-core CPU), th TPL will
		1 iterations,	1 tasks
		2 iterations,	2 tasks
		4 iterations,	4 tasks
		8 iterations,	6 tasks
		16 iterations,	7 tasks
		32 iterations,	6 tasks
	When required less than 6 parallel iterations(on a 6-core CPU), the TPL will reserve the same number of threads. When num of iters is slighly > 6 (8), TPL will also reserve 6 trhreads. When significatly higher, the TPL will employ onemore thread to ensure that there is no idle CPU time. Why is number of threads lowered to 6 with 32 iterations? Dont know that. But, number of reserved threads will vary as you restart the program.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
//using System.Diagnostics;
using System.Collections.Generic;


//------------------------------------------------------------------
// action for a task, will wait for a 1000sec, then lock taskIDs list and add current task id
static class DataShareEx
{
	private static void Action1 (HashSet<int> taskIDs, int taskId)
	{
		Thread.SpinWait((int) 1e6);

		lock(taskIDs)
			taskIDs.Add(taskId);

		return;
	}

	//---------------------------------------------------------
	// performs parallel for loop
	private static void Calc(int iters)
	{
		// list of task ids
		var taskIds = new HashSet<int>();
		///var sum = 0;

		Parallel.For (0, iters,
			i => Action1(taskIds, Task.CurrentId.Value)
		);

		Console.WriteLine("{0} iterations, {1} tasks", iters, taskIds.Count);
	}


	//---------------------------------------------------------
	//
	static void Main()
	{
		var iteratons = Enumerable.Range(0, 6).Select (it=>
			(it == 0)? 1 : 2 << (it-1));

		foreach(int it in iteratons)
			Calc(it);
	}
}