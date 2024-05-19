#nullable enable

namespace GodotInk;

public class InvalidInkException : System.Exception
{
    public InvalidInkException()
    {
    }

    public InvalidInkException(string message) : base(message)
    {
    }
}
