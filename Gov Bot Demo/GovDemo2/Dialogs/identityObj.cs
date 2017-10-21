using Microsoft.Bot.Builder.Dialogs;
using System;

public class identityObj
{
    public Value[] value { get; set; }
}

public class Value
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTime Timestamp { get; set; }
    public string FaceId { get; set; }
    public string NRIC { get; set; }
    public string Name { get; set; }
    public string PersonId { get; set; }
}
