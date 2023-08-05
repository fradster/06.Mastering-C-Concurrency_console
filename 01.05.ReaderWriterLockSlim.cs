/*
The System.Threading.ReaderWriterLockSlim is modern variant of ReaderWriterLock library, which is obsolete. Helps to manage locking regular collections (dictionary). This example mesures performance of read-write locking of Dictionary in three different ways: by simple lock, by oboslete ReaderWriterLock method, and by using ReaderWriterLockSlim.
Simple lock should be the slowest, and ReaderWriterLockSlim the fastest one. Not with me, here are my results:
	Simple lock: 274ms
	ReaderWriterLock: 451ms
	ReaderWriterLockSlim: 334ms
*/

using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

static class DataShareEx
{
	//define time and counting constrains
	private const int _readersCount = 5;
	private const int _writersCount = 1;
	private const int _readPayload = 100;
	private const int _writePayload = 100;
	private const int _count = 100000;

	//target collection
	private static readonly Dictionary<int, string> _map = new Dictionary<int, string>();


	//define lock object for simple lock
	private static readonly object _simpleLockLock = new object();

	//define object for the obsolete (pre .Net 3.5) ReaderWriterLock method
	private static readonly ReaderWriterLock _rwLock = new ReaderWriterLock();

	//define object for the ReaderWriterLockSlim method
	private static readonly ReaderWriterLockSlim _rwLockSlim = new ReaderWriterLockSlim();


	//-----------------------------------------------------------
	// Tries to read the value of the random key of the _map dictionary. The key is the remainder of division of system ticks (ms) by 100000 (_count). String val is read value. If the key is not found, TryGetValue will return default value for the string
	private static void ReaderProc()
	{
		string val;

		_map.TryGetValue(Environment.TickCount % _count, out val);
		// Do some work
		Thread.SpinWait (_readPayload);
		return;
	}


	//-----------------------------------------------------------
	// Writes new value in dictinary. Both key and value are the same: n is the remainder of division of system ticks (ms) by 100000 (_count)
	private static void WriterProc()
	{
		var n = Environment.TickCount % _count;
		// Do some work
		Thread.SpinWait(_writePayload);
		_map[n] = n.ToString();
		return;
	}


	//-----------------------------------------------------------
	// Define threads, start them, measure the elapsed miliseconds while all threads are done
	private static long Measure(Action reader, Action writer)
	{
		//creates 5 threads (_readersCount) where each thread calls reader() - reads random key of the map, and one (_writersCount) thread to call writer() (writes random num into random key of the distionary)
		var threads = Enumerable.Range(0, _readersCount)
		.Select (n => new Thread(() => {
			for (int i = 0; i < _count; i++)
				reader();
		}))
		.Concat (Enumerable.Range(0, _writersCount)
		.Select(n => new Thread(() => {
			for (int i = 0; i < _count; i++)
				writer();
		}))).ToArray();

		//clears the dictionary, start stopwatch (System.Diagnostics), start threads
		_map.Clear();
		var sw = Stopwatch.StartNew();

		foreach (var thread in threads)
			thread.Start();

		foreach (var thread in threads)
			thread.Join();

		sw.Stop();
		return sw.ElapsedMilliseconds;
	}


	//-----------------------------------------------------------
	// reader using simple lock
	private static void SimpleLockReader()
	{
		lock (_simpleLockLock)
			ReaderProc();
		return;
	}


	//-----------------------------------------------------------
	// writer using simple lock
	private static void SimpleLockWriter()
	{
		lock (_simpleLockLock)
			WriterProc();
		return;
	}


	//-----------------------------------------------------------
	// reader using obsolete ReaderWriterLock
	private static void RWLockReader()
	{
		_rwLock.AcquireReaderLock(-1);
		try {ReaderProc();}
		finally {_rwLock.ReleaseReaderLock();}
		return;
	}


	//-----------------------------------------------------------
	// writer using obsolete ReaderWriterLock
	private static void RWLockWriter()
	{
		_rwLock.AcquireWriterLock(-1);
		try {WriterProc();}
		finally {_rwLock.ReleaseWriterLock();}
	}


	//-----------------------------------------------------------
	// reader using ReaderWriterLockSlim
	private static void RWLockSlimReader()
	{
		_rwLockSlim.EnterReadLock();
		try {	ReaderProc();}
		finally {_rwLockSlim.ExitReadLock();}
	}


	//-----------------------------------------------------------
	// writer using ReaderWriterLockSlim
	private static void RWLockSlimWriter()
	{
		_rwLockSlim.EnterWriteLock();
		try {WriterProc();} finally {_rwLockSlim.ExitWriteLock();}
	}


	//-----------------------------------------------------------
	//
	static void Main()
	{
		//	Warm up
		Measure(SimpleLockReader, SimpleLockWriter);

		// Measure simple lock
		var simpleLockTime = Measure(SimpleLockReader, SimpleLockWriter);
		Console.WriteLine("Simple lock: {0}ms", simpleLockTime);

		// Warm up
		Measure(RWLockReader, RWLockWriter);

		// Measure obsolete ReaderWriterLock
		var rwLockTime = Measure(RWLockReader, RWLockWriter);
		Console.WriteLine("ReaderWriterLock: {0}ms", rwLockTime);

		// Warm  up
		Measure(RWLockSlimReader, RWLockSlimWriter);

		// Measure ReaderWriterLockSlim
		var rwLockSlimTime = Measure(RWLockSlimReader, RWLockSlimWriter);
		Console.WriteLine("ReaderWriterLockSlim: {0}ms", rwLockSlimTime);
	}
}