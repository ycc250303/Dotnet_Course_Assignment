using System;
using System.Threading;

using static System.Console;

class Program
{
	static void PrintNumbers()
	{
		WriteLine("Starting...");
		for (int i = 1; i < 10; i++)
		{
			WriteLine(i);
		}
	}

	static void Main(string[] args)
	{
		Thread t = new Thread(PrintNumbers);
		t.Start();
		PrintNumbers();
	}
}

