using System;
using System.Collections.Generic;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

class Program
{
	static ReaderWriterLockSlim _rw    = new ReaderWriterLockSlim();
	static Dictionary<int, int> _items = new Dictionary<int, int>();

	static void Main(string[] args)
	{
		new Thread(Read){ IsBackground = true }.Start();
		new Thread(Read){ IsBackground = true }.Start();
		new Thread(Read){ IsBackground = true }.Start();
		new Thread(() => Write("Thread 1")){ IsBackground = true }.Start();
		new Thread(() => Write("Thread 2")){ IsBackground = true }.Start();

		Sleep(TimeSpan.FromSeconds(30));
	}

	static void Read()
	{
		WriteLine("Reading contents of a dictionary");
		while (true)
		{
			try
			{
				_rw.EnterReadLock();
				WriteLine("In Reading");
				foreach (var key in _items.Keys)
				{
					Sleep(TimeSpan.FromSeconds(0.1));
				}
			}
			finally
			{
				_rw.ExitReadLock();
			}
		}
	}

	static void Write(string threadName)
	{
		while (true)
		{
			try
			{
				int newKey = new Random().Next(250);

				//Upgradeable mode allows the thread to upgrade the read lock as needed, without risk of deadlocks.

				//Use upgradeable mode when a thread usually accesses the resource that is protected 
				//by the ReaderWriterLockSlim in read mode, but may need to enter write mode 
				//if certain conditions are met. A thread in upgradeable mode can 
				//downgrade to read mode or upgrade to write mode.

				//Only one thread can enter upgradeable mode at any given time. 
				//If a thread is in upgradeable mode, and there are no threads waiting to enter write mode,
				// any number of other threads can enter read mode,
				// even if there are threads waiting to enter upgradeable mode.

				//If one or more threads are waiting to enter write mode, 
				//a thread that calls the EnterUpgradeableReadLock method blocks
				//until those threads have either timed out or entered write mode and then exited from it.

				_rw.EnterUpgradeableReadLock();
				if (!_items.ContainsKey(newKey))
				{
					try
					{
						_rw.EnterWriteLock();
						_items[newKey] = 1;
						WriteLine($"New key {newKey} is added to a dictionary by a {threadName}");
					}
					finally
					{
						_rw.ExitWriteLock();
					}
				}
				Sleep(TimeSpan.FromSeconds(0.1));
			}
			finally
			{
				_rw.ExitUpgradeableReadLock();
			}
		}
	}
}

// 作业2: 设计一个实验说明EnterUpgradeableReadLock的优势