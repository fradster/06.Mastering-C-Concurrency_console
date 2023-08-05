/*
Monitor class can be used to orchestrate multiple threads into a workflow with the Wait, Pulse and PulseAll	methods. When a main thread calls the Wait method, the current lock is released, and the thread is blocked until some other thread calls the Pulse or PulseAll methods. This allows the coordination of different threads execution into some sort of sequence.
Here, we have 2 threads, main and additional that performs calculation. We pause the main thread until the second one finishes, and then get back tothe main thread and block the 2nd thread until new data for calculation is available
*/

using System;
using System.Threading;
using System.Linq;

class DataShareEx
{

	static void Main()
	{
		int arg = 0, counter = 0;
		string result = String.Empty;

		var lockHandle = new object();

		//define calc thread
		var calcThread = new Thread(() => {
			while (true)
				lock (lockHandle)
				{
					counter++; result= arg.ToString();

					//after calc is completed, 2nd thread calls Pulse which moves the main thread, which is at the top of the "waiting queue", into the "ready queue". Ifr there are more threads that are in the "waiting queue", it will move only the first one. Then Wait makes the main thread (now in the "ready queue") reacquires the lock, changes the calc data and repeats the process one more time.
					Monitor.Pulse(lockHandle); Monitor.Wait(lockHandle);
				}
		})
		// What is this, Thread block?? What is IsBackground ?? Anyway, its not necessary, can be commented
		///{
			///IsBackground	=	true
		///}
		;


		lock	(lockHandle)
		{
			//start calc thread
			calcThread.Start();
			Thread.Sleep(100);
			Console.WriteLine("counter = {0},  result = {1}", counter, result);

			//we call Pulse on lockHandle, which puts the calc thread into queue ("ready queue"), meaning that thread is ready to accquire this lock as soon as it gets released. Then Wait method realeases lockHandle and put the mainHTread into "waiting queue". In that moment, the calc thread acquires the lock and starts to work.
			arg = 123;
			Monitor.Pulse(lockHandle);
			Monitor.Wait(lockHandle);
			Console.WriteLine("counter = {0},  result = {1}", counter, result);

			arg	=	321;
			Monitor.Pulse(lockHandle);
			Monitor.Wait(lockHandle);
			Console.WriteLine("counter = {0},  result = {1}", counter, result);
		}
	}
}