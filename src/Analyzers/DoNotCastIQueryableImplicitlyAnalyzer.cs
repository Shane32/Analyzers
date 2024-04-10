using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Shane32.Analyzers.Helpers;

namespace Shane32.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotCastIQueryableImplicitlyAnalyzer : DiagnosticAnalyzer //CodeAnalyzerBase
    {
        public static readonly DiagnosticDescriptor NoImplicitIQueryableCasts = new(
            id: DiagnosticIds.NO_IMPLICIT_IQUERYABLE_CASTS,
            title: "Don't implicitly cast IQueryable to IEnumerable",
            messageFormat: "Don't implicitly cast IQueryable to IEnumerable",
            category: DiagnosticCategories.USAGE,
            defaultSeverity: DiagnosticSeverity.Warning,
            //helpLinkUri: HelpLinks.DO_NOT_USE_OBSOLETE_ARGUMENT_METHOD,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            NoImplicitIQueryableCasts);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(AnalyzeOperationAction, OperationKind.Conversion);
        }

        private void AnalyzeOperationAction(OperationAnalysisContext context)
        {
            if (context.Operation is IConversionOperation conversionOperation &&
                conversionOperation.Operand?.Type?.Name == nameof(IQueryable<object>) &&
                conversionOperation.Type?.Name == nameof(IEnumerable<object>) &&
                conversionOperation.IsImplicit) {

                if (conversionOperation.Parent is IArgumentOperation argumentOperation &&
                    argumentOperation.Parent is IInvocationOperation invocationOperation &&
                    invocationOperation.TargetMethod.Name == nameof(Enumerable.AsEnumerable)) {
                    return;
                }

                var diagnostic = Diagnostic.Create(
                    NoImplicitIQueryableCasts,
                    conversionOperation.Syntax.GetLocation(),
                    conversionOperation.Operand.Syntax.ToString());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
