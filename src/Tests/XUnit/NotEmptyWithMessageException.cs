using Xunit.Sdk;

namespace Tests;

public sealed class NotEmptyWithMessageException : XunitException
{
    private NotEmptyWithMessageException(string message) :
        base(message)
    { }

    public static NotEmptyWithMessageException ForNonEmptyCollection(string userMessage) =>
        new(userMessage + Environment.NewLine + NotEmptyException.ForNonEmptyCollection().Message);
}
