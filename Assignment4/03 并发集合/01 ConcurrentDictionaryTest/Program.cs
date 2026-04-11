using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Console;

class Program
{
	const string Item = "Dictionary item";
    const int Iterations = 10000000;
    public static string CurrentItem;

    static void Main(string[] args)
	{
		var concurrentDictionary = new ConcurrentDictionary<int, string>();
		var dictionary = new Dictionary<int, string>();

		var sw = new Stopwatch();

		sw.Start();
		for (int i = 0; i < Iterations; i++)
		{
			lock (dictionary)
			{
				dictionary[i] = Item;
			}
		}
		sw.Stop();
		WriteLine($"Writing to dictionary with a    lock: {sw.Elapsed}");

		sw.Restart();
		for (int i = 0; i < Iterations; i++)
		{
			concurrentDictionary[i] = Item;
		}
		sw.Stop();
		WriteLine($"Writing to a concurrent   dictionary: {sw.Elapsed}");

		sw.Restart();
		for (int i = 0; i < Iterations; i++)
		{
			lock (dictionary)
			{
				CurrentItem = dictionary[i];
			}
		}
		sw.Stop();
		WriteLine($"Reading from dictionary with a  lock: {sw.Elapsed}");

		sw.Restart();
		for (int i = 0; i < Iterations; i++)
		{
			CurrentItem = concurrentDictionary[i];
		}
		sw.Stop();
		WriteLine($"Reading from a concurrent dictionary: {sw.Elapsed}");
	}
}

//作业3:使用单个线程与多个线程(6个)测试对比ConcurrentDictionary数据结构的性能
//作业4:查找资料，熟悉以下数据结构的用法

//(1)ConcurrentDictionary
//(2)ConcurrentQueue
//(3)ConcurrentStack
//(4)ConcurrentBag
//(5)BlockingCollection
