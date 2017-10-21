using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;


public class qnaObj
{
    public Answer[] answers { get; set; }
}

public class Answer
{
    public string answer { get; set; }
    public string[] questions { get; set; }
    public float score { get; set; }
}
