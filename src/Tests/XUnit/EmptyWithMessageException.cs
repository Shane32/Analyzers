using Xunit.Sdk;

namespace Tests;

public sealed class EmptyWithMessageException : XunitException
{
    private EmptyWithMessageException(string message) :
        base(message)
    { }

    public static EmptyWithMessageException ForNonEmptyCollection(string collection, string userMessage) =>
        new(userMessage + Environment.NewLine + EmptyException.ForNonEmptyCollection(collection).Message);
}
