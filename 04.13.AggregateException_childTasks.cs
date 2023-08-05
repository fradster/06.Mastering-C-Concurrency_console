/*
	Exception handeling - System.AggregateException: exceptions are nested the same way tasks are: children AggregateException are wrapped within parent one, we need check inner exceptions of all child AggregateException to get accurate information about exception events.
	Here, we have family of 4 tasks, with nested hierarchy, and try catch block which will catch all paretn and child AggregateException and unwrap its internal Exceptions
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
	static void  ThrowAppException (string escMessage)
	{
		throw new ApplicationException(escMessage);
	}


	//---------------------------------------------------------
	//
	static void Main()
	{
		//AttachedToParent option will automatically wait for child to complete before marking parent task as completed
		var Att2Par = TaskCreationOptions.AttachedToParent;

		//create parent task, I gen
		var parentTask = Task.Factory.StartNew( () =>  {

			//1. child task of II gen.
			Task.Factory.StartNew (
				() => {

					//child task of III gen.
					Task.Factory.StartNew( () =>
						ThrowAppException("III Gen. And we need to go deeper"), Att2Par
					);

					//throw exception for the II gen task
					ThrowAppException("II Gen. Test exception");
				}, Att2Par
			);

			//2. child task of II gen.
			Task.Factory.StartNew ( () =>	ThrowAppException("II Gen. Test sibling exception"), Att2Par);

			//1 Gen exception
			ThrowAppException("I Gen. Parent exception");
		});

		try { parentTask.Wait();}
		catch (AggregateException aExc)
		{
			//goes through all excpetions nested within AggregateException, flattens them to a non-nested list, get all inner Exceptions from it
			foreach (Exception e in aExc.Flatten().InnerExceptions)
				Console.WriteLine ("{0} : {1}", e.GetType(), e.Message);
		}
	}
}