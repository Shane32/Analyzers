using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Shane32.Analyzers.Helpers;

namespace Shane32.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotCallQueryableSynchronousMethodsAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor NoSynchronousCalls = new(
        id: DiagnosticIds.NO_SYNCHRONOUS_IQUERYABLE_CALLS,
        title: "Don't call synchronous methods on IQueryable",
        messageFormat: "Don't call Queryable.{0}; instead use an asynchronous equivalent such as {0}Async",
        category: DiagnosticCategories.USAGE,
        defaultSeverity: DiagnosticSeverity.Warning,
        //helpLinkUri: HelpLinks.DO_NOT_USE_OBSOLETE_ARGUMENT_METHOD,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        NoSynchronousCalls);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeOperationAction, OperationKind.Invocation);
    }

    private void AnalyzeOperationAction(OperationAnalysisContext context)
    {
        // look for static methods on Queryable that do not return an IQueryable
        if (context.Operation is IInvocationOperation invocationOperation &&
            invocationOperation.TargetMethod.IsStatic &&
            invocationOperation.TargetMethod.ContainingType.ToString() == "System.Linq.Queryable" &&
            invocationOperation.Type != null &&
            invocationOperation.Type.Name != nameof(IQueryable)) {

            // check if the return type implements IQueryable
            var returnType = invocationOperation.Type!;
            foreach (var i in returnType.Interfaces) {
                if (i.Name == nameof(IQueryable)) {
                    return;
                }
            }

            // ignore calls within Expressions
            var parent = invocationOperation.Parent;
            while (parent != null) {
                if (parent.Type?.Name == nameof(System.Linq.Expressions.Expression)) {
                    return;
                }
                parent = parent.Parent;
            }

            var diagnostic = Diagnostic.Create(
                NoSynchronousCalls,
                invocationOperation.Syntax.GetLocation(),
                invocationOperation.TargetMethod.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
