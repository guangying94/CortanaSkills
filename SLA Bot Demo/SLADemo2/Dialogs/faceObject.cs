using Microsoft.Bot.Builder.Dialogs;

public class FaceRectangle
{
    public int top { get; set; }
    public int left { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}

public class faceObject
{
    public string faceId { get; set; }
    public FaceRectangle faceRectangle { get; set; }
}