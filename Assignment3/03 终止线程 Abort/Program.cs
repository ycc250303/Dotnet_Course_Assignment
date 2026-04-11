using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;
class Program
{
	static void PrintNumbersWithDelay()
	{
		try
		{
			WriteLine("Starting...");
			for (int i = 1; i < 10; i++)
			{
				Sleep(TimeSpan.FromSeconds(2));
				WriteLine(i);
			}
		}
		catch (ThreadAbortException)
		{
			WriteLine($"Here ThreadAbortException");
			Thread.ResetAbort();
		}
		finally
		{
			WriteLine("\r\nPrintNumbersWithDelay exit 1");
		}
		WriteLine("PrintNumbersWithDelay exit 2");
	}
	static void Main(string[] args)
	{
		WriteLine("Starting program...");
		Thread t = new Thread(PrintNumbersWithDelay);
		t.Start();
		Sleep(TimeSpan.FromSeconds(6));
		t.Abort();
		t.Join();
		WriteLine("A thread has been aborted");
	}
}
