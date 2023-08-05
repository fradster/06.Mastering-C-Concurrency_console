/*
print the current size of a handle, giving us a way to detect whether we are in 32-bit or 64-bit mode. Then the code will start new threads until we get any exception, and it will print out the   number of threads that we were able to start
IntPtr.Size : 8
num of Threads: cca 800000
*/

using System;
using System.Threading;
using System.Linq;


//------------------------------------------------------------------
//
static class DataShareEx
{
	//---------------------------------------------------------
	//
	static void Main()
	{
		Console.WriteLine(IntPtr.Size);//how many bytes is int
		int cnt = 0;

		try
		{
			for (int i= 0; i< int.MaxValue; i++)
			{
				new Thread(()=> Thread.Sleep(Timeout.Infinite)).Start();
				cnt++;
			}
		}
		catch{Console.WriteLine("num of Threads: {0}", cnt);}
	}
}