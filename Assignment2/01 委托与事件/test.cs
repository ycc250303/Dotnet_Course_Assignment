using System;

//typedef void (*CFUN)(const char* name);
//CFUN fn = cfun;

namespace Test
{
	public delegate void GreetingDelegate(string name);

	public interface IGreeting
	{
		void GreetingPeople(string name);
	}
	public class EnglishGreeting_cls : IGreeting
	{
		public void GreetingPeople(string name)
		{
			Console.WriteLine("Morning: " + name);
		}
	}
	public class ChineseGreeting_cls : IGreeting
	{
		public void GreetingPeople(string name)
		{
			Console.WriteLine("早上好: " + name);
		}
	}

	public class Program
	{
		static void EnglishGreeting(string name)
		{
			Console.WriteLine("Morning: " + name);
		}
		
		static void ChineseGreeting(string name)
		{
			Console.WriteLine("早上好: " + name);
		}

		static void GreetPeople(string name, GreetingDelegate fn)
		{
			fn(name);
		}

		static void GreetPeopleWithInterface(string name, IGreeting makegreeting)
		{
			makegreeting.GreetingPeople(name);
		}

		static void TestInterface()
		{
			GreetPeopleWithInterface("Fred", new EnglishGreeting_cls());
			GreetPeopleWithInterface("Fred", new ChineseGreeting_cls());
		}

		static void Main()
		{
			TestInterface();
		}
	}
}

