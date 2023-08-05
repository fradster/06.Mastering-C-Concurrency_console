/*
Basic example of threads that only count iterations, with one thread having lock replaced with Monitor.TryEnter method.
Why is order of locks reversed in Monitor.EnterTry in comparison to lock method? In lock method, it seems that bLock is nested within aLock, while in Monitor method, it looks reversed. Changin the order still results in success.
*/

using System;
using System.Threading;
using System.Linq;

class DataShareEx
{

	static void Main()
	{
		int iterations = 10;

		var aLock = new object();
		var bLock = new object();

		//define thread and ThreadStart delegate inline
		var thread1 = new Thread (() => LockTimeOut1(aLock, bLock, iterations));

		//define thread2
		var thread2 = new Thread(() => LockTimeOut2(aLock, bLock, iterations));

		//start and join threads
		thread1.Start(); thread2.Start();
		thread1.Join(); thread2.Join();

		Console.WriteLine("Done");
	}


	//function for the thread1, usling lock
	static void LockTimeOut1 (object aLock, object bLock, int iterations)
	{
		for (int i= 0; i < iterations; i++)
		{
			//bLock is a nested lock within aLock
			lock (aLock)
				lock (bLock)
					Thread.SpinWait(100);
		}
		return;
	}


	//function for the thread2, using Monitor.EnterTry and Monitor.Exit
	static void LockTimeOut2 (object aLock, object bLock, int iterations)
	{
		bool isBAccquired = false, isAAccquired = false;
		const int waitSec = 5, retryCount = 3;

		for (int i= 0; i< iterations; i++)
		{
			//will try to lock bLock and nested aLock up to 3 times by 5 seconds.
			int retries= 0;
			while (retries < retryCount)
			{
				//Console.WriteLine($"retry: {retries+1}");

				//try to lock bLock, then aLock if Block was successfull. ALso release aLock in case of success. Othrewisae oncrease num of retires
				try
				{
					isBAccquired = Monitor.TryEnter(bLock, TimeSpan.FromSeconds(waitSec));
					//Console.WriteLine($"is bLock Accquired: {isBAccquired}");

					//if lock was successfull, try to accquire aLock the same way. If that succesedes, wait for 100 (ms?) and break the whilw loop. if lock was successfull, release it
					if(isBAccquired)
					{
						try
						{
							isAAccquired = Monitor.TryEnter(aLock, TimeSpan.FromSeconds(waitSec));
							//Console.WriteLine($"is aLock Accquired: {isAAccquired}");

							if(isAAccquired) {Thread.SpinWait(100); break;}
							else retries++;
						}
						finally {if(isAAccquired) Monitor.Exit(aLock);}
					}
					else retries++;
				}
				finally {if(isBAccquired) Monitor.Exit(bLock);}

			}
			if(retries >= retryCount)
				Console.WriteLine("Could not obtain locks");
		}
		return;
	}
}