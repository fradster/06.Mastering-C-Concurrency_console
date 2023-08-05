/*
	OperationCanceledException //this exception is treated diferrently in TPL, the status of task will be TaskStatus.Canceled instead of TaskStatus.Faulted, Exception property will be empty. But, if token is not passed as a second argument in StartNew() method, it will be trated as regular exception
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
//using System.Diagnostics;
//using System.Collections.Generic;


//------------------------------------------------------------------
// Task for a long calculation.
static class DataShareEx
{
	static void  ThrowOperationCanceledException (CancellationToken token)
	{
		while (true)
			token.ThrowIfCancellationRequested();
	}


	//---------------------------------------------------------
	//
	static void Main()
	{
		var cancelSource = new CancellationTokenSource();
		var cancelTok = cancelSource.Token;

		var task = Task.Factory.StartNew( () => ThrowOperationCanceledException(cancelTok), cancelTok);

		//wait while task starts to run
		while (task.Status != TaskStatus.Running) {}
		cancelSource.Cancel();

		//wait while task is completed
		while	(!task.IsCompleted) {}

		Console.WriteLine ("Status = {0}, IsCanceled = {1}",	task.Status,
		task.IsCanceled);
		Console.WriteLine(task.Exception);
	}
}