using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;
using static System.Diagnostics.Process;
class Program
{
	class ThreadSample
	{
		private bool _isStopped = false;
		public void Stop()
		{
			_isStopped = true;
		}
		public void CountNumbers()
		{
			long counter = 0;
			while (!_isStopped)
			{
				counter++;
			}
			WriteLine($"{CurrentThread.Name} with " +
                $"{CurrentThread.Priority,11} priority " +
				$"has a count = {counter,13:N0}");
		}
	}
	static void RunThreads()
	{
		var sample = new ThreadSample();
		var threadOne = new Thread(sample.CountNumbers);
		threadOne.Name = "ThreadOne";
		var threadTwo = new Thread(sample.CountNumbers);
		threadTwo.Name = "ThreadTwo";
		threadOne.Priority = ThreadPriority.Highest;
		threadTwo.Priority = ThreadPriority.Lowest;
		threadOne.Start();
		threadTwo.Start();
		Sleep(TimeSpan.FromSeconds(2));
		sample.Stop();
	}
	static void Main(string[] args)
	{
		WriteLine($"Current thread priority: {CurrentThread.Priority}");
		WriteLine("Running on all cores available");
		RunThreads();
		Sleep(TimeSpan.FromSeconds(2));
		WriteLine("Running on a single core");
		//每个处理器都表示为一个位。 位 0 是处理器 1，位 1 是处理器 2，等等
		//试试 1 ,3 ,7，看看有什么效果
		GetCurrentProcess().ProcessorAffinity = new IntPtr(3);
		RunThreads();
		WriteLine("Main End.");
	}
}
