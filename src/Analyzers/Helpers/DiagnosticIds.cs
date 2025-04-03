namespace Shane32.Analyzers.Helpers;

public static class DiagnosticIds
{
    public const string NO_IMPLICIT_IQUERYABLE_CASTS = "SHANE001";
    public const string NO_SYNCHRONOUS_IQUERYABLE_CALLS = "SHANE002";
    public const string ASYNC_METHODS_MUST_HAVE_CANCELLATION_TOKEN = "SHANE003";
}
