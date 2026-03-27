using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Threading;

public sealed class Program
{
    //1 引发异常而不替换栈信息
    //2 熟悉异步函数的执行
    static public string FormatBytes(long bytes)
    {
        string[] magnitudes = new string[] { "GB", "MB", "KB", "Bytes" };
        long max            =  (long)Math.Pow(1024, magnitudes.Length);

        return string.Format("Result {1:##.##} {0}", magnitudes.FirstOrDefault(magnitude =>
                 bytes > (max /= 1024)) ?? "0 Bytes",
                (decimal)bytes / (decimal)max).Trim();
    }

    private static async Task AwaitWriteWebRequestSize(string url)
    {
        WebRequest webRequest = WebRequest.Create(url);

        Console.WriteLine($"---1--- TID: {Thread.CurrentThread.ManagedThreadId} InPool: {Thread.CurrentThread.IsThreadPoolThread}");
        WebResponse response  = await webRequest.GetResponseAsync();
        Console.WriteLine($"---2--- TID: {Thread.CurrentThread.ManagedThreadId} InPool: {Thread.CurrentThread.IsThreadPoolThread}");
        StreamReader reader = new StreamReader(response.GetResponseStream());
        
        string text = await reader.ReadToEndAsync();
        Console.WriteLine($"---3--- TID: {Thread.CurrentThread.ManagedThreadId} InPool: {Thread.CurrentThread.IsThreadPoolThread}");

        Console.WriteLine("");
        Console.WriteLine(FormatBytes(text.Length));
        
        reader.Dispose();
    }

    public static void Main(string[] args)
    {
        string url = "http://www.baidu.com";
        if (args.Length > 0)
        {
            url = args[0];
        }
        Console.WriteLine(url);
       
        Task task = AwaitWriteWebRequestSize(url);
        
        try
        {
            try
            {
                while (!task.Wait(100))
                {
                    Console.Write(".");
                }
            }
            catch (AggregateException exception)
            {
                exception = exception.Flatten();
                ExceptionDispatchInfo.Capture(exception.InnerException??exception).Throw();
                throw;
            }
            Console.WriteLine("-----------End 1------------");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }

        Console.WriteLine("-----------End 2------------");
    }
}