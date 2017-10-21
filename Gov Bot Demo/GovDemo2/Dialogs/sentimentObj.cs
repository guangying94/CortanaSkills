using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

public class Document
{
    public double score { get; set; }
    public string id { get; set; }
}

public class sentimentObj
{
    public IList<Document> documents { get; set; }
    public IList<object> errors { get; set; }
}