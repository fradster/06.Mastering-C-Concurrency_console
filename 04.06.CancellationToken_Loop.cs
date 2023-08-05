/*
Never use Thread.Abort. It throws ThreadAbortException, which can not be stopped by the usual exception handler - catch block will be executed, but exception will be thrown right after. Also, If your thread is waiting for unmanaged code to complete, which is almost every I/O operation, the thread will not be aborted until this operation ends. If the operation never completes, your program will hang. Use Cancellation API instead of Thread.Abort (System.Threading.CancellationToken).
If the code inside a task is quite easy, for example, it is a loop with short iterations, then the easiest way to stop the operation is to check some flag variable inside this loop and exit if the flag is set.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;


//------------------------------------------------------------------
//
static class DataShareEx
{
	//----------------------------------------------------------
	// Create a Task, provide it with a tokem initiate cancellation process. Since its a static method, a Cancellation action needs to be inputed as a parameter
	private static void RunTest (Action<CancellationToken> action, string Name)
	{
		var cancelSource = new CancellationTokenSource();
		var cancelTok = cancelSource.Token;

		//define task. Here, cancellation token is provided both to our action, and to TPL's StartNew method. Its because the TPL is aware of cancellation tokens as well and can cancel the task even if it has not started yet and our code is not able to handle cancellation.
		var task = Task.Factory.StartNew(()=> action(cancelTok), cancelTok);

		//wait for the task to to start.
		while (task.Status != TaskStatus.Running) {}

		// start the stopwatch, cancel the task,
		var sw = Stopwatch.StartNew();
		cancelSource.Cancel();

		//wait while task is cacnceled (i.e. completed). Stop the stopwatch
		while (! task.IsCompleted) {}
		sw.Stop();

		Console.WriteLine("{0} task cancelled in {1} ms", Name, sw.ElapsedMilliseconds);
	}

	//---------------------------------------------------------
	// call the cancelation method with a infinite loop as a parameter
	static void Main()
	{
		RunTest(tok => doLoop(tok), "CheckFlag");
	}


	//---------------------------------------------------------
	// Loop method. Running the code in a infinite loop, check if cancellation is requested within the loop
	static void doLoop(CancellationToken tok)
	{
		while (true)
		{
			Thread.Sleep(100);
			if (tok.IsCancellationRequested)
				break;
		}
		return;
	}
}