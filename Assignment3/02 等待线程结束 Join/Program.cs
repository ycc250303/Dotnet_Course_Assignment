using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

class Program
{
	static void PrintNumbersWithDelay()
	{
		WriteLine("Starting...");
		for (int i = 1; i < 10; i++)
		{
			Sleep(TimeSpan.FromSeconds(2));
			WriteLine(i);
		}
	}
	static void Main(string[] args)
	{
		WriteLine("Starting program...");
		Thread t = new Thread(PrintNumbersWithDelay);
		t.Start();
		t.Join();
		WriteLine("Thread completed");
	}
}