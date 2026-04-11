using System;
using System.Diagnostics;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static System.Diagnostics.Process;

class Program
{
	const string Item = "Dictionary item";
    const int Iterations = 2000000;
    //public static string CurrentItem;

	static void UseThreads_ConcurrentDictionary(int numberOfOperations)
	{
		var concurrentDictionary = new ConcurrentDictionary<int, string>();

		using (var countdown = new CountdownEvent(numberOfOperations))
		{
			WriteLine("Scheduling work by creating threads");
			for (int i = 0; i < numberOfOperations; i++)
			{
				var thread = new Thread((index) =>
                {
                	int indexPos = (int)index;
                	int interval = Iterations/numberOfOperations;
                	int nFrom = interval * indexPos;
                	for (int i = nFrom; i < (nFrom + interval); i++)
					{
						concurrentDictionary[i] = Item;
					}
					countdown.Signal();
				});
				thread.Start(i);
			}
			countdown.Wait();
			WriteLine();
		}
	}

	static void UseThreads_Dictionary(int numberOfOperations)
	{
		var dictionary           = new Dictionary<int, string>();

		using (var countdown = new CountdownEvent(numberOfOperations))
		{
			WriteLine("Scheduling work by creating threads");
			for (int i = 0; i < numberOfOperations; i++)
			{
				var thread = new Thread((index) =>
                {
                	int indexPos = (int)index;
                	int interval = Iterations/numberOfOperations;
                	int nFrom = interval * indexPos;

					for (int i = nFrom; i < (nFrom + interval); i++)
					{
						lock (dictionary)
						{
							dictionary[i] = Item;
						}
					}
					countdown.Signal();
				});
				thread.Start(i);
			}
			countdown.Wait();
			WriteLine();
		}
	}

	

	static void Main(string[] args)
	{
		WriteLine("Running on a single core");
		//每个处理器都表示为一个位。 位 0 是处理器 1，位 1 是处理器 2，等等
		//试试 1 ,3 ,7，看看有什么效果
		GetCurrentProcess().ProcessorAffinity = new IntPtr(7);

		const int numberOfOperations = 3;
		var sw = new Stopwatch();
		sw.Start();
		UseThreads_Dictionary(numberOfOperations);
		sw.Stop();
		WriteLine($"Execution time using threads: {sw.ElapsedMilliseconds}");

		sw.Reset();
		sw.Start();
		UseThreads_ConcurrentDictionary(numberOfOperations);
		sw.Stop();
		WriteLine($"Execution time using the thread pool: {sw.ElapsedMilliseconds}");
	}
}
