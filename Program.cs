using System;
using System.Collections;

namespace MakiOneDrawingBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            foreach (var env in args)
            {
                Console.WriteLine($"arg:{env}");
            }
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                Console.WriteLine($"{env.Key}={env.Value}");
            }
        }
    }
}
