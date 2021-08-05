using System;
using System.Collections;
using System.Linq;

namespace MakiOneDrawingBot
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var env in args)
            {
                Console.WriteLine($"arg:{env}");
            }
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                Console.WriteLine($"{env.Key}={env.Value}");
            }

            var command = args.SkipWhile(a => a != "--command").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--command");
            var twitterApiKey = args.SkipWhile(a => a != "--twitter-api-key").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--twitter-api-key");
            var twitterApiSecret = args.SkipWhile(a => a != "--twitter-api-secret").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--twitter-api-secret");

            switch (command)
            {
                case "NotificationStart":
                    NotificationStart();
                    return;
                case "AccumulationPosts":
                    AccumulationPosts();
                    return;
                case "NotificationFinish":
                    NotificationFinish();
                    return;
                case "NotificationMorning":
                    NotificationMorning();
                    return;
                default:
                    throw new ArgumentException($"--command={command}");
            }
        }

        static void NotificationStart() => Console.WriteLine(nameof(NotificationStart));
        static void AccumulationPosts() => Console.WriteLine(nameof(AccumulationPosts));
        static void NotificationFinish() => Console.WriteLine(nameof(NotificationFinish));
        static void NotificationMorning() => Console.WriteLine(nameof(NotificationMorning));
    }
}
