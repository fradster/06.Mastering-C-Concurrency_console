/*
	Parallel.Invoke - execute parallel actions if CPU has multiple cores. Blocks the calling thread until all actions are completed. Here, we generate 3 actions, one spins for 10 seconds before printing action no.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
//using System.Diagnostics;
//using System.Collections.Generic;


//------------------------------------------------------------------
// Task for a long calculation.
static class DataShareEx
{
	//---------------------------------------------------------
	//
	static void Main()
	{
		Parallel.Invoke (
			() => Console.WriteLine("Action 1"),
			() => {Thread.SpinWait(10000); Console.WriteLine("Action 2"); return;},
			() => Console.WriteLine("Action 3")
		);

		Console.WriteLine("End");
	}
}