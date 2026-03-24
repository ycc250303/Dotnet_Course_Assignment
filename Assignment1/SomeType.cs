using System;
//(1)类
public class SomeType
{
    //(2)嵌套类
    class SomeNestType
    {
        public void f() { Console.WriteLine("f"); }
    }
    //(3)常数
    const Int32 SomeConstant = 1000;
    //(4)只读
    public readonly Int32 SomereadOnlyFiled = 2;
    //(5)静态
    static Int32 SomeReadWriteFiled = 3;
    //(6)类型构造器
    static SomeType()
    {
        Console.WriteLine("Static SomeType");
    }
    //(7)实例构造
    public SomeType()
    {
        Console.WriteLine("Inst SomeType");
        SomereadOnlyFiled = 100;
    }
    public SomeType(Int32 x)
    { }
    //(8)析构、终结器
    ~SomeType()
    {
        Console.WriteLine("~SomeType");
    }
    //(9)实例方法 静态方法
    public void f()
    {
        //SomereadOnlyFiled = 100;
    }
    public override string ToString()
    {
        return "SomeTypeToStringVal";
    }
    static void Main() { }
    //(11)实例属性
    int II
    {
        get; set;
    }
    int F
    {
        get
        {
            return SomereadOnlyFiled;
        }
    }
    //(12)实例索引器
    public int this[int v]
    {
        get
        {
            int[] bb = { 55, 33, 44 };
            return bb[v];
        }
    }
    //(13)实例事件
    public event EventHandler SomeEvent;
}
//(14)扩展方法
public static class SomeExtent
{
    public static void MyF(this SomeType e, int nn)
    {
        Console.WriteLine("MyF {0} {1}", nn, e.SomereadOnlyFiled);
    }

}