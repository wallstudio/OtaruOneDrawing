using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace MakiOneDrawingBot
{
    // https://www.slideshare.net/ngzm/oauth-10-oauth-20-openid-connect

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
            var bearerToken = args.SkipWhile(a => a != "--bearer-token").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--bearer-token");
            var accessToken = args.SkipWhile(a => a != "--access-token").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--access-token");
            var accessTokenSecret = args.SkipWhile(a => a != "--access-token-secret").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--access-token-secret");

            switch (command)
            {
                case "Interactive":
                    var newCommand = Console.ReadLine();
                    Main(args.Select(a => a == command ? newCommand : a).ToArray());
                    return;
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

        /// <summary>
        /// 朝の予告ツイートを投げる
        /// </summary>
        static void NotificationMorning() => Console.WriteLine(nameof(NotificationMorning));

        /// <summary>
        /// ワンドロ開始のツイートを投げる
        /// </summary>
        static void NotificationStart() => Console.WriteLine(nameof(NotificationStart));

        /// <summary>
        /// ワンドロ終了のツイートを投げる
        /// </summary>
        static void NotificationFinish() => Console.WriteLine(nameof(NotificationFinish));

        /// <summary>
        /// 投稿を集計してRTとランキングを更新する
        /// </summary>
        static void AccumulationPosts() => Console.WriteLine(nameof(AccumulationPosts));
    }
}
