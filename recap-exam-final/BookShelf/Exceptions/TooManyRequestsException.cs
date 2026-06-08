namespace BookShelf.Exceptions;

public class TooManyRequestsException : SystemException
{
    public TooManyRequestsException(string message = "Too many requests") : base(message) { }
}
