using System;
using System.Threading;
using static System.Console;
using static System.Threading.Thread;
class Program
{
	class ThreadSample
	{
		private readonly int _iterations;
		public ThreadSample(int iterations)
		{
			_iterations = iterations;
		}
		public void CountNumbers()
		{
			for (int i = 1; i <= _iterations; i++)
			{
				Sleep(TimeSpan.FromSeconds(0.5));
                WriteLine($"{CurrentThread.Name} prints {i}");
            }
		}
	}
	static void Count(object iterations)
	{
		CountNumbers((int)iterations);
	}
	static void CountNumbers(int iterations)
	{
		for (int i = 1; i <= iterations; i++)
		{
			Sleep(TimeSpan.FromSeconds(0.5));
			WriteLine($"{CurrentThread.Name} prints {i}");
		}
	}
	//(2)使用ref传递一下看看效果
	static void PrintNumber(ref int number)
	{
		WriteLine(number++);
		//WriteLine(number);
	}
	static void Main(string[] args)
	{
		Test();
		
		var sample = new ThreadSample(10);

		var threadOne = new Thread(sample.CountNumbers);
		threadOne.Name = "ThreadOne";
		threadOne.Start();
		threadOne.Join();

		WriteLine("--------------------------");

		var threadTwo = new Thread(Count);
		threadTwo.Name = "ThreadTwo";
		threadTwo.Start(8);
		threadTwo.Join();

		WriteLine("--------------------------");

		var threadThree = new Thread(() => CountNumbers(12));
		threadThree.Name = "ThreadThree";
		threadThree.Start();
		threadThree.Join();
		WriteLine("--------------------------");

		int i = 10;
		var threadFour = new Thread(() => PrintNumber(ref i));
		i = 20;
		var threadFive = new Thread(() => PrintNumber(ref i));
		threadFour.Start(); 
		threadFive.Start();
	}
	//(1)使用out参数实验
	static void TestOut(out int o)
	{
		o = 1;

		//使用未赋值的参数
		//所有路径都需要设定
		
		int n = o;

		//o = 1;
	}
	//(3)参数数组
	static void AnyNumberInts(params int[] intArray)
	{
		WriteLine($"intArray: {intArray.Length}");
		foreach (var i in intArray)
		{
			WriteLine($"Value:{i}");
		}
	}
	//(4)可选参数与命名参数
	static void TestSome(int x,int y,int z = 2022)
	{
		WriteLine($"TestSome x: {x} y :{y} z: {z}");
	}	

	static void Test()
	{
		//AnyNumberInts();
		//AnyNumberInts(1,2,3,4,5);
		
		//TestSome(1,2);
		//TestSome(1,2,3);
		TestSome(y:500,x:100);
	}
}
