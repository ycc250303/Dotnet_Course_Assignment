using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

class Program
{
	private static void AsyncOperation(object state)
	{
		WriteLine($"Operation state: {state ?? "(null)"}");
		WriteLine($"Worker thread id: {CurrentThread.ManagedThreadId}");
		Sleep(TimeSpan.FromSeconds(2));
	}
	static void Main(string[] args)
	{
		const int x = 1;
		const int y = 2;
		const string lambdaState = "lambda state 2";

		ThreadPool.QueueUserWorkItem(AsyncOperation);
		Sleep(TimeSpan.FromSeconds(1));

		ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
		Sleep(TimeSpan.FromSeconds(1));

		ThreadPool.QueueUserWorkItem( state => 
        {
			WriteLine($"Operation state: {state}");
			WriteLine($"Worker thread id: {CurrentThread.ManagedThreadId}");
			Sleep(TimeSpan.FromSeconds(2));
		}, "lambda state me");

		ThreadPool.QueueUserWorkItem( _ =>
		{
			WriteLine($"Operation state: {x + y}, {lambdaState}");
			WriteLine($"Worker thread id: {CurrentThread.ManagedThreadId}");
			Sleep(TimeSpan.FromSeconds(2));
		}, "lambda state other");

		Sleep(TimeSpan.FromSeconds(2));
	}
}
