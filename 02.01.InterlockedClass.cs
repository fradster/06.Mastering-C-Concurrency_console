/*
System.Threading.Interlocked for lock-free concurrency - provides methods for atomic operations on shared data - they can be done with a single CPU instruction, so they are uninterruptible by other threads, so classical lock is unnecessery. Here, we compare increment / decrement operations with classical lock, and with Interlocked method (not a massive difference at 6 or 8 threads)
my results (6 threads):
	Simple lock:		2114 ms
	Interlocked:		1946 ms
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
		object counterLock = new object();
		int counter = 0;
		int numOfThreads = 6;
		int numOfIters= (Int32) 1e6;

		//-----------------------------------------------------------
		// thread delegate for simple lock
		ThreadStart SimpleLockProc = delegate () {
			for(int i = 0; i< numOfIters; i++)
			{
				lock (counterLock)
					counter++;
				Thread.SpinWait(100);
				lock (counterLock)
					counter--;
			}
			return;
		};


		//-----------------------------------------------------------
		// thread delegate for Interlocked
		ThreadStart InterLockProc = () => {
			for(int i = 0; i< numOfIters; i++)
			{
				Interlocked.Increment(ref counter);
				Thread.SpinWait(100);
				Interlocked.Decrement(ref counter);
			}
			return;
		};


		//create threads for simple lock
		IEnumerable<Thread> simpleLockThreads = Enumerable.Range(0, numOfThreads)
			.Select(n => new Thread(SimpleLockProc)).ToList();

		var sw= Stopwatch.StartNew();
		foreach(var thread in simpleLockThreads)
			thread.Start();
		foreach(var thread in simpleLockThreads)
			thread.Join();
		sw.Stop();
		Console.WriteLine("Simple lock: counter: {0}, time: {1} ms", counter, sw.ElapsedMilliseconds);


		//create threads for Interlocked
		IEnumerable<Thread> interLockThreads = Enumerable.Range(0, numOfThreads)
			.Select(n => new Thread(InterLockProc)).ToList();

		sw= Stopwatch.StartNew();
		foreach(var thread in interLockThreads)
			thread.Start();
		foreach(var thread in interLockThreads)
			thread.Join();
		sw.Stop();
		Console.WriteLine("Interlock: counter: {0}, time: {1} ms", counter, sw.ElapsedMilliseconds);
	}
}