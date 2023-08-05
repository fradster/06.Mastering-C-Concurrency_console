/*
Solving the problem of data sharing (from previous ex.) with locking the object. The result is always 0.;
*/

using System;
using System.Threading;
using System.Linq;

class DataShareEx
{
	static void Main()
	{
		int iterations = 10000;
		int counter = 0;

		var lockFlag = new object();

		//define thread delegate
		ThreadStart proc = () => {
			for (int i= 0; i < iterations; i++)
			{
				lock (lockFlag)
					counter++;
				Thread.SpinWait(100);
				lock (lockFlag)
					counter--;
			}
		};

		//define threads through array
		var threads = Enumerable.Range(0, 4)
			.Select(n => new Thread(proc))
		.ToArray();

		//start the threads
		foreach(var thr in threads)
			thr.Start();
		foreach(var thr in threads)
			thr.Join();

		Console.WriteLine(counter);
	}
}