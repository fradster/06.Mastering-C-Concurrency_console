/*
	Exception handeling - cant do it the usual way because each thread has its own stack. The simplest way to do it in multi threading is to check the task status. If tsk.Status = Faulted, exception was thown.
	The exception is wrapped within System.AggregateException, because there could be many exceptions from many tasks. In AggregateException instance, there is InnerExceptions property that will contain all the wrapped exceptions.
	while loop is used instead of Task.Wait, because Task.Wait will rethrow an exception after it was already handeled, so we will get an unhandelled exception.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
//using System.Collections.Generic;


//------------------------------------------------------------------
// Task for a long calculation.
static class DataShareEx
{
	static void  exclMethod (string escMessage)
	{
		throw new ApplicationException(escMessage);
	}


	//---------------------------------------------------------
	//
	static void Main()
	{
		// start the task thah will only throw eception
		var excTask = Task.Factory.StartNew(()=> exclMethod("Test exception"));

		//loop is used instead of Task.Wait, because Task.Wait will rethrow an exception after it was already handeled, so we will get an unhandelled exception.
		while(!excTask.IsCompleted) {}

		//print out task status and exception
		Console.WriteLine("Status = {0}", excTask.Status);
		Console.WriteLine(excTask.Exception);
	}
}