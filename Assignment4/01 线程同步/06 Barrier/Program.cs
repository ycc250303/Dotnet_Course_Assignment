using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;


class Program
{
	static void Main(string[] args)
	{
		var t1 = new Thread(() => PlayMusic("the guitarist", "play an amazing solo", 5));
		var t2 = new Thread(() => PlayMusic("the singer", "sing his song", 2));

		t1.Start();
		t2.Start();
	}

	//指定同步两个线程,
	static Barrier _barrier = new Barrier(2,
							b => WriteLine($"End of phase {b.CurrentPhaseNumber + 1}"));

	static void PlayMusic(string name, string message, int seconds)
	{
		for (int i = 1; i < 30; i++)
		{
			WriteLine("----------------------------------------------");
			Sleep(TimeSpan.FromSeconds(seconds));
			WriteLine($"{name} starts to {message}");
			Sleep(TimeSpan.FromSeconds(seconds));
			WriteLine($"{name} finishes to {message}");

			//在SignalAndWait处停下来,直到两个人都到了，才开始下一个回合
			_barrier.SignalAndWait();
		}
	}
}