using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

class Program
{
	static volatile bool _isCompleted = false;

	static void UserModeWait()
	{
		long count = 0;
		while (!_isCompleted)
		{
			//Write(".");
			count++;
		}
		WriteLine();
		WriteLine($"UserModeWait Waiting is complete: {count}");
	}

	static void HybridSpinWait()
	{
		long count = 0;
		var w = new SpinWait();
		while (!_isCompleted)
		{
			w.SpinOnce();
			//WriteLine(w.NextSpinWillYield);
			count++;
		}
		WriteLine($"HybridSpinWait Waiting is complete : {count}");
	}

	static void Main(string[] args)
	{
		var t1 = new Thread(UserModeWait);
		var t2 = new Thread(HybridSpinWait);
		WriteLine("Running user mode waiting");
		t1.Start();
		Sleep(10000);
		_isCompleted = true;
		Sleep(TimeSpan.FromSeconds(1));
		_isCompleted = false;
		WriteLine("Running hybrid SpinWait construct waiting");
		t2.Start();
		Sleep(10000);
		_isCompleted = true;
		Sleep(1000);
		WriteLine("Main end");
	}
}