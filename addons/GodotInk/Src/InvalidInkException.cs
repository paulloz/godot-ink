#nullable enable

namespace GodotInk;

public class InvalidInkException : System.Exception
{
    public InvalidInkException() : base()
    {
    }

    public InvalidInkException(string message) : base(message)
    {
    }
}
