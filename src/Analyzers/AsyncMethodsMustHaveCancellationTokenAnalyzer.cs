using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Shane32.Analyzers.Helpers;

namespace Shane32.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncMethodsMustHaveCancellationTokenAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor AsyncMethodsMustHaveCancellationToken = new(
        id: DiagnosticIds.ASYNC_METHODS_MUST_HAVE_CANCELLATION_TOKEN,
        title: "Async methods should have a CancellationToken parameter",
        messageFormat: "Method '{0}' ends with 'Async' but does not have a CancellationToken parameter",
        category: DiagnosticCategories.USAGE,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        AsyncMethodsMustHaveCancellationToken);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
            return;

        // Check if method name ends with "Async"
        var methodName = methodDeclaration.Identifier.Text;
        if (!methodName.EndsWith("Async"))
            return;

        // Get the containing class
        var classDeclaration = methodDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDeclaration == null)
            return;

        // Check if class name ends with "Controller"
        var className = classDeclaration.Identifier.Text;
        if (className.EndsWith("Controller"))
            return;

        // Check if method has a CancellationToken parameter
        var hasCancellationToken = false;
        var semanticModel = context.SemanticModel;

        foreach (var parameter in methodDeclaration.ParameterList.Parameters) {
            if (parameter.Type == null)
                continue;
            var parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
            if (parameterType != null && parameterType.ToString() == "System.Threading.CancellationToken") {
                hasCancellationToken = true;
                break;
            }
        }

        if (!hasCancellationToken) {
            var diagnostic = Diagnostic.Create(
                AsyncMethodsMustHaveCancellationToken,
                methodDeclaration.Identifier.GetLocation(),
                methodName);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
