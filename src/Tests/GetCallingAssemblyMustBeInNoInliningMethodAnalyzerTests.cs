using Xunit;
using VerifyCS = Tests.CSharpAnalyzerVerifier<Shane32.Analyzers.GetCallingAssemblyMustBeInNoInliningMethodAnalyzer>;

namespace Tests;

public class GetCallingAssemblyMustBeInNoInliningMethodAnalyzerTests
{
    [Fact]
    public async Task GetCallingAssembly_WithoutNoInlining_Diagnostic()
    {
        const string source =
            """
            using System.Reflection;

            public class TestClass
            {
                public void GetCaller()
                {
                    var assembly = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 17, 5, 26).WithArguments("GetCaller"));
    }

    [Fact]
    public async Task GetCallingAssembly_WithNoInlining_NoDiagnostic()
    {
        const string source =
            """
            using System.Reflection;
            using System.Runtime.CompilerServices;

            public class TestClass
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public void GetCaller()
                {
                    var assembly = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task GetCallingAssembly_WithNoInliningAndOtherOptions_NoDiagnostic()
    {
        const string source =
            """
            using System.Reflection;
            using System.Runtime.CompilerServices;

            public class TestClass
            {
                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
                public void GetCaller()
                {
                    var assembly = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task GetCallingAssembly_WithOtherMethodImplOptions_Diagnostic()
    {
        const string source =
            """
            using System.Reflection;
            using System.Runtime.CompilerServices;

            public class TestClass
            {
                [MethodImpl(MethodImplOptions.NoOptimization)]
                public void GetCaller()
                {
                    var assembly = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(7, 17, 7, 26).WithArguments("GetCaller"));
    }

    [Fact]
    public async Task GetCallingAssembly_NotCalled_NoDiagnostic()
    {
        const string source =
            """
            using System.Reflection;

            public class TestClass
            {
                public void GetCaller()
                {
                    var assembly = Assembly.GetExecutingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task GetCallingAssembly_InConstructor_WithoutNoInlining_Diagnostic()
    {
        const string source =
            """
            using System.Reflection;

            public class TestClass
            {
                public TestClass()
                {
                    var assembly = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 12, 5, 21).WithArguments("TestClass"));
    }

    [Fact]
    public async Task GetCallingAssembly_InConstructor_WithNoInlining_NoDiagnostic()
    {
        const string source =
            """
            using System.Reflection;
            using System.Runtime.CompilerServices;

            public class TestClass
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public TestClass()
                {
                    var assembly = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task GetCallingAssembly_MultipleCallsInSameMethod_SingleDiagnostic()
    {
        const string source =
            """
            using System.Reflection;

            public class TestClass
            {
                public void GetCaller()
                {
                    var a1 = Assembly.GetCallingAssembly();
                    var a2 = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 17, 5, 26).WithArguments("GetCaller"),
            VerifyCS.Diagnostic().WithSpan(5, 17, 5, 26).WithArguments("GetCaller"));
    }

    [Fact]
    public async Task GetCallingAssembly_StaticMethod_WithoutNoInlining_Diagnostic()
    {
        const string source =
            """
            using System.Reflection;

            public class TestClass
            {
                public static void GetCaller()
                {
                    var assembly = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(5, 24, 5, 33).WithArguments("GetCaller"));
    }

    [Fact]
    public async Task GetCallingAssembly_StaticMethod_WithNoInlining_NoDiagnostic()
    {
        const string source =
            """
            using System.Reflection;
            using System.Runtime.CompilerServices;

            public class TestClass
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public static void GetCaller()
                {
                    var assembly = Assembly.GetCallingAssembly();
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
