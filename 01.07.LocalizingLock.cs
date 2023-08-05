/*
Shared data minimization - instead of locking the whole collection, we can just lock the certain members. Also, code within the lock should be as mimimal as ut can be, i.e. calculation can be sometimes performed outside of the lock. This will compare calculation over the certain range of elements in 3 variances: 1. calulation is within the lock and the whole collection is locked, 2. calc is outside the lock and the whole collection is locked, 3. calulation is outside the lock and only given range is locked
my results:
	Simple lock:					590 ms
	MinimizedLock:					534 ms
	MinimizedSharedDataLock:	227 ms
*/

using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

static class DataShareEx
{
	//define time and counting constrains
	private const int _count = 1000000;
	private const int _threadCount = 8;

	private static readonly List<string> _resList= new List<string> ();

	//-------------------------------------------------------
	// calculating function
	private static string Calc (int prm)
	{
		Thread.SpinWait(100); return prm.ToString();
	}


	//-------------------------------------------------------
	// function that perorms a calucation on each element of given range, staring from startIndic and counting count. It will perform a calculation inside a lock and will lock the whole collection
	private static void SimpleLock(int startIndic, int count)
	{
		for(int i = startIndic; i <  startIndic + count; i++)
		{
			lock(_resList)
				_resList.Add(Calc(i));
		}
		return;
	}


	//-------------------------------------------------------
	// function that perorms a calucation on each element of given range, staring from startIndic and counting count. It will perform a calculation outside of the lock and will lock the whole collection
	private static void MinimizedLock(int startIndic, int count)
	{
		for(int i = startIndic; i <  startIndic + count; i++)
		{
			var calcVal = Calc(i);
			lock(_resList)
				_resList.Add(calcVal);
		}
		return;
	}


	//-------------------------------------------------------
	// function that perorms a calucation on each element of given range, staring from startIndic and counting count. It will perform a calculation outside of the lock and will lock only the range being added to collection, by using a temporary local collection
	private static void MinimizedSharedDataLock(int startIndic, int count)
	{
		List<string> tempList= new List<string> ();

		for(int i = startIndic; i <  startIndic + count; i++)
		{
			var calcVal = Calc(i);
			tempList.Add(calcVal);
		}

		lock (_resList)
			_resList.AddRange(tempList);

		return;
	}


	//-------------------------------------------------------
	// Measurte the performance of given action, simultaneously ran by several threads
	private static long Measure(Func<int, ThreadStart> actionCreator)
	{
		_resList.Clear();
		var threads = Enumerable.Range(0, _threadCount)
			.Select(n => new Thread(actionCreator(n))).ToArray();

		var sw =  Stopwatch.StartNew();
		foreach (var thread in threads)
			thread.Start();

		foreach (var thread in threads)
			thread.Join();

		sw.Stop();
		return sw.ElapsedMilliseconds;
	}




	//-----------------------------------------------------------
	//
	static void Main()
	{

		//	Warm	up
		SimpleLock(1, 1);
		MinimizedLock(1, 1);
		MinimizedSharedDataLock(1, 1);

		const int part = _count / _threadCount;

		var time = Measure(n => () => SimpleLock(n*part, part));
		Console.WriteLine("Simple lock: {0}ms", time);

		time = Measure(n => () => MinimizedLock(n * part, part));
		Console.WriteLine("Minimized lock: {0}ms", time);

		time = Measure(n => () => MinimizedSharedDataLock(n * part, part));
		Console.WriteLine("Minimized shared data: {0}ms",  time);
	}
}