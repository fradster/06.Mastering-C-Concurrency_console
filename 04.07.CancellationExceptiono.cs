/*
If the code inside a task is complicated, it is difficult to check the flag in every part of the code (many loops, many methods, ...), it is much easier to use another cancellation technique—throwing a special cancellation exception (OperationCanceledException). It will stop the code and set task status to TaskState.Canceled
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
	// Create a Task, provide it with a tokem initiate cancellation process. Since its a static method, a Cancellation action needds to be inputed as a parameter
	private static void RunTest (Action<CancellationToken> action, string Name)
	{
		var cancelSource = new CancellationTokenSource();
		var cancelTok = cancelSource.Token;

		//define task, Here, cancellation token is provided both to our action, and to TPL's StartNew method. Its because the TPL is aware of cancellation tokens as well and can cancel the task even if it has not started yet and our code is not able to handle cancellation.
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
		RunTest(tok => doLoop(tok), "ThrowException");
	}


	//---------------------------------------------------------
	// Loop method. Running the code in a infinite loop, check if cancellation is requested within the loop
	static void doLoop(CancellationToken tok)
	{
		while (true)
		{
			Thread.Sleep(100);
			tok.ThrowIfCancellationRequested();//throw the exception, it will stop the code and set set task status to TaskState.Canceled
			break;//needed just to suppress "unreachable code" warning for "return" command
		}
		return;
	}
}