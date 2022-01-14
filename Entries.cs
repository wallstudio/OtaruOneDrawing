using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MakiOneDrawingBot.Entries;


[TableName("schedule3")]
public class Schedule : EntryBase
{
    [Column] public TimeSpan? PreTime { get; set; }
    [Column] public TimeSpan? BeginTime { get; set; }
    [Column] public TimeSpan? EndTime { get; set; }
    [Column] public TimeSpan? AccTime { get; set; }
    [Column] public string Theme1 { get; set; }
    [Column] public string Theme2 { get; set; }
    [Column] public long? PreId { get; set; }
    [Column] public long? BeginId { get; set; }
    [Column] public long? EndId { get; set; }
    [Column] public long? AccId { get; set; }

    public override void Deserialize(Dictionary<string, string> columns)
    {
        base.Deserialize(columns);
        PreTime = TryParseTimeOffset(columns[nameof(PreTime)], out var preTime) ? preTime : null;
        BeginTime = TryParseTimeOffset(columns[nameof(BeginTime)], out var beginTime) ? beginTime : null;
        EndTime = TryParseTimeOffset(columns[nameof(EndTime)], out var endTime) ? endTime : null;
        AccTime = TryParseTimeOffset(columns[nameof(AccTime)], out var accTime) ? accTime : null;
        PreId = long.TryParse(columns[nameof(PreId)], out var preId) ? preId : null;
        BeginId = long.TryParse(columns[nameof(BeginId)], out var beginId) ? beginId : null;
        EndId = long.TryParse(columns[nameof(EndId)], out var endId) ? endId : null;
        AccId = long.TryParse(columns[nameof(AccId)], out var accId) ? accId : null;
    }

    static bool TryParseTimeOffset(string input, out TimeSpan offset)
    {
        if (Regex.Match(input, @"(?<sign>[+-]?)(?<hours>\d+)\:(?<minutes>\d+)") is not { Success: true, Groups: var groups })
        {
            offset = default;
            return false;
        }
        var sign = groups["sign"].Value == "-" ? -1 : 1;
        var hours = int.Parse(groups["hours"].Value);
        var minutes = int.Parse(groups["minutes"].Value);
        offset = sign * new TimeSpan(hours, minutes, 0);
        return true;
    }
}

[TableName("post2")]
public class Post : EntryBase
{
    [Column] public string ScheduleId { get; set; }
    [Column] public string UserName { get; set; }
}
