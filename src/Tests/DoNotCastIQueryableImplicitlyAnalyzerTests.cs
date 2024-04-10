using Xunit;
using VerifyCS = Tests.CSharpAnalyzerVerifier<Shane32.Analyzers.DoNotCastIQueryableImplicitlyAnalyzer>;

namespace Tests;

public class DoNotCastIQueryableImplicitlyAnalyzerTests
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
    public async Task IQueryable_ImplicitCast()
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
                    var list = query2.ToList();
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(9, 20, 9, 26).WithArguments("query2"));
    }

    [Fact]
    public async Task IQueryable_ExplicitCast()
    {
        const string source =
            """
            using System.Collections.Generic;
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.Where(x => x > 5);
                    var list = ((IEnumerable<int>)query2).ToList();
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IQueryable_ExplicitCast2()
    {
        const string source =
            """
            using System.Collections.Generic;
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.Where(x => x > 5);
                    var list = (query2 as IEnumerable<int>).ToList();
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IQueryable_ExplicitAssignment()
    {
        const string source =
            """
            using System.Collections.Generic;
            using System.Linq;

            public class TestClass
            {
                public TestClass()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    IEnumerable<int> query2 = query.Where(x => x > 5);
                    var list = query2.ToList();
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(9, 35, 9, 58).WithArguments("query.Where(x => x > 5)"));
    }

    [Fact]
    public async Task IQueryable_ImplicitCastReturn()
    {
        const string source =
            """
            using System.Collections.Generic;
            using System.Linq;

            public class TestClass
            {
                public IEnumerable<int> Test()
                {
                    var query = Enumerable.Empty<int>().AsQueryable();
                    var query2 = query.Where(x => x > 5);
                    return query2;
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic().WithSpan(10, 16, 10, 22).WithArguments("query2"));
    }

    [Fact]
    public async Task IQueryable_AsEnumerable()
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
                    var list = query2.AsEnumerable().ToList();
                }
            }
            """
        ;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
