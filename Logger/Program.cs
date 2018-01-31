using System;

namespace Logger
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // set path of an exception file
            var path = "";

            var ex = ExceptionSerializer.Deserialize(path);

            // put a debug node here to examine the exception
            Console.WriteLine("done!");
        }
    }
}