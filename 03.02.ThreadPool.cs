/*
Instead of creating a thread every time we need one, we ask the thread pool for a worker thread. If it has a thread available, a thread pool returns it to us. When its job is done, it goes back into the thread pool in a suspended state until it is needed again. If we try to acquire more worker threads than the thread pool’s limit, the subsequent requests will be queued and will wait until a  worker thread becomes available.
This will create thread poll worker threads and see how many threads are being allocated at a time, Doesnt work as it should, "At first we almost immediately have nine worker threads, then the counter grows slowly until 35-40..." It actually stays at 1 all the time.
and   then  it goes  back  to 0"
*/

using System;
using System.Threading;
using System.Linq;


//------------------------------------------------------------------
//
static class DataShareEx
{
	//---------------------------------------------------------
	//
	static void Main()
	{
		int _threadCount = 15;// number of threads to acquire
		int _runCount =0;// counter for the current number of working threads req by this code

		for (int i = 0; i < _threadCount; i++)
		{
			ThreadPool.QueueUserWorkItem(s =>
			{
				Interlocked.Increment(ref _runCount);
				Thread.Sleep(3000);
				Interlocked.Decrement(ref _runCount);
			});

			Thread.Sleep(1000);
			while (_runCount > 0)
			{
				Console.WriteLine(_runCount);
				Thread.Sleep(1000);
			}
		}
	}
}