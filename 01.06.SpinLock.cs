/*
To avoid unnecessary context switches in such a situation, we can use a loop, which checks the other locks in each iteration. It doesnt use too much CPU, and we have a significant performance boost by not using the operating system resources. It’s not encouraged to use a spinlock directly unless you are 100% sure what you’re doing. Make sure that you have confirmed the performance bottleneck with tests and you know that your locks are really short. Spinlock uses a Memory barrier by default (memory fencing to notify other threads that lock has been released). It improves the fairness of the lock at the expense of performance.
This will compare simple lock vs spin lock with memory barrier vs spinLock without memory barrier, while calculating sin 1e7 times and adding it to collection
my results:
	Simple lock:							2582 ms
	spinlock with memory barrier:		2375 ms
	spinlock without memory barrier:	2278 ms
*/

using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

static class DataShareEx
{
	//define time and counting constrains
	private const int _count = 10000000;

	//-----------------------------------------------------------
	//
	static void Main()
	{

		//target collection
		var map = new Dictionary<double, double>();

		// Warm up
		var r	= Math.Sin(0.01);

		// simple lock, it will lock 1e7 times (_count), will measure the time for the lock, calculating sine, and add it to collection
		map.Clear();
		var lockFlag = new object();
		var prm = 0d;
		var sWatch1 = Stopwatch.StartNew();

		for(int i= 0; i< _count; i++)
		{
			var sintmp = Math.Sin(prm);
			lock(lockFlag)
				map.Add(prm, sintmp);
			prm += 0.01;
		}
		sWatch1.Stop();
		Console.WriteLine("simple Lock:  {0}ms", sWatch1.ElapsedMilliseconds);


		// spin lock with memory barrier, it will lock 1e7 times (_count), will measure the time for the lock, calculating sine, and add it to collection
		map.Clear();
		var spinLock = new SpinLock();
		prm = 0d;
		sWatch1 = Stopwatch.StartNew();

		for(int i= 0; i< _count; i++)
		{
			bool isLock = false;
			var sintmp = Math.Sin(prm);

			try{spinLock.Enter(ref isLock); map.Add(prm, sintmp);}
			finally{if(isLock) spinLock.Exit(true);}

			prm += 0.01;
		}
		sWatch1.Stop();
		Console.WriteLine("Spinlock with memory barrier:  {0}ms", sWatch1.ElapsedMilliseconds);


		// spin lock without memory barrier -  spinLock.Exit(False)
		map.Clear();
		prm = 0d;
		sWatch1 = Stopwatch.StartNew();

		for(int i= 0; i< _count; i++)
		{
			bool isLock = false;
			var sintmp = Math.Sin(prm);

			try{spinLock.Enter(ref isLock); map.Add(prm, sintmp);}
			finally{if(isLock) spinLock.Exit(false);}

			prm += 0.01;
		}

		sWatch1.Stop();
		Console.WriteLine("Spinlock without memory barrier:  {0}ms", sWatch1.ElapsedMilliseconds);
	}
}