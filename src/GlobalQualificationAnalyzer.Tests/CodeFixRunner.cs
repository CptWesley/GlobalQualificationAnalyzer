using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GlobalQualificationAnalyzer.Tests;

internal static class CodeFixRunner
{
    private static readonly GlobalQualificationCodeFixProvider Provider = new();

    /// <summary>Applies the code fix to the first diagnostic in <paramref name="source"/>.</summary>
    public static async Task<string> Fix(DiagnosticAnalyzer analyzer, string source)
    {
        using var workspace = new AdhocWorkspace();
        var project = CreateProject(workspace, [source]);
        var diagnostic = First(await GetDiagnostics(project, analyzer));

        var actions = new List<CodeAction>();
        var context = new CodeFixContext(Document(project, diagnostic), diagnostic, (action, _) => actions.Add(action), CancellationToken.None);
        await Provider.RegisterCodeFixesAsync(context);

        return (await Apply(actions.Single(), project)).Single();
    }

    /// <summary>
    /// Applies fix-all in <paramref name="scope"/>, triggered from the first diagnostic in the first source,
    /// and returns every source afterwards.
    /// </summary>
    public static async Task<string[]> FixAll(DiagnosticAnalyzer analyzer, FixAllScope scope, params string[] sources)
    {
        using var workspace = new AdhocWorkspace();
        var project = CreateProject(workspace, sources);
        var diagnostics = await GetDiagnostics(project, analyzer);
        var diagnostic = First(diagnostics);

        var context = new FixAllContext(
            Document(project, diagnostic),
            Provider,
            scope,
            diagnostic.Id,
            [diagnostic.Id],
            new DiagnosticProvider(diagnostics),
            CancellationToken.None);

        var action = await Provider.GetFixAllProvider().GetFixAsync(context);
        return await Apply(action!, project);
    }

    private static Project CreateProject(AdhocWorkspace workspace, string[] sources)
    {
        var project = workspace.AddProject("Test", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true))
            .WithParseOptions(CSharpParseOptions.Default)
            .WithMetadataReferences(AnalyzerRunner.References);

        for (var i = 0; i < sources.Length; i++)
        {
            project = project.AddDocument($"Test{i}.cs", sources[i], filePath: $"Test{i}.cs").Project;
        }

        return project;
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnostics(Project project, DiagnosticAnalyzer analyzer)
    {
        var compilation = await project.GetCompilationAsync();
        return await compilation!.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync();
    }

    private static Diagnostic First(ImmutableArray<Diagnostic> diagnostics)
        => diagnostics
            .OrderBy(d => d.Location.SourceTree!.FilePath, StringComparer.Ordinal)
            .ThenBy(d => d.Location.SourceSpan.Start)
            .First();

    private static Document Document(Project project, Diagnostic diagnostic)
        => project.GetDocument(diagnostic.Location.SourceTree)!;

    private static async Task<string[]> Apply(CodeAction action, Project project)
    {
        var operations = await action.GetOperationsAsync(CancellationToken.None);
        var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        var documents = solution.GetProject(project.Id)!.Documents.OrderBy(d => d.FilePath, StringComparer.Ordinal);

        var texts = new List<string>();
        foreach (var document in documents)
        {
            texts.Add((await document.GetTextAsync()).ToString());
        }

        return [.. texts];
    }

    private sealed class DiagnosticProvider(ImmutableArray<Diagnostic> diagnostics) : FixAllContext.DiagnosticProvider
    {
        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);

        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
            => Task.FromResult(diagnostics.Where(d => d.Location.SourceTree!.FilePath == document.FilePath));

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
            => Task.FromResult(Enumerable.Empty<Diagnostic>());
    }
}
