using System;

//方法 字段 类型
//private、protected、public、internal

//仅仅类型
//abstract、sealed

//字段
//static、readonly

//方法修饰
//实例方法
//static、virtual 、new、override、abstract、sealed

abstract class BaseClass
{
	public virtual void vf()
	{
		Console.WriteLine("In BaseClass vf");
	}

	public virtual void vf_new()
	{
		Console.WriteLine("In BaseClass vf_new");
	}

	public abstract void vf_abstract();

	public sealed override string ToString()
	{
		Console.WriteLine("In BaseClass vf_sealed");
		return "BaseClass_ToString";
	}
}

internal class DerivedClass : BaseClass
{
	public override void vf()
	{
		Console.WriteLine("In DerivedClass vf");
	}
	public new void vf_new()
	{
		Console.WriteLine("In DerivedClass vf_new");
	}
	public override void vf_abstract()
	{
		Console.WriteLine("In DerivedClass vf_abstract");
	}
	/*
	public sealed override string ToString()
	{
		Console.WriteLine("In BaseClass vf_sealed");
		return "BaseClass_ToString";
	}
	*/
}

class Test
{
	//泛型应用于接口与委托 【简单语法】
	//不支持 协变[out]/ 逆变[in]的问题？

	delegate void TestDelegate<T>(T t);
	//delegate void TestDelegate<in T>(T t);

	static void BaseFn(BaseClass b)
	{
		Console.WriteLine("In BaseFn");
		b.vf();
	}
	static void DerivedFn(DerivedClass d)
	{}

	static void Main()
	{
		System.Console.WriteLine("Hi");

		DerivedClass o2 = new DerivedClass();
		//一个子类的引用赋值给子类的引用
		BaseClass    o  = o2;

		o.vf();
		o.vf_new();
		o.vf_abstract();
		o.ToString();

		o2.vf();
		o2.vf_new();
		o2.vf_abstract();
		o2.ToString();

		//泛型与委托
		TestDelegate<BaseClass>    bfn =  BaseFn;
		TestDelegate<DerivedClass> dfn =  DerivedFn;
		
		//这两个类型之间没啥关系
		//这是泛型带来的问题
		//所以提出一个变体(协变[out]/ 逆变[in])来解决这个问题
		//协变out:  父类【接口或者委托】 <= 子类
		//逆变in :  子类【接口或者委托】 <= 父类

		//理解原理
		//问题:
		//bfn = dfn;
		//dfn = bfn;
		//dfn(o2);
	}
}