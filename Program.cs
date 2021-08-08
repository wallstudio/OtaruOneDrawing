using System;
using System.Collections;
using System.Linq;
using CoreTweet;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            var actions = new Actions(
                twitterApiKey: args.SkipWhile(a => a != "--twitter-api-key").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--twitter-api-key"),
                twitterApiSecret: args.SkipWhile(a => a != "--twitter-api-secret").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--twitter-api-secret"),
                bearerToken: args.SkipWhile(a => a != "--bearer-token").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--bearer-token"),
                accessToken: args.SkipWhile(a => a != "--access-token").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--access-token"),
                accessTokenSecret: args.SkipWhile(a => a != "--access-token-secret").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--access-token-secret"));

            switch (command)
            {
                case "NotificationMorning":
                    actions.NotificationMorning();
                    break;
                case "NotificationStart":
                    actions.NotificationStart();
                    break;
                case "NotificationFinish":
                    actions.NotificationFinish();
                    break;
                case "AccumulationPosts":
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

    class Actions
    {
        readonly string HASH_TAG = "&弦巻マキ深夜の真剣お絵描き60分勝負";
        readonly string HELP_URL = "https://example.com";
        readonly string twitterApiKey;
        readonly string twitterApiSecret;
        readonly string bearerToken;
        readonly string accessToken;
        readonly string accessTokenSecret;
        readonly Tokens tokens;

        public Actions(string twitterApiKey, string twitterApiSecret, string bearerToken, string accessToken, string accessTokenSecret)
        {
            this.twitterApiKey = twitterApiKey;
            this.twitterApiSecret = twitterApiSecret;
            this.bearerToken = bearerToken;
            this.accessToken = accessToken;
            this.accessTokenSecret = accessTokenSecret;
            tokens = Tokens.Create(twitterApiKey, twitterApiSecret, accessToken, accessTokenSecret);
        }

        /// <summary>
        /// 朝の予告ツイートを投げる
        /// </summary>
        public void NotificationMorning()
        {
            // TODO: Read theme from DB
            var theme1 = "";
            var theme2 = "";
            var uploadResult = tokens.Media.Upload(new FileInfo("lenna.png"));
            tokens.Statuses.Update(
                status: $@"
{HASH_TAG}
今夜のわんどろのテーマ発表！

今回のお題はこちらの二つ！
「{theme1}」
「{theme2}」

▼イベントルール詳細
{HELP_URL}
                ".Trim(),
                media_ids: new []{ uploadResult.MediaId },
                auto_populate_reply_metadata: true);
        }

        /// <summary>
        /// ワンドロ開始のツイートを投げる
        /// </summary>
        public void NotificationStart()
        {
            var me = tokens.Account.VerifyCredentials();
            var mooning = EnumerateSearchTweets(
                q: $"{HASH_TAG} 今夜のわんどろのテーマ発表 from:{me.ScreenName} exclude:retweets",
                result_type: "recent",
                until: (DateTime.Now.Date - TimeSpan.FromDays(1) + TimeSpan.FromHours(22)).ToUniversalTime().ToString("yyy-MM-dd"),
                count: 100).FirstOrDefault();

            // TODO: Read theme from DB
            var theme1 = "";
            var theme2 = "";
            var uploadResult = tokens.Media.Upload(new FileInfo("lenna.png"));
            tokens.Statuses.Update(
                status: $@"
{HASH_TAG}
わんどろスタート！(｀・ω・´）

今回のお題はこちらの二つ！
投稿時はタグを忘れないでくださいね！！
「{theme1}」
「{theme2}」

▼イベントルール詳細
{HELP_URL}
                ".Trim(),
                media_ids: new []{ uploadResult.MediaId },
                in_reply_to_status_id: mooning?.Id,
                auto_populate_reply_metadata: true);
        }

        /// <summary>
        /// ワンドロ終了のツイートを投げる
        /// </summary>
        public void NotificationFinish()
        {
            var me = tokens.Account.VerifyCredentials();
            var start = EnumerateSearchTweets(
                q: $"{HASH_TAG} わんどろスタート from:{me.ScreenName} exclude:retweets",
                result_type: "recent",
                until: (DateTime.Now.Date - TimeSpan.FromDays(1) + TimeSpan.FromHours(22)).ToUniversalTime().ToString("yyy-MM-dd"),
                count: 100).FirstOrDefault();

            var next = DateTime.Now.Date;
            while(next.Day % 3 != 0) next += TimeSpan.FromDays(1);
            tokens.Statuses.Update(
                status: $@"
{HASH_TAG}
わんどろ終了ーー！！( ´ ∀`)ﾉA

投稿いただいたイラストは明日のお昼にRTします！！
次回は {next:MM/dd\(ddd\)} の予定です、お楽しみに！！

▼イベントルール詳細
{HELP_URL}
                ".Trim(),
                in_reply_to_status_id: start?.Id,
                // attachment_url: null, // 引用
                auto_populate_reply_metadata: true);
        }

        /// <summary>
        /// 投稿を集計してRTとランキングを更新する
        /// </summary>
        public void AccumulationPosts()
        {
            var me = tokens.Account.VerifyCredentials();
            var tweets = EnumerateSearchTweets(
                q: $"{HASH_TAG} exclude:retweets", // https://gist.github.com/cucmberium/e687e88565b6a9ca7039
                result_type: "recent",
                until: (DateTime.Now.Date - TimeSpan.FromDays(1) + TimeSpan.FromHours(22)).ToUniversalTime().ToString("yyy-MM-dd"),
                count: 100).ToArray();

            var finish = EnumerateSearchTweets(
                q: $"{HASH_TAG} わんどろ終了 from:{me.ScreenName} exclude:retweets",
                result_type: "recent",
                until: (DateTime.Now.Date - TimeSpan.FromDays(1) + TimeSpan.FromHours(22)).ToUniversalTime().ToString("yyy-MM-dd"),
                count: 100).FirstOrDefault();
            var next = DateTime.Now.Date;
            while(next.Day % 3 != 0) next += TimeSpan.FromDays(1);
            tokens.Statuses.Update(
                status: (tweets.Length > 0
                    ? $@"
{HASH_TAG}
昨日のわんどろの投稿イラストをRTします！！！(ﾟ∇^*)

次回は {next:MM/dd\(ddd\)} の予定です、お楽しみに！！

▼イベントルール詳細
{HELP_URL}
                    "
                    : $@"
{HASH_TAG}
昨日のわんどろの投稿イラストをRT……
って、誰も投稿してくれなかったみたい…(´；ω；｀)

次回は {next:MM/dd\(ddd\)} の予定です、よろしくおねがいします。

▼イベントルール詳細
{HELP_URL}
                ").Trim(),
                in_reply_to_status_id: finish?.Id,
                // attachment_url: null, // 引用
                auto_populate_reply_metadata: true);

            foreach (var tweet in tweets)
            {
                // tokens.Favorites.Create(tweet.Id);
                // tokens.Statuses.Retweet(tweet.Id);
            }

            var followees = tokens.Friends.EnumerateIds(EnumerateMode.Next, user_id: (long)me.Id, count: 5000).ToArray();
            foreach (var id in tweets.Select(s => s.User.Id).OfType<long>().Distinct().Where(id => !followees.Contains(id)))
            {
                // tokens.Friendships.Create(user_id: id, follow: true);
            }

            // TODO: Read from DB
            // TODO: Aggregate
            // TODO: Write to DB + Doc
        }

        IEnumerable<Status> EnumerateSearchTweets(string q, string geocode = null, string lang = null, string locale = null, string result_type = null, int? count = null, string until = null, long? since_id = null, long? max_id = null, bool? include_entities = null, bool? include_ext_alt_text = null, TweetMode? tweet_mode = null)
        {
            do
            {
                var r = tokens.Search.Tweets(q, geocode, lang, locale, result_type, count, until, since_id, max_id, include_entities, include_ext_alt_text, tweet_mode);
                max_id = string.IsNullOrEmpty(r.SearchMetadata.NextResults) ? null
                    : long.Parse(Regex.Match(r.SearchMetadata.NextResults, $"{nameof(max_id)}=(?<{nameof(max_id)}>[0-9]+)").Groups[$"{nameof(max_id)}"].Value);
                foreach (var s in r) yield return s;
            }
            while(max_id != null);
        }

    }
}
