using Xunit.Sdk;

namespace Tests;

public sealed class EqualWithMessageException : XunitException
{
    private EqualWithMessageException(string message) :
        base(message)
    { }

    public static EqualWithMessageException ForMismatchedValues(
        object? expected,
        object? actual,
        string userMessage) =>
        new(userMessage + Environment.NewLine + EqualException.ForMismatchedValues(expected, actual).Message);
}
