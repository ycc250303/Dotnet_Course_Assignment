using System;
using System.Runtime.Serialization;

//5个构造函数
class DatabaseException : Exception
{
    /*
    public DatabaseException(
        string message,
        System.Data.SqlClient.SQLException? exception)
        : base(message, innerException: exception)
    {
        // ...
    }

    public DatabaseException(
        string message,
        System.Data.OracleClient.OracleException? exception)
        : base(message, innerException: exception)
    {
        // ...
    }
    */

    public DatabaseException()
    {
        // ...
    }

    public DatabaseException(string message)
        : base(message)
    {
        // ...
    }

    public DatabaseException(
        string message, Exception exception)
        : base(message, innerException: exception)
    {
        // ...
    }

    static void Main()
    {
       throw new DatabaseException("Error");
    }
}