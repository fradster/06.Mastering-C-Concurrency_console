/*
Interlocked.CompareExchange : 3 arguments, compares first to third, if they are equal, swaps the first and second. Returns the original value of the first
*/

using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

static class DataShareEx
{
	//-----------------------------------------------------------
	//
	static void Main()
	{
		int _total=0;// initial value of sum
		int _val2Add= 50;// value to add to sum
		int numOfThreads = 5; //number of thread
		int numOfCycles =60; // number of addition cycles for each thread

		//-----------------------------------------------------------
		// thread delegate. will try to add current value to _total, but only if _total is not changed meanwhile (by some other thread). CompareExchange returns the initial value of _total, so if its not the same as beforeValue, it will try to add current again to a new value of _total. Repeat the whole thing for a given number of cycles
		ThreadStart SumProc = () =>
		{
			int beforeValue, newValue;

			for (int i= 0; i< numOfCycles; i++)
			{
				do
				{
					beforeValue = _total;
					newValue = beforeValue + _val2Add;
				}
				while	(beforeValue != Interlocked.CompareExchange(ref _total, newValue, beforeValue));
			}
			return;
		};


		//create 6 threads
		var threads = Enumerable.Range (0, numOfThreads)
			.Select(n => new Thread(SumProc)).ToList();

		foreach(var thr in threads)
			thr.Start();
		foreach(var thr in threads)
			thr.Join();

		var expectedValue = numOfCycles * numOfThreads * _val2Add;

		Console.WriteLine("Expected value of the Total: {0}", expectedValue);
		Console.WriteLine("Calculated value of the Total: {0}", _total);
	}
}