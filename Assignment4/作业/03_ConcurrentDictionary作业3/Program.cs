using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static System.Console;
using static System.Diagnostics.Process;

/// <summary>
/// 作业3：6 个工作线程并发写入，对比 Dictionary+lock 与 ConcurrentDictionary。
/// 题目来源：Assignment4/03 并发集合/01 ConcurrentDictionaryTest/ProgramRun.cs（改为 6 线程）。
/// </summary>
class Program
{
	const string Item = "Dictionary item";
	const int Iterations = 2_000_000;
	/// <summary>作业要求：6 个线程。</summary>
	const int NumberOfOperations = 6;

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
					int indexPos = (int)index!;
					int interval = Iterations / numberOfOperations;
					int nFrom = interval * indexPos;
					for (int j = nFrom; j < nFrom + interval; j++)
						concurrentDictionary[j] = Item;
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
		var dictionary = new Dictionary<int, string>();

		using (var countdown = new CountdownEvent(numberOfOperations))
		{
			WriteLine("Scheduling work by creating threads");
			for (int i = 0; i < numberOfOperations; i++)
			{
				var thread = new Thread((index) =>
				{
					int indexPos = (int)index!;
					int interval = Iterations / numberOfOperations;
					int nFrom = interval * indexPos;

					for (int j = nFrom; j < nFrom + interval; j++)
					{
						lock (dictionary)
							dictionary[j] = Item;
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
		WriteLine("作业3：6 线程并发写入，总键数 2_000_000");
		WriteLine("ProcessorAffinity = 7（与课程 ProgramRun 一致，可按需注释）");
		GetCurrentProcess().ProcessorAffinity = new IntPtr(7);

		var sw = new Stopwatch();
		sw.Start();
		UseThreads_Dictionary(NumberOfOperations);
		sw.Stop();
		WriteLine($"Execution time using threads: {sw.ElapsedMilliseconds}");

		sw.Reset();
		sw.Start();
		UseThreads_ConcurrentDictionary(NumberOfOperations);
		sw.Stop();
		WriteLine($"Execution time using the thread pool: {sw.ElapsedMilliseconds}");
		WriteLine("(第二行标签为课程原注释笔误；此处两段均为 new Thread。)");
	}
}
