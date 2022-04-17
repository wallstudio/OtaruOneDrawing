
using System.Linq;
using CoreTweet;
using System.Collections.Generic;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MakiOneDrawingBot
{
    
    public static class Views
    {
        public static readonly string HASH_TAG = "#トドいわんドロ";
        static string HELP_URL => $"https://wallstudio.github.io/OtaruOneDrawing?v={DateTime.Now.Ticks:x}";
        public static string HELP_URL_INDEX => $"index";
        public static string HELP_URL_RECENTRY => $"recentry";
        public static string HELP_URL_POST_RANK => $"post_rank";
        public static string HELP_URL_ENTRY_RANK => $"entry_rank";
        public static string HELP_URL_CONTINUE_RANK => $"continue_rank";

        public class Recentry
        {
            public Status Status { get; init; }
            public Entries.Post Post { get; init; }
        }
        
        public class Post
        {
            public User User { get; init; }
            public int Count { get; init; }
        }
        

        public static string PredictTweet(string theme1, string theme2)
        {
            var text = $@"
{HASH_TAG}
#小春六花 #夏色花梨 #花隈千冬
今夜のわんどろのテーマ発表！

今回のお題はこちらの二つ！
「{theme1}」
「{theme2}」

▼イベントルール詳細
{HELP_URL}
            ";
            return text.Trim();
        }

        public static string StartTweet(string theme1, string theme2)
        {
            var text = $@"
{HASH_TAG}
#小春六花 #夏色花梨 #花隈千冬
わんどろスタート！

今回のお題はこちらの二つ！
投稿時はタグを忘れないでくださいね！！
「{theme1}」
「{theme2}」

▼イベントルール詳細
{HELP_URL}
            ";
            return text.Trim();
        }

        public static string FinishTweet(DateOnly? nextDate)
        {
            var text = $@"
{HASH_TAG}
#小春六花 #夏色花梨 #花隈千冬
わんどろ終了ーー！！

投稿いただいたイラストは後日RTします！！
次回は {nextDate?.ToString(@"MM/dd\(ddd\)") ?? "未定"} の予定です、お楽しみに！！

▼イベントルール詳細
{HELP_URL}
            ";
            return text.Trim();
        }

        public static string ResultTweet(Entries.Post[] tweets, DateOnly? nextDate)
        {
            string text;
            if(tweets.Length > 0)
            {
                text = $@"
{HASH_TAG}
#小春六花 #夏色花梨 #花隈千冬
先日のわんどろの投稿イラストをRTします！！！
{tweets.Length}作品の投稿をいただきました！

次回は {nextDate?.ToString(@"MM/dd\(ddd\)") ?? "未定"} の予定です、お楽しみに！！

▼イベントルール詳細
{HELP_URL}
                ";
            }
            else
            {
                text = $@"
{HASH_TAG}
昨日のわんどろの投稿イラストをRT……
って、誰も投稿してくれなかったみたい…(´；ω；｀)

次回は {nextDate?.ToString(@"MM/dd\(ddd\)") ?? "未定"} の予定です、よろしくおねがいします。

▼イベントルール詳細
{HELP_URL}
                ";
            }
            return text.Trim();
        }

        public static byte[] GenerateTextImage(string text)
        {
            using var image = Image.Load("docs/img/theme_template.png");
            image.Mutate(context =>
            {
                var font = new FontCollection().Install("font/Corporate-Logo-Rounded.ttf");
                var option = new DrawingOptions();
                option.TextOptions.LineSpacing = 1.0f;
                option.TextOptions.VerticalAlignment = VerticalAlignment.Center;
                option.TextOptions.HorizontalAlignment = HorizontalAlignment.Left;
                option.TextOptions.FallbackFonts.Add(new FontCollection().Install("font/TwemojiMozilla.ttf")); // 幅計算がうまく行ってないっぽい
                try
                {
                    context.DrawText(
                        options: option,
                        text: Regex.Replace(text, "(\uFE00|\uFE01|\uFE02|\uFE03|\uFE04|\uFE05|\uFE06|\uFE07|\uFE08|\uFE09|\uFE0A|\uFE0B|\uFE0C|\uFE0D|\uFE0E|\uFE0F)", ""),
                        font: font.CreateFont(160, FontStyle.Bold),
                        color: Color.Black,
                        location: new PointF(image.Width * 0.375f, image.Height * 0.45f));
                }
                catch(Exception)
                {
                    Console.Error.WriteLine($"fail DrawText \"${text}\"");
                    throw;
                }
            });
            using var buffer = new MemoryStream();
            image.SaveAsPng(buffer);
            return buffer.ToArray();
        }

        public static string Dashboard(Recentry[] recently, Post[] postRanking, Post[] entryRanking, Post[] continueRanking)
        {
            var text = @$"
[📝基本ルール](#基本ルール)

## ランキング

### 最近の作品

| 1️⃣ | 2️⃣ | 3️⃣ | 4️⃣ | 5️⃣ | 6️⃣ | 7️⃣ | 8️⃣ | 9️⃣ | 🔟 |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| {string.Join(" | ", recently.Take(10).Select(post => LinkedMedia(post?.Status)))} |
| {string.Join(" | ", recently.Take(10).Select(post => LinkedName(post?.Status?.User)))} |


[全てみる]({HELP_URL_RECENTRY})


### 🏆Best 作品数🏆

沢山の小樽っ娘イラスト作品を描き上げた方々です！

| 🥇 | 🥈 | 🥉 |
| :---: | :---: | :---: |
| {string.Join(" | ", postRanking.Take(3).Select(post => LinkedImage(post?.User)))} |
| {string.Join(" | ", postRanking.Take(3).Select(post => LinkedName(post?.User)))} |
| {string.Join(" | ", postRanking.Take(3).Select(post => $"{post?.Count} 作品"))} |

[全てみる]({HELP_URL_POST_RANK})

### 🏆Best 参加回数🏆

イベントに沢山参加してくださった方々です！

| 🥇 | 🥈 | 🥉 |
| :---: | :---: | :---: |
| {string.Join(" | ", entryRanking.Take(3).Select(post => LinkedImage(post?.User)))} |
| {string.Join(" | ", entryRanking.Take(3).Select(post => LinkedName(post?.User)))} |
| {string.Join(" | ", entryRanking.Take(3).Select(post => $"{post?.Count} 回"))} |

[全てみる]({HELP_URL_ENTRY_RANK})

### 🏆Best 継続数🏆

継続的に参加してくださっている方々です！

| 🥇 | 🥈 | 🥉 |
| :---: | :---: | :---: |
| {string.Join(" | ", continueRanking.Take(3).Select(post => LinkedImage(post?.User)))} |
| {string.Join(" | ", continueRanking.Take(3).Select(post => LinkedName(post?.User)))} |
| {string.Join(" | ", continueRanking.Take(3).Select(post => $"{post?.Count} 回連続"))} |

[全てみる]({HELP_URL_CONTINUE_RANK})

{File.ReadAllText("README.md")}
            ";
            return text.Trim();
        }

        public static string RecentryPage(Recentry[] recently)
        {
            var text = @$"
[戻る]({HELP_URL_INDEX})

[📝基本ルール](#基本ルール)

## 全ての作品

| サムネイル | イベント日 | アイコン | ユーザー名 |
| :--: | :--: | :--: | :--: |
{string.Join("\n", recently.Select((post, i) =>
{
    return $"| {LinkedMedia(post.Status)} | {post?.Post?.ScheduleId} | {LinkedImage(post?.Status?.User)} | {LinkedName(post?.Status?.User)} |";
}))}

{File.ReadAllText("README.md")}
            ";
            return text.Trim();
        }

        public static string PostRankingPage(Post[] postRanking)
        {
            var text = @$"
[戻る]({HELP_URL_INDEX})

[📝基本ルール](#基本ルール)

## 🏆Best 作品数🏆 （全て）

| No | アイコン | ユーザー名 | スコア |
| :--: | :--: | :--: | :--: |
{string.Join("\n", postRanking.Select((post, i) =>
{
    return $"| {i + 1} | {LinkedImage(post?.User)} | {LinkedName(post?.User)} | {post?.Count} 作品 |";
}))}

{File.ReadAllText("README.md")}
            ";
            return text.Trim();
        }

        public static string EntryRankingPage(Post[] entryRanking)
        {
            var text = @$"
[戻る]({HELP_URL_INDEX})

[📝基本ルール](#基本ルール)

## 🏆Best 参加回数🏆 （全て）

| No | アイコン | ユーザー名 | スコア |
| :--: | :--: | :--: | :--: |
{string.Join("\n", entryRanking.Select((post, i) =>
{
    return $"| {i + 1} | {LinkedImage(post?.User)} | {LinkedName(post?.User)} | {post?.Count} 回 |";
}))}

{File.ReadAllText("README.md")}
            ";
            return text.Trim();
        }
    
        public static string ContinueRankingPage(Post[] continueRanking)
        {
            var text = @$"
[戻る]({HELP_URL_INDEX})

[📝基本ルール](#基本ルール)

## 🏆Best 継続数🏆 （全て）

| No | アイコン | ユーザー名 | スコア |
| :--: | :--: | :--: | :--: |
{string.Join("\n", continueRanking.Select((post, i) =>
{
    return $"| {i + 1} | {LinkedImage(post?.User)} | {LinkedName(post?.User)} | {post?.Count} 回連続 |";
}))}

{File.ReadAllText("README.md")}
            ";
            return text.Trim();
        }

        static string LinkedMedia(Status status)
        {
            var screenName = status.User.ScreenName;
            var statusId = status.Id.ToString();
            var mediaUrl = status.Entities.Media.Concat(status.ExtendedEntities.Media).FirstOrDefault()?.MediaUrlHttps;
            return $"[![]({mediaUrl}:thumb)](https://twitter.com/{screenName}/status/{statusId})";
        }

        static string LinkedName(User user) => $"[@{user?.ScreenName}](https://twitter.com/{user?.ScreenName})";
        static string LinkedImage(User user) => $"[![@{user?.ScreenName}]({user?.ProfileImageUrlHttps.Replace("_normal.", "_bigger.")})](https://twitter.com/{user?.ScreenName})";

    }

}
