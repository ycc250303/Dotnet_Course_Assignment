using System;
using System.Threading;
using static System.Console;
class Program
{
	abstract class CounterBase
	{
		public abstract void Increment();
		public abstract void Decrement();
	}
	class Counter : CounterBase
	{
		public int Count { get; private set; }
		public override void Increment()
		{
			Count++;
		}
		public override void Decrement()
		{
			Count--;
		}
	}
	class CounterWithMonitor : CounterBase
	{
		private readonly object _syncRoot = new Object();
		public int Count { get; private set; }
		public override void Increment()
		{
			bool lockTaken = false;
			try
			{
				Monitor.Enter(_syncRoot,ref lockTaken);
				Count++;
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(_syncRoot);
				}
			}
		}
		public override void Decrement()
		{
			bool lockTaken = false;
			try
			{
				Monitor.Enter(_syncRoot,ref lockTaken);
				Count--;
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(_syncRoot);
				}
			}
		}
	}

	class CounterWithLock : CounterBase
	{
		private readonly object _syncRoot = new Object();
		public int Count { get; private set; }
		public override void Increment()
		{
			lock (_syncRoot)
			{
				Count++;
			}
		}
		public override void Decrement()
		{
			lock (_syncRoot)
			{
				Count--;
			}
		}
	}
	

	static void TestCounter(CounterBase c)
	{
		for (int i = 0; i < 100000; i++)
		{
			c.Increment();
			//c.Decrement();
		}
	}

	static void Main(string[] args)
	{
		WriteLine("Incorrect counter");

		var c = new Counter();

		var t1 = new Thread(() => TestCounter(c));
		var t2 = new Thread(() => TestCounter(c));
		var t3 = new Thread(() => TestCounter(c));
		t1.Start();
		t2.Start();
		t3.Start();
		t1.Join();
		t2.Join();
		t3.Join();

		WriteLine($"Total count: {c.Count}");

		WriteLine("--------------------------");
		WriteLine("Correct counter with Monitor");
		var c1 = new CounterWithMonitor();
		t1 = new Thread(() => TestCounter(c1));
		t2 = new Thread(() => TestCounter(c1));
		t3 = new Thread(() => TestCounter(c1));
		t1.Start();
		t2.Start();
		t3.Start();
		t1.Join();
		t2.Join();
		t3.Join();
		WriteLine($"Total count: {c1.Count}");

		WriteLine("Correct counter with lock");
		var c2 = new CounterWithLock();
		t1 = new Thread(() => TestCounter(c2));
		t2 = new Thread(() => TestCounter(c2));
		t3 = new Thread(() => TestCounter(c2));
		t1.Start();
		t2.Start();
		t3.Start();
		t1.Join();
		t2.Join();
		t3.Join();
		WriteLine($"Total count: {c2.Count}");
	}
}
