/*
Usually real world programs require some interaction between threads. Common problem with shared state is undefined access order (race condition). The expected counter value is 0. However, when you run the program, you get different numbers (which is usually not 0, but it could be) each time. The reason is that incrementing and decrementing the counter is not an atomic operation, but consists of three separate steps: reading the counter value, incrementing or decrementing this value, and writing the result back into the counter. Treads independetly raise and lower the counter.
Each thread will increase counter, wait for a 100ms, and then decrease it. And doing it 10000 times.
*/

using System;
using System.Threading;
using System.Linq;

class DataShareEx
{
	static void Main()
	{
		const int iterations = 10000;
		int counter = 0;

		//define proc delegate
		ThreadStart proc = () => {
			for (int i = 1; i<= iterations; i++)
				{counter++; Thread.SpinWait(100); counter --;}
			return;
		};


		// same but delegate version instead of lambda express.
		///ThreadStart proc = delegate(){
			///for (int i = 1; i<= iterations; i++)
				///{counter++; Thread.SpinWait(100); counter --;}
			///return;
		///};

		//define threads through array
		var threads = Enumerable.Range(0, 3)
			.Select(n => new Thread(proc))
		.ToArray();

		//start the threads
		foreach (var thread in threads)
			thread.Start();
		foreach (var thread in threads)
			thread.Join();

		Console.WriteLine(counter);
	}
}