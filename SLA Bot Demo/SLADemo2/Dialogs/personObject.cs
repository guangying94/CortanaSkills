using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

public class PersonObject
{
    public string personId { get; set; }
    public List<string> persistedFaceIds { get; set; }
    public string name { get; set; }
    public string userData { get; set; }
}