using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

public class Candidate
{
    public string personId { get; set; }
    public double confidence { get; set; }
}

public class identifyFaceObject
{
    public string faceId { get; set; }
    public List<Candidate> candidates { get; set; }
}