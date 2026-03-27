public class Program
{
    //依赖选项
    //-checked[+|-]                 生成溢出检查
    //csc Program.cs -checked+
    //csc Program.cs -checked-

    public static void Main()
    {
        // int.MaxValue equals 2147483647
        int n = int.MaxValue;
        System.Console.WriteLine($"MaxValue: {n}");
        n = n + 1;
        System.Console.WriteLine($"MaxValue+1: {n}");
    }
    /*
    public static void Main()
    {
        checked
        {
            // int.MaxValue equals 2147483647
            int n = int.MaxValue;
            n = n + 1;
            System.Console.WriteLine(n);
        }
    }
    */
   
    //不受编译器选项的影响
    /*
    public static void Main()
    {
        unchecked
        {
            // int.MaxValue equals 2147483647
            int n = int.MaxValue;
            n = n + 1;
            System.Console.WriteLine(n);
        }
    }
    */
}