using System;
using System.Threading;
//(1)静态的using声明
using static System.Console;
class Program
{
	//(2)表达式方法体
	static public bool Compare(int x,int y)
	{
		return x == y;
	}
	static public bool Compare2(int x,int y) => x == y;
	class SomeFeature
	{
		//(3)表达式属性,get存取器的单行属性
		string name = "myName";
		public string FullName
		{
			get
			{
				return name;
			}
		}
		public string FullName2 => name;
		//(4)自动实现的初始化器
		public SomeFeature()
		{
			Age = 18;
		}
		public int Age {get;set;}
		public int Age2 {get;set;} = 19;
	}
	static void Main(string[] args)
	{
		//(1)静态的using声明
		WriteLine("Can ignore Console.");	
		//(2)表达式方法体	
		WriteLine($"Compare {Compare(1,1)}");
		WriteLine($"Compare2 {Compare2(1,1)}");
		//(3)表达式属性,get存取器的单行属性
		SomeFeature sf = new SomeFeature();
		WriteLine($"SomeFeature {sf.FullName} {sf.FullName2}");

		//(4)自动实现的初始化器
		WriteLine($"SomeFeature {sf.Age} {sf.Age2}");

		//(5)空值传播运算符
		int? age = sf?.Age;
		WriteLine($"age: {age}");
		sf = null;
		age = sf?.Age;
		WriteLine($"age: {age}");
		if (sf == null)
		{
			age = null;
		}
		else
		{
			age = sf.Age;
		}
		WriteLine($"age: {age}");
	}
}