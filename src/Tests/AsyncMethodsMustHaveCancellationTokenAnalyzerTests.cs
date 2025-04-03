using Xunit;
using VerifyCS = Tests.CSharpAnalyzerVerifier<Shane32.Analyzers.AsyncMethodsMustHaveCancellationTokenAnalyzer>;

namespace Tests;

public class AsyncMethodsMustHaveCancellationTokenAnalyzerTests
{
    [Fact]
    public async Task AsyncMethodWithoutCancellationToken_Diagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public class TestService
            {
                public async Task GetDataAsync()
                {
                    await Task.Delay(100);
                    return;
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 23, 5, 35).WithArguments("GetDataAsync"));
    }

    [Fact]
    public async Task AsyncMethodWithCancellationToken_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading;
            using System.Threading.Tasks;

            public class TestService
            {
                public async Task GetDataAsync(CancellationToken cancellationToken)
                {
                    await Task.Delay(100, cancellationToken);
                    return;
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AsyncMethodInControllerWithoutCancellationToken_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public class TestController
            {
                public async Task GetDataAsync()
                {
                    await Task.Delay(100);
                    return;
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NonAsyncMethodWithoutCancellationToken_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public class TestService
            {
                public Task GetData()
                {
                    return Task.Delay(100);
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AsyncMethodWithCancellationTokenAsLastParameter_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading;
            using System.Threading.Tasks;

            public class TestService
            {
                public async Task GetDataAsync(string id, int count, CancellationToken cancellationToken)
                {
                    await Task.Delay(100, cancellationToken);
                    return;
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AsyncMethodWithOptionalCancellationToken_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading;
            using System.Threading.Tasks;

            public class TestService
            {
                public async Task GetDataAsync(CancellationToken cancellationToken = default)
                {
                    await Task.Delay(100, cancellationToken);
                    return;
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AsyncMethodInInterfaceWithoutCancellationToken_ShouldReportDiagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public interface ITestService
            {
                Task GetDataAsync();
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 10, 5, 22).WithArguments("GetDataAsync"));
    }
}
