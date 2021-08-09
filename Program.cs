using System;
using System.Collections;
using System.Linq;
using CoreTweet;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using System.Threading;
using System.Text;
using Color = SixLabors.ImageSharp.Color;
using System.Globalization;

namespace MakiOneDrawingBot
{
    // https://www.slideshare.net/ngzm/oauth-10-oauth-20-openid-connect

    class Program
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
                googleServiceAccountJwt: args.SkipWhile(a => a != "--google-service-account-jwt").Skip(1).FirstOrDefault() ?? throw new ArgumentException("--google-service-account-jwt"));

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
        readonly string DB_SHEET_ID = "1Un15MnW9Z2ChwSdsxdAVw495uSmJN4jBHngcBpYxo_0";
        readonly string HASH_TAG = "#者犬葉当夜位乃思遣於介火器99分聖父";
        // readonly string HASH_TAG = "#弦巻マキ深夜の真剣お絵描き60分勝負";
        readonly string HELP_URL = "https_example_com";
        readonly string twitterApiKey;
        readonly string twitterApiSecret;
        readonly string bearerToken;
        readonly string accessToken;
        readonly string accessTokenSecret;
        readonly string googleServiceAccountJwt;
        readonly Tokens tokens;

        DateTime JpNow => DateTime.UtcNow + TimeSpan.FromHours(+9);
        DateTime JpNowDate => JpNow.Date;

        public Actions(string twitterApiKey, string twitterApiSecret, string bearerToken, string accessToken, string accessTokenSecret, string googleServiceAccountJwt)
        {
            this.twitterApiKey = twitterApiKey;
            this.twitterApiSecret = twitterApiSecret;
            this.bearerToken = bearerToken;
            this.accessToken = accessToken;
            this.accessTokenSecret = accessTokenSecret;
            this.googleServiceAccountJwt = Encoding.UTF8.GetString(Convert.FromBase64String(googleServiceAccountJwt));
            tokens = Tokens.Create(twitterApiKey, twitterApiSecret, accessToken, accessTokenSecret);
        }

        /// <summary>
        /// 朝の予告ツイートを投げる
        /// </summary>
        public void NotificationMorning()
        {
            using var tables = GetTables();

            // Create new schedule
            var schedules = tables.First(tbl => tbl.Name == "schedule");
            var schedule = schedules.Add(JpNowDate.ToString("yyyyMMdd"));
            var unusedTheme = tables
                .First(tbl => tbl.Name == "theme")
                .Where(thm => !schedules.Any(ev => ev["id_theme"] == thm["id"]));
            var theme = unusedTheme
                .Where(thm => !DateTime.TryParse(thm["date"], out var date) || date == JpNowDate) // 別の日を除外
                .OrderByDescending(thm => DateTime.TryParse(thm["date"], out var date) && date == JpNowDate)
                .First();
            schedule["id_theme"] = theme["id"];
            schedule["date"] = JpNowDate.ToString("yyyy/MM/dd");

            // Post tweet
            var theme1 = tables.First(tbl => tbl.Name == "theme").First(thm => thm["id"] == schedule["id_theme"])["theme1"];
            var theme2 = tables.First(tbl => tbl.Name == "theme").First(thm => thm["id"] == schedule["id_theme"])["theme2"];
            var uploadResult = tokens.Media.Upload(CreateTextImage($"{theme1}\n\n{theme2}"));
            var morning = tokens.Statuses.Update(
                status: $@"
{HASH_TAG}
今夜のわんどろのテーマ発表！

今回のお題はこちらの二つ！
「{theme1}」
「{theme2}」

▼イベントルール詳細
{HELP_URL}
                ".Trim(),
                media_ids: new[] { uploadResult.MediaId },
                auto_populate_reply_metadata: true);

            // Record
            schedule["id_morning_status"] = morning.Id.ToString();
            schedule["ts_morning_status"] = JpNow.ToString();
            schedule["ts_utc_morning_status"] = DateTime.UtcNow.ToString();
        }


        /// <summary>
        /// ワンドロ開始のツイートを投げる
        /// </summary>
        public void NotificationStart()
        {
            using var tables = GetTables();
            var schedule = tables
                .First(tbl => tbl.Name == "schedule")
                .First(sch => sch["id"] == JpNowDate.ToString("yyyyMMdd"));

            // Post tweet
            var theme1 = tables.First(tbl => tbl.Name == "theme").First(thm => thm["id"] == schedule["id_theme"])["theme1"];
            var theme2 = tables.First(tbl => tbl.Name == "theme").First(thm => thm["id"] == schedule["id_theme"])["theme2"];
            var uploadResult = tokens.Media.Upload(CreateTextImage($"{theme1}\n\n{theme2}"));
            var start = tokens.Statuses.Update(
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
                in_reply_to_status_id: long.Parse(schedule["id_morning_status"]),
                auto_populate_reply_metadata: true);

            // Record
            schedule["id_start_status"] = start.Id.ToString();
            schedule["ts_start_status"] = JpNow.ToString();
            schedule["ts_utc_start_status"] = DateTime.UtcNow.ToString();
        }

        /// <summary>
        /// ワンドロ終了のツイートを投げる
        /// </summary>
        public void NotificationFinish()
        {
            using var tables = GetTables();
            var schedule = tables
                .First(tbl => tbl.Name == "schedule")
                .First(sch => sch["id"] == JpNowDate.ToString("yyyyMMdd"));

            // Post tweet
            var next = JpNowDate + TimeSpan.FromDays(1);
            while(next.Day % 10 != 3) next += TimeSpan.FromDays(1);
            var finish = tokens.Statuses.Update(
                status: $@"
{HASH_TAG}
わんどろ終了ーー！！( ´ ∀`)ﾉA

投稿いただいたイラストは明日のお昼にRTします！！
次回は {next:MM/dd\(ddd\)} の予定です、お楽しみに！！

▼イベントルール詳細
{HELP_URL}
                ".Trim(),
                in_reply_to_status_id: long.Parse(schedule["id_start_status"]),
                // attachment_url: null, // 引用
                auto_populate_reply_metadata: true);
                
            // Record
            schedule["id_finish_status"] = finish.Id.ToString();
            schedule["ts_finish_status"] = JpNow.ToString();
            schedule["ts_utc_finish_status"] = DateTime.UtcNow.ToString();
        }

        /// <summary>
        /// 投稿を集計してRTとランキングを更新する
        /// </summary>
        public void AccumulationPosts()
        {
            using var tables = GetTables();
            var schedule = tables
                .First(tbl => tbl.Name == "schedule")
                // .First(sch => sch["id"] == (JpNowDate - TimeSpan.FromDays(1)).ToString("yyyyMMdd"));
                .First(sch => sch["id"] == JpNowDate.ToString("yyyyMMdd")); // TODO:

            // Collection
            var me = tokens.Account.VerifyCredentials();
            var since = DateTime.Parse(schedule["ts_utc_start_status"]) - TimeSpan.FromMinutes(15) - TimeSpan.FromDays(1); // TODO:
            // var since = DateTime.Parse(schedule["ts_utc_start_status"]) - TimeSpan.FromMinutes(15);
            var until = DateTime.Parse(schedule["ts_utc_finish_status"]) + TimeSpan.FromMinutes(15);
            var tweets = EnumerateSearchTweets(
                q: $"{HASH_TAG} -from:{me.ScreenName} exclude:retweets since:{since:yyy-MM-dd} until:{until:yyy-MM-dd}", // https://gist.github.com/cucmberium/e687e88565b6a9ca7039
                result_type: "recent",
                until: DateTime.UtcNow.ToString("yyy-MM-dd"),
                count: 100)
                .Where(twt => since <= twt.CreatedAt && twt.CreatedAt <= until)
                .ToArray();

            // Post tweet
            var next = JpNowDate;
            while(next.Day % 10 != 3) next += TimeSpan.FromDays(1);
            var preRetweet = tokens.Statuses.Update(
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
                in_reply_to_status_id: long.Parse(schedule["id_finish_status"]),
                // attachment_url: null, // 引用
                auto_populate_reply_metadata: true);

            // Record
            schedule["id_accumulation_status"] = preRetweet.Id.ToString();
            schedule["ts_accumulation_status"] = JpNow.ToString();
            schedule["ts_utc_accumulation_status"] = DateTime.UtcNow.ToString();

            // Twitter
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

            // Aggregate
            var posts = tables.Find(tbl => tbl.Name == "post");
            foreach (var tweet in tweets)
            {
                var post = posts.Add(tweet.Id.ToString());
                post["id_status"] = tweet.Id.ToString();
                post["id_schedule"] = schedule["id"];
                post["id_user"] = tweet.User.Id.ToString();
                post["user_display_name"] = tweet.User.Name;
                post["user_screen_name"] = tweet.User.ScreenName;
                post["url_user_icon"] = tweet.User.ProfileImageUrlHttps;
            }
            var postRanking = posts
                .GroupBy(pst => pst["id_user"])
                .Select(g => new { id = g.Key, posts = g, count = g.Count() })
                .OrderBy(info => info.count)
                .ToArray();
            schedule["ranking_post"] = string.Join(",", postRanking.Select(p => p.id));
            var entryRanking = posts
                .GroupBy(pst => pst["id_user"])
                .Select(g => new { id = g.Key, posts = g, count = g.Select(p => p["id_schedule"]).Distinct().Count() })
                .OrderBy(info => info.count)
                .ToArray();
            schedule["ranking_entry"] = string.Join(",", entryRanking.Select(p => p.id));
            var continueRanking = posts
                .GroupBy(pst => pst["id_user"])
                .Select(g =>
                {
                    var count = tables
                        .First(tbl => tbl.Name == "schedule")
                        .OrderByDescending(s => DateTime.Parse(s["date"]))
                        .TakeWhile(s => g.Any(pst => pst["id_schedule"] == s["id"]))
                        .Count();
                    return new { id = g.Key, posts = g, count };
                })
                .OrderBy(info => info.count)
                .ToArray();
            schedule["ranking_continue"] = string.Join(",", continueRanking.Select(p => p.id));
            
            // TODO: Write to Doc
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

        public byte[] CreateTextImage(string text)
        {
            using var image = Image.Load("image_template.png");
            image.Mutate(context =>
            {
                var font = new FontCollection().Install("font/Corporate-Logo-Rounded.ttf");
                var option = new DrawingOptions();
                option.TextOptions.VerticalAlignment = VerticalAlignment.Center;
                option.TextOptions.HorizontalAlignment = HorizontalAlignment.Center;
                // option.TextOptions.FallbackFonts.Add(new FontCollection().Install("font/NotoEmoji-Regular.ttf"));
                // option.TextOptions.FallbackFonts.Add(new FontCollection().Install("font/NotoColorEmoji.ttf"));
                // option.TextOptions.FallbackFonts.Add(new FontCollection().Install("font/NotoColorEmoji_WindowsCompatible.ttf"));
                // option.TextOptions.FallbackFonts.Add(new FontCollection().Install("font/seguiemj.ttf"));
                context.DrawText(
                    options: option,
                    text: text,
                    font: font.CreateFont(120, FontStyle.Bold),
                    color: Color.Black,
                    location: new PointF(image.Width/2, image.Height/3));
            });
            using var buffer = new MemoryStream();
            image.SaveAsPng(buffer);
            return buffer.ToArray();
        }
    
        public DisposableList<Table> GetTables()
        {
            var service = new SheetsService(new SheetsService.Initializer()
            {
                HttpClientInitializer = GoogleCredential.FromJson(googleServiceAccountJwt).CreateScoped(new[]{ SheetsService.Scope.Spreadsheets }),
            });
            var tables = service.Spreadsheets.Get(DB_SHEET_ID).Execute().Sheets.Select(s => new Table(service.Spreadsheets, DB_SHEET_ID, s)).ToArray();
            return new DisposableList<Table>(tables);
        }
    }
}
