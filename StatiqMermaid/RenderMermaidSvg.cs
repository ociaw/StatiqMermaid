using Microsoft.Extensions.Logging;
using Statiq.Common;
using IDocument = Statiq.Common.IDocument;

namespace Ociaw.StatiqMermaid;

/// <summary>
/// Renders mermaid definition files (<c>.mmd</c>) to SVG.
/// </summary>
public sealed class RenderMermaidSvg : ParallelModule, IDisposable
{
    private readonly MermaidCliExecutor _executor;

    /// <summary>
    /// Creates a new <see cref="RenderMermaidSvg"/> module with the specified mermaid CLI executable.
    /// </summary>
    /// <param name="executable">The executable path.</param>
    public RenderMermaidSvg(String executable) => _executor = new MermaidCliExecutor(executable);

    /// <inheritdoc />
    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        String diagram;
        try
        {
            diagram = await _executor.RenderSvgAsync(input.ContentProvider.GetStream(), context.CancellationToken);
        }
        catch (Exception innerEx)
        {
            context.LogInformation("Exception while generating SVG from mermaid definition: {message}", innerEx.Message);
            return Enumerable.Empty<IDocument>();
        }

        var svgContentProvider = context.GetContentProvider(diagram, "image/svg+xml");
        return new[] { context.CreateDocument(input.Source, input.Destination.ChangeExtension("svg"), svgContentProvider) };
    }

    /// <inheritdoc />
    public void Dispose() => _executor.Dispose();
}
