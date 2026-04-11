using System;
using System.Threading;
using static System.Console;

/// <summary>
/// 作业(1)：演示持有 Mutex 的线程未 ReleaseMutex 就结束，等待线程的行为。
/// 题目来源：Assignment4/01 线程同步/01 Mutex/Program.cs 文末注释。
/// </summary>
class Program
{
	static void Main()
	{
		// 使用本地（未命名）互斥体，避免与课程示例的全局命名 Mutex 冲突
		using var mutex = new Mutex(false);

		var holderStarted = new ManualResetEventSlim(false);

		var holder = new Thread(() =>
		{
			mutex.WaitOne();
			WriteLine("[持有线程] 已获取 Mutex，即将在线程结束时故意不 ReleaseMutex。");
			holderStarted.Set();
			Thread.Sleep(300);
			WriteLine("[持有线程] 线程方法返回（未调用 ReleaseMutex）。");
		});

		var waiter = new Thread(() =>
		{
			holderStarted.Wait();
			WriteLine("[等待线程] 开始 WaitOne() …");
			try
			{
				mutex.WaitOne(TimeSpan.FromSeconds(10));
				WriteLine("[等待线程] WaitOne 正常返回（未观察到 AbandonedMutexException）。");
				mutex.ReleaseMutex();
			}
			catch (AbandonedMutexException ex)
			{
				WriteLine($"[等待线程] 捕获 AbandonedMutexException：{ex.Message}");
				WriteLine("说明：上一持有者未释放就终止后，系统把互斥体判为 abandoned；");
				WriteLine("      .NET 在把所有权交给等待方时会抛出该异常以提示状态可能不安全。");
				try
				{
					mutex.ReleaseMutex();
				}
				catch (Exception e2)
				{
					WriteLine($"ReleaseMutex 异常（可忽略于本演示）：{e2.GetType().Name}: {e2.Message}");
				}
			}
		});

		waiter.Start();
		holder.Start();
		holder.Join();
		waiter.Join();

		WriteLine();
		WriteLine("演示结束。结论：等待方通常不会永远阻塞在 WaitOne，但可能收到 AbandonedMutexException。");
	}
}
