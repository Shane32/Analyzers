using System.Collections.Immutable;
using System.Linq;
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

        // Check if method is in a class or interface
        var classDeclaration = methodDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        var interfaceDeclaration = methodDeclaration.FirstAncestorOrSelf<InterfaceDeclarationSyntax>();

        // Skip if not in a class or interface
        if (classDeclaration == null && interfaceDeclaration == null)
            return;

        // Skip if in a Controller class
        if (classDeclaration != null) {
            var className = classDeclaration.Identifier.Text;
            if (className.EndsWith("Controller"))
                return;
        }

        var semanticModel = context.SemanticModel;

        // Skip if method overrides a base member or implements an interface member
        if (IsOverrideOrInterfaceImplementation(methodDeclaration, semanticModel))
            return;

        // Check if method has a CancellationToken parameter or HttpContext parameter
        var hasCancellationToken = false;
        var hasHttpContext = false;

        foreach (var parameter in methodDeclaration.ParameterList.Parameters) {
            if (parameter.Type == null)
                continue;
            var parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
            if (parameterType != null) {
                var typeString = parameterType.ToString();
                if (typeString == "System.Threading.CancellationToken") {
                    hasCancellationToken = true;
                }
                else if (typeString == "Microsoft.AspNetCore.Http.HttpContext") {
                    hasHttpContext = true;
                }
            }
        }

        // Skip if method has HttpContext parameter (common in ASP.NET Core minimal APIs)
        if (hasHttpContext)
            return;

        if (!hasCancellationToken) {
            var diagnostic = Diagnostic.Create(
                AsyncMethodsMustHaveCancellationToken,
                methodDeclaration.Identifier.GetLocation(),
                methodName);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsOverrideOrInterfaceImplementation(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
    {
        // Check if method has override modifier
        if (methodDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword))
            return true;

        // Get the method symbol to check if it implements an interface
        var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
        if (methodSymbol == null)
            return false;

        // Check if the method implements an interface member
        var containingType = methodSymbol.ContainingType;
        if (containingType == null)
            return false;

        // Check all interfaces implemented by the containing type
        foreach (var interfaceType in containingType.AllInterfaces)
        {
            foreach (var interfaceMember in interfaceType.GetMembers().OfType<IMethodSymbol>())
            {
                // Check if this method implements the interface method
                var implementation = containingType.FindImplementationForInterfaceMember(interfaceMember);
                if (SymbolEqualityComparer.Default.Equals(implementation, methodSymbol))
                    return true;
            }
        }

        return false;
    }
}
