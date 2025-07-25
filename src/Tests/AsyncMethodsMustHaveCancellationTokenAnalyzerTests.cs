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

    [Fact]
    public async Task AsyncOverrideMethodWithoutCancellationToken_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public abstract class BaseService
            {
                public abstract Task ProcessAsync();
            }

            public class TestService : BaseService
            {
                public override async Task ProcessAsync()
                {
                    await Task.Delay(100);
                }
            }
            """
        ;

        // The abstract method should trigger a diagnostic, but the override should not
        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 26, 5, 38).WithArguments("ProcessAsync"));
    }

    [Fact]
    public async Task AsyncInterfaceImplementationWithoutCancellationToken_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public interface ITestService
            {
                Task GetDataAsync();
            }

            public class TestService : ITestService
            {
                public async Task GetDataAsync()
                {
                    await Task.Delay(100);
                }
            }
            """
        ;

        // The interface method should trigger a diagnostic, but the implementation should not
        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 10, 5, 22).WithArguments("GetDataAsync"));
    }

    [Fact]
    public async Task AsyncExplicitInterfaceImplementationWithoutCancellationToken_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public interface ITestService
            {
                Task ProcessDataAsync();
            }

            public class TestService : ITestService
            {
                async Task ITestService.ProcessDataAsync()
                {
                    await Task.Delay(100);
                }
            }
            """
        ;

        // The interface method should trigger a diagnostic, but the explicit implementation should not
        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 10, 5, 26).WithArguments("ProcessDataAsync"));
    }

    [Fact]
    public async Task AsyncMethodInDerivedClassNotOverridingWithoutCancellationToken_Diagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public abstract class BaseService
            {
                public abstract Task ProcessAsync();
            }

            public class TestService : BaseService
            {
                public override async Task ProcessAsync()
                {
                    await Task.Delay(100);
                }

                public async Task GetDataAsync()
                {
                    await Task.Delay(100);
                }
            }
            """
        ;

        // The abstract method and the non-override method should both trigger diagnostics
        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 26, 5, 38).WithArguments("ProcessAsync"),
            VerifyCS.Diagnostic().WithSpan(15, 23, 15, 35).WithArguments("GetDataAsync"));
    }

    [Fact]
    public async Task AsyncMethodImplementingInterfaceWithMultipleInterfaces_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public interface IFirstService
            {
                Task GetDataAsync();
            }

            public interface ISecondService
            {
                Task ProcessAsync();
            }

            public class TestService : IFirstService, ISecondService
            {
                public async Task GetDataAsync()
                {
                    await Task.Delay(100);
                }

                public async Task ProcessAsync()
                {
                    await Task.Delay(100);
                }
            }
            """
        ;

        // The interface methods should trigger diagnostics, but the implementations should not
        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 10, 5, 22).WithArguments("GetDataAsync"),
            VerifyCS.Diagnostic().WithSpan(10, 10, 10, 22).WithArguments("ProcessAsync"));
    }

    [Fact]
    public async Task AsyncMethodWithHttpContextParameterWithoutCancellationToken_NoDiagnostic()
    {
        const string source =
            """
            using System.Threading.Tasks;

            namespace Microsoft.AspNetCore.Http
            {
                public class HttpContext { }
            }

            public class TestService
            {
                public async Task ProcessRequestAsync(Microsoft.AspNetCore.Http.HttpContext context)
                {
                    await Task.Delay(100);
                }

                public async Task ProcessRequestWithDataAsync(Microsoft.AspNetCore.Http.HttpContext context, string data)
                {
                    await Task.Delay(100);
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
