/*
CallBack cancelation
If we use external code, which we have no control over (I/O operation), we can register some cancellation code as a callBack  and run this callBack as soon as a cancellation is requested.
Here, we have server code that listens to the port 8083 and, when connection is established, waits for 100ms (simulates some IO operation) and responds with a "OK".
Within our task (client part), we connect server via TCPCLient class and cancel the conection as soon as possible.
The main line of the code, Close method closes TCP connection if it has been opened. Register method accepts the callBack thta will be called in case of cancellation and returns CancellationTokenRegistration structure. It will not wait for the server to close and will cancel the operation almost immidiatelly
using (tok.Register(client.Close)
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.IO;


//------------------------------------------------------------------
//
static class DataShareEx
{
	//------------------------------------------------------------------
	// Method for the server part of the app. Listens to the port 8083 and, when connection is established, waits for 100ms (simulates some IO operation) and responds with a "OK"
	private static void ServerAction(int port)
	{
		//listens to the port
		var listener = new TcpListener(IPAddress.Any, port);
		listener.Start();

		while (true)
		{
			using (var client = listener.AcceptTcpClient())
				using (var stream = client.GetStream())
					using (var writer = new StreamWriter(stream))
					{
						Thread.Sleep(100);
						writer.WriteLine("OK");
					}
		}
	}



	//---------------------------------------------------------
	// Client method. Connect server via TCPCLient class and cancel the conection as soon as possible
	static void ClientMethod(CancellationToken tok, int port)
	{
		while (true)
		{
			//connect to the server, read the stream and print it out
			using (var client = new TcpClient())
				//this is the main line of the code, Close method closes TCP connection if it has been opened. Register method accepts the callBack thta will be called in case of cancellation and returns CancellationTokenRegistration structure. It will not wait for the server to close and will cancel the operation almost immidiatelly. Without this line (just comment the line and append the code block to the using above) cancelation wil take 109ms, with a line it takes 10ms
				using (tok.Register(client.Close))
				{
					client.Connect("localhost", port);

					using (var stream = client.GetStream())
						using (var reader = new StreamReader(stream))
							Console.WriteLine(reader.ReadLine());
				}

			tok.ThrowIfCancellationRequested();//throw the exception, it will stop the code and set set task status to TaskState.Canceled
			break;//needed just to suppress "unreachable code" warning for "return" command
		}
		return;
	}


	//----------------------------------------------------------
	// Create a Task for a client action, provide it with a tokem initiate cancellation process. Since its a static method, a Cancellation action needds to be inputed as a parameter
	private static void RunTest (Action<CancellationToken> action, int port, string Name)
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
		const int port = 8083;

		//run server action in a new thread as a background process
		Thread serverThread = new Thread(() => ServerAction(port));
		serverThread.IsBackground = true;
		serverThread.Start();


		//run client task
		RunTest(tok => ClientMethod(tok, port), port, "Callback");
	}
}