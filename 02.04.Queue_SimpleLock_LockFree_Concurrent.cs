/*
Implemetnation of the queue, to use with multi-threading with several types of locking and lock-free exchange.
	1. Lock-free implementation, uses Interlocked.CompareExchange.
	2. QueueWrapper uses a simple lock
	3. using ConcurrentQueue, i.e, collection that is meant for concurrent processing
	My results - obvous poor performance of LockFreeQueue, and good performance of simple Lock and superior Concurrent collections.
	1. simple lock queue:					2328 ms
	2. LockFreeQueue CompareExchange:	6244 ms
	3. ConcurrentQueue:						1762 ms
*/

using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using	System.Collections.Concurrent;



//------------------------------------------------------------------
//
public class LockFreeQueue<T>
{
	//--------------------------------------------------------------
	//private class of LockFreeQueue, contains user data and the next element on the queue
	protected class Item
	{
		public T Data;
		public Item Next;
	}

	//Queue's top and tail item
	private Item _head;
	private Item _tail;

	//-------------------------------------------------------
	// constructor
	public LockFreeQueue()
	{
		_head = new Item();
		_tail = _head;
	}

	//--------------------------------------------------------
	//
	public void Enqueue (T data)
	{
		Item item = new Item();
		item.Data = data;

		Item oldTail = null;
		Item oldNext = null;

		bool update = false;
		while (!update)
		{
			oldTail = _tail; oldNext = oldTail.Next;

			Thread.MemoryBarrier();//full fencing
			if(_tail == oldTail)
			{
				if(oldNext == null)
					update = (Interlocked.CompareExchange(ref _tail.Next, item, null) == null);
				else
					Interlocked.CompareExchange(ref _tail, oldNext, oldTail);
			}
		}
		Interlocked.CompareExchange(ref _tail, item, oldTail);
	}


	//--------------------------------------------------------
	//
	public bool TryDequeue (out T result)
	{
		result = default(T);
		Item oldNext = null;
		bool advanced = false;

		while (!advanced)
		{
			//local copies of variables that are needed
			Item oldHead = _head;
			Item oldTail = _tail;
			oldNext = oldHead.Next;

			//acquire a full fence to prevent read and write reordering
			Thread.MemoryBarrier();

			//in case when the head item has not been changed yet
			if (oldHead == _head)
			{
				if (oldHead == oldTail) //check whether the queue is empty
				{
					if (oldNext != null) //this should be false. If not, it means that we have a lagging  tail and we need to update it
					{
						Interlocked.CompareExchange(ref _tail, oldNext, oldTail);
						continue;
					}
					//If we are here, we have an empty queue
					result = default(T);
					return false;
				}
				//get the dequeueing item and try to advance the head reference
				result = oldNext.Data;
				advanced = Interlocked.CompareExchange(ref _head, oldNext, oldHead) == oldHead;
			}
		}
		//remove any references that can prevent the garbage collector from doing its job, and exit
		oldNext.Data = default(T);
		return true;
	}


	//--------------------------------------------------------
	//
	public bool	IsEmpty
	{
		get {return _head == _tail;}
	}
}


//------------------------------------------------------------------------
// unify access to queues and compare different ways to synchronize access to the queue
public interface IConcurrentQueue<T>
{
	void Enqueue (T data);
	bool TryDequeue (out T data);
	bool IsEmpty { get; }
}


//-------------------------------------------------------------------------
// Both LockFreeQueue and the standard ConcurrentQueue are already implementing this interface, and all we need to do is to create a wrapper class like this:
class LockFreeQueueWrapper<T> : LockFreeQueue<T>, IConcurrentQueue<T> {}

//-------------------------------------------------------------------------
//
class ConcurrentQueueWrapper<T> : ConcurrentQueue<T>, IConcurrentQueue<T> {}


//-------------------------------------------------------------------------
// We need a more advanced wrapper in the case of a non-thread-safe Queue collection:
class	QueueWrapper<T> : IConcurrentQueue<T>
{
	private readonly object _syncRoot = new object();
	private readonly Queue<T> _queue = new Queue<T>();

	//--------------------------------------------------------
	//
	public void Enqueue(T data)
	{
		lock(_syncRoot)
			_queue.Enqueue(data);
	}

	//--------------------------------------------------------
	//
	public bool TryDequeue(out T data)
	{
		//We have used a double checked locking pattern inside the TryDequeue method. At first glance, it seems that the first if statement is not doing anything useful, and we can just remove it. If you do an experiment and run the program without the first check, it will run about 50 times slower. The goal of the first check is to see whether the queue is empty so that a lock is not acquired; the lock and other threads are allowed to access the queue. Making a lock code minimal is very important, and it is illustrated here very well.
		if (_queue.Count > 0)
		{
			lock (_syncRoot)
			{
				if (_queue.Count > 0)
				{
					data = _queue.Dequeue();
					return true;
				}
			}
		}
		data = default(T);
		return false;
	}

	//--------------------------------------------------------
	//
	public bool IsEmpty
	{
		get {return _queue.Count == 0;}
	}
}


//------------------------------------------------------------------
//
static class DataShareEx
{
	private static int numOfCycles =(Int32) 1E6; // number of cycles for each thread
	private static int numOfThreads = 6;


	//------------------------------------------------------------------
	//
	private static long Measure (IConcurrentQueue<string>	queue)
	{
		//--------------------------------------------------------
		ThreadStart WriteProc = () =>
		{
			for (int i1= 0; i1< numOfCycles; i1++)
				queue.Enqueue(i1.ToString());
			return;
		};

		//--------------------------------------------------------
		ThreadStart ReadProc = () =>
		{
			var left = numOfCycles * numOfThreads;

			while (left > 0)
			{
				string res;
				if (queue.TryDequeue(out res))
					left--;
			}
			return;
		};

		var threadEnum= Enumerable.Range(0, numOfThreads);
		var threads = threadEnum.Select(n=> new Thread(WriteProc)).ToList()
			.Concat(new List<Thread>{new Thread(ReadProc)});

		var sw = Stopwatch.StartNew();

		foreach(var thr in threads)
			thr.Start();
		foreach(var thr in threads)
			thr.Join();

		sw.Stop();

		return sw.ElapsedMilliseconds;
	}

	//---------------------------------------------------------
	//
	static void Main()
	{
		///var queue= new QueueWrapper<string>();
		///var queue= new LockFreeQueueWrapper<string>();
		var queue= new ConcurrentQueueWrapper<string>();

		long elapsedMiliSec = Measure(queue);
		Console.WriteLine("Elapsed time: {0} ms", elapsedMiliSec);
	}
}