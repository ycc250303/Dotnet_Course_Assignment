using System;

public class Program
{
    //(0)抛出异常，异常类型与信息尽可能具体，
    //【不要引发NullReferenceException、SystemException、Exception、ApplicationException】
    //(1)异常捕捉调换位置怎样？
    //(2)nameof
    public static int Parse(string textDigit)
    {
        string[] digitTexts = 
            { "zero", "one", "two", "three", "four", 
              "five", "six", "seven", "eight", "nine" };

        int result = Array.IndexOf(digitTexts, textDigit.ToLower());

        if(result < 0)
        {
            throw new ArgumentException("The argument did not represent a digit", nameof(textDigit));
        }

        return result;
    }

    static void Main()
    {
        try
        {
            int nIndex = Parse("9");
            Console.WriteLine($"Parse nIndex : {nIndex}");
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine($"ArgumentException: {ex}");
        }
        //如果将这个异常捕捉放到前面如何?
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }
}