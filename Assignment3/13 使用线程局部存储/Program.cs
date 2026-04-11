using System;
using System.Threading;

using static System.Console;

class Program
{
	//[ThreadStatic]
	static int counterPerThread = 0;

	static void PrintNumbers()
	{
		for (int i = 0; i < 50000000; i++)
		{
			//WriteLine(i);
			counterPerThread++;
		}
		WriteLine($"counterPerThread: {counterPerThread}");
	}

	static void Main(string[] args)
	{
		Thread t = new Thread(PrintNumbers);
		t.Start();
		PrintNumbers();
	}
}

