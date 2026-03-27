using System;
using System.ComponentModel;

public sealed class Program
{
    //1 多异常类型捕获
    //2 When子句
    public static void Main(string[] args)
    {
        try
        {
            throw new Win32Exception(5);
            //throw new InvalidOperationException("Arbitrary exception");
        }
        catch(Win32Exception exception) when(0x80004005 == (uint)exception.ErrorCode)
        {
            Console.WriteLine($"Win32Exception: {exception.ErrorCode}");
        }
        catch(NullReferenceException exception)
        {
            Console.WriteLine($"NullReferenceException: {exception}");
        }
        catch(ArgumentException exception)
        {
             Console.WriteLine($"ArgumentException: {exception}");
        }
        catch(InvalidOperationException exception)
        {
            Console.WriteLine($"InvalidOperationException: {exception}");
        }
        catch (SystemException excpetion)
        {
            Console.WriteLine($"SystemException: {excpetion}");
            Win32Exception we = excpetion as Win32Exception;
            if (we != null)
            {
                Console.WriteLine($"Win32Exception Code: {we.ErrorCode}");
            }
        }
        catch(Exception exception)
        {
            // Handle Exception
            Console.WriteLine($"Exception:{exception}");
        }
        finally
        {
            // Handle any cleanup code here as it runs
            // regardless of whether there is an exception
            Console.WriteLine("In Finally");
        }
    }
}