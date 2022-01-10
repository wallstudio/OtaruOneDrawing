namespace MakiOneDrawingBot.Entries;


[TableName("schedule2")]
public class Schedule : EntryBase
{
    [Column] public string Theme1 { get; set; }
    [Column] public string Theme2 { get; set; }
    [Column] public string PreId { get; set; }
    [Column] public string BeginId { get; set; }
    [Column] public string EndId { get; set; }
    [Column] public string AccId { get; set; }
}

[TableName("post2")]
public class Post : EntryBase
{
    [Column] public string ScheduleId { get; set; }
    [Column] public string UserName { get; set; }
}
