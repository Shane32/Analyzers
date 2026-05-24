using System.Collections.Immutable;
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

        context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
            return;

        // Check if this is a call to Assembly.GetCallingAssembly()
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        if (methodSymbol.Name != "GetCallingAssembly" ||
            methodSymbol.ContainingType?.ToDisplayString() != "System.Reflection.Assembly" ||
            methodSymbol.Parameters.Length != 0)
            return;

        // Find the containing method or constructor
        var containingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        var containingConstructor = invocation.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();

        if (containingMethod != null) {
            if (!HasNoInliningAttribute(containingMethod.AttributeLists, context.SemanticModel)) {
                context.ReportDiagnostic(Diagnostic.Create(
                    GetCallingAssemblyMustBeInNoInliningMethod,
                    containingMethod.Identifier.GetLocation(),
                    containingMethod.Identifier.Text));
            }
        } else if (containingConstructor != null) {
            if (!HasNoInliningAttribute(containingConstructor.AttributeLists, context.SemanticModel)) {
                context.ReportDiagnostic(Diagnostic.Create(
                    GetCallingAssemblyMustBeInNoInliningMethod,
                    containingConstructor.Identifier.GetLocation(),
                    containingConstructor.Identifier.Text));
            }
        }
    }

    private static bool HasNoInliningAttribute(SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel)
    {
        foreach (var attributeList in attributeLists) {
            foreach (var attribute in attributeList.Attributes) {
                var attrSymbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                if (attrSymbol?.ContainingType?.ToDisplayString() == "System.Runtime.CompilerServices.MethodImplAttribute") {
                    // Check that MethodImplOptions.NoInlining is specified
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
                // MethodImplOptions.NoInlining == 8
                const int NoInlining = 8;
                if ((intValue & NoInlining) != 0)
                    return true;
            }
        }

        return false;
    }
}
