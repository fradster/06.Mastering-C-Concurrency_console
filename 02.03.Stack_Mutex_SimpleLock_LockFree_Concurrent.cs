/*
Implemetnation of the stack, to use with multi-threading and with several types of locking and lock-free exchange. This will create an abstract BaseStack class and 4 implementations.
	1. MutexStack that uses OS- level lock i.e. System.Threading.Mutex which uses the mutex synchronization primitive from the OS. This implementation puts a thread in a blocked state every time it has to wait for the lock to be released.
	2. LockStack uses a simple lock
	3. Lock-free implementation, uses Interlocked.CompareExchange. A constraint "class" needs to be added to generic type parameter (because we cannot atomically exchange values that are more than 8 bytes in size)
	4. using ConcurrentStack, i.e, collection that is meant for concurrent processing
	My results - obvious poor performance of Mutex (OS-level) lock, and good performance of simple Lock (due to very simple routine within lock block) and superior Concurrent collections.
	1. Mutex:							4744 ms
	2. LockStack:						0142 ms
	3. Lock-free CompareExchange:	0348 ms
	4. ConcurrentStack:				0094 ms
*/

using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using	System.Collections.Concurrent;



//------------------------------------------------------------------
// base class for various stack implemetnations
public abstract class StackBase<T>
{
	//--------------------------------------------------------------
	//private class of StackBase, contains user data and the next element on the stack
	protected class Item
	{
		private readonly T _data;
		private readonly Item _next;

		//----------------------------------------------------
		//public constructor for private class
		public Item (T data, Item next)
		{
			_data = data; _next = next;
		}

		//---------------------------------------------------
		//
		public T Data {get {return _data;}}

		//---------------------------------------------------
		//
		public Item Next {get {return _next;}}
	}


	//stack's top item
	protected Item _head;

	//-------------------------------------------------------
	// indicates if stack is empty
	public bool IsEmpty()
		{return _head == null;}

	//-------------------------------------------------------
	//abstract push method
	public abstract void Push(T data);

	//-------------------------------------------------------
	//abstract pop method
	public abstract bool TryPop(out T data);
}



//------------------------------------------------------------------
// implementation of base class that uses OS-level Mutex lock
public class MutexStack<T> : StackBase<T>
{
	private readonly Mutex _lock = new Mutex();


	//-------------------------------------------------------
	// push implementation
	public override void Push(T data)
	{
		_lock.WaitOne();
		try{_head = new Item(data, _head);}
		finally{_lock.ReleaseMutex();}
		return;
	}


	//-------------------------------------------------------
	// pop implementation
	public override bool TryPop(out T data)
	{
		_lock.WaitOne();
		try
		{
			if(IsEmpty())
			{data = default(T); return false;}

			data = _head.Data;
			_head = _head.Next;
			return true;
		}
		finally{_lock.ReleaseMutex();}
	}
}



//------------------------------------------------------------------
// implementation of base class that uses simple lock
public class LockStack<T> : StackBase<T>
{
	private readonly object _lock = new object();

	//-------------------------------------------------------
	// push implementation
	public override void Push(T data)
	{
		lock(_lock)
			_head = new Item(data, _head);
		return;
	}


	//-------------------------------------------------------
	// pop implementation
	public override bool TryPop(out T data)
	{
		lock(_lock)
		{
			if(IsEmpty())
			{data = default(T); return false;}

			data = _head.Data;
			_head = _head.Next;
			return true;
		}
	}
}



//------------------------------------------------------------------
// implementation of base class that uses lock-free and Interlocked.CompareExchange atomic operation
public class LockFreeStack<T> : StackBase<T> where T: class
{

	//-------------------------------------------------------
	// push implementation.
	public override void Push(T data)
	{
		Item item, oldHead;

		do
		{oldHead = _head; item = new Item(data, oldHead);}
		while (oldHead != Interlocked.CompareExchange(ref _head, item, oldHead));
		return;
	}


	//-------------------------------------------------------
	// pop implementation
	public override bool TryPop(out T data)
	{
		var oldHead = _head;
		while (!IsEmpty())
		{
			if(oldHead == Interlocked.CompareExchange(ref _head, oldHead.Next, oldHead))
			{
				data = oldHead.Data;
				return true;
			}
			oldHead = _head;
		}

		data = null;
		return false;
	}
}



//------------------------------------------------------------------
// implementation of base class that uses ConcurrentStack
public class ConcurrentStackWrapper<T> : StackBase<T>
{
	private readonly ConcurrentStack<T> _stack;

	//-------------------------------------------------------
	// constructor
	public ConcurrentStackWrapper()
	{
		_stack = new ConcurrentStack<T>();
	}


	//-------------------------------------------------------
	// push implementation.
	public override void Push(T data)
	{
		_stack.Push(data);
	}


	//-------------------------------------------------------
	// pop implementation
	public override bool TryPop(out T data)
	{
		return _stack.TryPop(out data);
	}
}



//------------------------------------------------------------------
//
static class DataShareEx
{
	private static int numOfThreads = 6; //number of thread


	//------------------------------------------------------------------
	//
	private static long Measure (ThreadStart proc)
	{
		var threads = Enumerable.Range(0, numOfThreads).Select(n=> new Thread(proc)).ToList();

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

		int numOfCycles =60; // number of cycles for each thread
		int _iterationDepth = 1000;// num of elements to push to the stack

		var stack= new MutexStack<string>();
		///var stack= new LockStack<string>();
		///var stack= new LockFreeStack<string>();
		///var stack= new ConcurrentStackWrapper<string>();

		//--------------------------------------------
		ThreadStart Proc = () =>
		{
			for (int i1= 0; i1< numOfCycles; i1++)
			{
				for (int i2= 0; i2< _iterationDepth; i2++)
					stack.Push(i2.ToString());

				string result;
				for (int i2= 0; i2< _iterationDepth; i2++)
					stack.TryPop(out result);
			}
			return;
		};

		long elapsedMiliSec = Measure(Proc);
		Console.WriteLine("Elapsed time: {0} ms", elapsedMiliSec);
	}
}