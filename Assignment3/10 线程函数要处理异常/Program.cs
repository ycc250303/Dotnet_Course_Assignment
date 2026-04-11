using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;

class Program
{
	static void BadFaultyThread()
	{
		WriteLine("Starting a faulty thread...");
		Sleep(TimeSpan.FromSeconds(2));
		throw new Exception("Boom!");
	}
	static void FaultyThread()
	{
		try
		{
			WriteLine("Starting a faulty thread...");
			Sleep(TimeSpan.FromSeconds(1));
			throw new Exception("Boom!");
		}
		catch (Exception ex)
		{
			WriteLine($"Exception handled: {ex.Message}");
		}
	}

	static void Main(string[] args)
	{
		var t = new Thread(FaultyThread);
		t.Start();
		t.Join();
		WriteLine("----------------------");
		//无法被捕捉到新线程里面的异常
		try
		{
			t = new Thread(BadFaultyThread);
			t.Start();
			t.Join();
		}
		catch (Exception ex)
		{
			WriteLine($"We won't get here! {ex}");
		}
		//异常导致进程结束，这里无法执行
		WriteLine("Main End.");
	}
}
