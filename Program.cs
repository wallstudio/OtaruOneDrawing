using System;
using System.Linq;
using SixLabors.ImageSharp;
using System.Globalization;

namespace MakiOneDrawingBot
{
    // https://www.slideshare.net/ngzm/oauth-10-oauth-20-openid-connect

    static class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var command = args.SkipWhile(a => a != "--command").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--command");
            var actions = new Actions(
                twitterApiKey: args.SkipWhile(a => a != "--twitter-api-key").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--twitter-api-key"),
                twitterApiSecret: args.SkipWhile(a => a != "--twitter-api-secret").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--twitter-api-secret"),
                bearerToken: args.SkipWhile(a => a != "--bearer-token").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--bearer-token"),
                accessToken: args.SkipWhile(a => a != "--access-token").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--access-token"),
                accessTokenSecret: args.SkipWhile(a => a != "--access-token-secret").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--access-token-secret"),
                googleServiceAccountJwt: args.SkipWhile(a => a != "--google-service-account-jwt").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--google-service-account-jwt"),
                date: args.SkipWhile(a => a != "--date").Skip(1).FirstOrDefault(),
                next: args.SkipWhile(a => a != "--next").Skip(1).FirstOrDefault(),
                general: args.SkipWhile(a => a != "--general").Skip(1).FirstOrDefault());

            switch (command)
            {
                case nameof(Actions.NotificationMorning):
                    actions.NotificationMorning();
                    break;
                case nameof(Actions.NotificationStart):
                    actions.NotificationStart();
                    break;
                case nameof(Actions.NotificationFinish):
                    actions.NotificationFinish();
                    break;
                case nameof(Actions.AccumulationPosts):
                    actions.AccumulationPosts();
                    break;
                case "Interactive":
                    var newCommand = Console.ReadLine();
                    Main(args.Select(a => a == command ? newCommand : a).ToArray());
                    break;
                case "TestSequential":
                    actions.NotificationMorning();
                    actions.NotificationStart();
                    actions.NotificationFinish();
                    actions.AccumulationPosts();
                    break;
                default:
                    throw new ArgumentException($"--command={command}");
            }
        }
    }
}
