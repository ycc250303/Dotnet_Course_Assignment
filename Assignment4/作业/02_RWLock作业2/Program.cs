using System;
using System.Collections.Generic;
using System.Threading;
using static System.Console;

/// <summary>
/// 作业2：对比「长时间占写锁」与「可升级读锁 + 短时写锁」对读者吞吐的影响。
/// 题目来源：Assignment4/01 线程同步/07 ReaderWriterLockSlim/Program.cs 文末注释。
/// </summary>
class Program
{
	private static readonly ReaderWriterLockSlim Rw = new ReaderWriterLockSlim();
	private static readonly Dictionary<int, int> Items = new();
	private static long _readCount;
	private static volatile bool _running = true;

	static void Main()
	{
		const int readerThreads = 6;
		const int writerIntervalMs = 40;
		const int runSeconds = 3;
		const int simulateReadCostMs = 3;

		for (int pass = 1; pass <= 2; pass++)
		{
			Items.Clear();
			_readCount = 0;
			_running = true;

			bool useUpgradeable = pass == 2;
			WriteLine(useUpgradeable
				? "=== 策略 B：EnterUpgradeableReadLock，仅在插入时升级写锁 ==="
				: "=== 策略 A：EnterWriteLock 包住「模拟读耗时 + 检查 + 写入」===");

			var readers = new Thread[readerThreads];
			for (int i = 0; i < readers.Length; i++)
			{
				readers[i] = new Thread(ReaderLoop) { IsBackground = true };
				readers[i].Start();
			}

			var writer = new Thread(() => WriterLoop(useUpgradeable, writerIntervalMs, simulateReadCostMs))
			{
				IsBackground = true
			};
			writer.Start();

			Thread.Sleep(TimeSpan.FromSeconds(runSeconds));
			_running = false;
			writer.Join();
			foreach (var t in readers)
				t.Join();

			WriteLine($"读者累计完成读次数（越高越好）: {_readCount:N0}");
			WriteLine();
		}

		WriteLine("说明：在写者需要「先读后写」时，策略 B 通常让读锁持有时间更长，读者吞吐更高。");
	}

	static void ReaderLoop()
	{
		while (_running)
		{
			try
			{
				Rw.EnterReadLock();
				_ = Items.Count;
				foreach (var _ in Items.Keys)
				{
					Thread.Sleep(0);
					break;
				}
			}
			finally
			{
				if (Rw.IsReadLockHeld)
					Rw.ExitReadLock();
			}

			Interlocked.Increment(ref _readCount);
		}
	}

	static void WriterLoop(bool useUpgradeable, int intervalMs, int simulateReadCostMs)
	{
		var rnd = new Random();
		while (_running)
		{
			int key = rnd.Next(2000);

			if (!useUpgradeable)
			{
				try
				{
					Rw.EnterWriteLock();
					Thread.Sleep(simulateReadCostMs);
					if (!Items.ContainsKey(key))
						Items[key] = 1;
				}
				finally
				{
					if (Rw.IsWriteLockHeld)
						Rw.ExitWriteLock();
				}
			}
			else
			{
				try
				{
					Rw.EnterUpgradeableReadLock();
					Thread.Sleep(simulateReadCostMs);
					if (!Items.ContainsKey(key))
					{
						try
						{
							Rw.EnterWriteLock();
							if (!Items.ContainsKey(key))
								Items[key] = 1;
						}
						finally
						{
							if (Rw.IsWriteLockHeld)
								Rw.ExitWriteLock();
						}
					}
				}
				finally
				{
					if (Rw.IsUpgradeableReadLockHeld)
						Rw.ExitUpgradeableReadLock();
				}
			}

			Thread.Sleep(intervalMs);
		}
	}
}
