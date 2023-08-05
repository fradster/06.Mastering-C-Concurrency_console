/*
Task.Factory.StartNew, TaskCreationOptions.AttachedToParent parameter
Task scheduler needs explicitly defined dependencies between tasks to run them effectively and in the correct order. Besides this, there is a way to achieve implicit dependency definition; when we create one task inside another, a special parent-child dependency is created for these tasks. By default, this does not affect how these tasks will be executed, but if the task is created with TaskCreationOptions.AttachedToParent option, the parent task will not complete until every child task completes. Also, if any child task fails, the parent task will have the TaskStatus.Faulted status as well.
There is a difference when referrencig a separate action and when defining the action within the task. In first case, sometimes "Child started" and "Child completed" comments are missing. In the second case, "Parent completed" "Child completed" are missing (actually, they are still running, and will be printed latter). In third case (using TaskCreationOptions.AttachedToParent parameter), everything is ok, the child is started and completed within parents start and completion
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


//------------------------------------------------------------------
//
static class DataShareEx
{
	//---------------------------------------------------------
	//
	static void Main()
	{
		void ChildAction(string tskName)
		{
			Console.WriteLine(tskName +" Child started");
			Thread.Sleep(100);
			Console.WriteLine(tskName + " Child completed");
			return;
		}

		// This task defines a child task by referrencing separately defined method
		Task taskA1 () {
			Console.WriteLine("Ex. where child task is defined as a separate action");
			Console.WriteLine("A1 Parent started");
			Task.Factory.StartNew(() => ChildAction("A1"));
			return Task.CompletedTask;
		};

		Task.Factory.StartNew(() => taskA1()).Wait();
		Console.WriteLine("A1 Parent completed");


		// This task defines child task directly within the first task
		Task taskA2 () {
			Console.WriteLine("\nEx. where child task is defined within the parent task directly");
			Console.WriteLine("A2 Parent started");
			Task.Factory.StartNew(() =>
			{
				Console.WriteLine("A2 Child started");
				Thread.Sleep(100);
				Console.WriteLine("A2 Child completed");
				return;
			});
			return Task.CompletedTask;
		};

		Task.Factory.StartNew(() => taskA2()).Wait();
		Console.WriteLine("A2 Parent completed");


		// defines a child task by referrencing separately defined method, and using TaskCreationOptions.AttachedToParent
		Task taskA3 () {
			Console.WriteLine("\nEx. where child task is defined as a separate action and is using TaskCreationOptions.AttachedToParent");
			Console.WriteLine("A3 Parent started");
			Task.Factory.StartNew(() => ChildAction("A3"), TaskCreationOptions.AttachedToParent);
			return Task.CompletedTask;
		};

		Task.Factory.StartNew(() => taskA3()).Wait();
		Console.WriteLine("A3 Parent completed");
	}
}