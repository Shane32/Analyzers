using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Shane32.Analyzers.Helpers;

namespace Shane32.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GetCallingAssemblyMustBeInNoInliningMethodAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor GetCallingAssemblyMustBeInNoInliningMethod = new(
        id: DiagnosticIds.GET_CALLING_ASSEMBLY_MUST_BE_NO_INLINING,
        title: "Method calling Assembly.GetCallingAssembly() must have [MethodImpl(MethodImplOptions.NoInlining)]",
        messageFormat: "Method '{0}' calls Assembly.GetCallingAssembly() but is not marked with [MethodImpl(MethodImplOptions.NoInlining)]",
        category: DiagnosticCategories.USAGE,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        GetCallingAssemblyMustBeInNoInliningMethod);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
            return;

        if (!ContainsGetCallingAssemblyCall(methodDeclaration, context.SemanticModel))
            return;

        if (!HasNoInliningAttribute(methodDeclaration.AttributeLists, context.SemanticModel)) {
            context.ReportDiagnostic(Diagnostic.Create(
                GetCallingAssemblyMustBeInNoInliningMethod,
                methodDeclaration.Identifier.GetLocation(),
                methodDeclaration.Identifier.Text));
        }
    }

    private void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConstructorDeclarationSyntax constructorDeclaration)
            return;

        if (!ContainsGetCallingAssemblyCall(constructorDeclaration, context.SemanticModel))
            return;

        if (!HasNoInliningAttribute(constructorDeclaration.AttributeLists, context.SemanticModel)) {
            context.ReportDiagnostic(Diagnostic.Create(
                GetCallingAssemblyMustBeInNoInliningMethod,
                constructorDeclaration.Identifier.GetLocation(),
                constructorDeclaration.Identifier.Text));
        }
    }

    private static bool ContainsGetCallingAssemblyCall(SyntaxNode node, SemanticModel semanticModel)
    {
        return node.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation => IsGetCallingAssemblyCall(invocation, semanticModel));
    }

    private static bool IsGetCallingAssemblyCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return false;

        return methodSymbol.Name == "GetCallingAssembly" &&
               methodSymbol.ContainingType?.ToDisplayString() == "System.Reflection.Assembly" &&
               methodSymbol.Parameters.Length == 0;
    }

    private static bool HasNoInliningAttribute(SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel)
    {
        foreach (var attributeList in attributeLists) {
            foreach (var attribute in attributeList.Attributes) {
                var attrSymbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                if (attrSymbol?.ContainingType?.ToDisplayString() == "System.Runtime.CompilerServices.MethodImplAttribute") {
                    if (HasNoInliningOption(attribute, semanticModel))
                        return true;
                }
            }
        }

        return false;
    }

    private static bool HasNoInliningOption(AttributeSyntax attribute, SemanticModel semanticModel)
    {
        if (attribute.ArgumentList == null)
            return false;

        foreach (var argument in attribute.ArgumentList.Arguments) {
            var value = semanticModel.GetConstantValue(argument.Expression);
            if (value.HasValue && value.Value is int intValue) {
                if ((intValue & (int)System.Runtime.CompilerServices.MethodImplOptions.NoInlining) != 0)
                    return true;
            }
        }

        return false;
    }
}
