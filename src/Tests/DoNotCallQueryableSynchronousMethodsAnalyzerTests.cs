using Xunit;
using VerifyCS = Tests.CSharpAnalyzerVerifier<Shane32.Analyzers.DoNotCallQueryableSynchronousMethodsAnalyzer>;

namespace Tests;

public class DoNotCallQueryableSynchronousMethodsAnalyzerTests
{
    [Fact]
    public async Task UseOfIQueryable_NoDiagnostic()
    {
        const string source =
            """
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.Where(x => x > 5);
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task UseOfIQueryable_OrderBy_NoDiagnostic()
    {
        const string source =
            """
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.OrderBy(x => x);
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IQueryable_Single()
    {
        const string source =
            """
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.Where(x => x > 5);
                    var value = query2.Single();
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(9, 21, 9, 36).WithArguments("Single"));
    }

    [Fact]
    public async Task IQueryable_SingleOrDefault()
    {
        const string source =
            """
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.Select(x => new { Value = x });
                    var value = query2.SingleOrDefault(x => x.Value == 5);
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(9, 21, 9, 62).WithArguments("SingleOrDefault"));
    }

    [Fact]
    public async Task IQueryable_AsEnumerable_Single()
    {
        const string source =
            """
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.Where(x => x > 5);
                    var value = query2.AsEnumerable().Single();
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IQueryable_WithinQuery()
    {
        const string source =
            """
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.Where(x => query.Single() > 5);
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
