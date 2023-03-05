using System.Collections.Immutable;
using Statiq.Common;
using IDocument = Statiq.Common.IDocument;

namespace Ociaw.StatiqMermaid;

/// <summary>
/// Renders mermaid definition files (<c>.mmd</c>) to SVG.
/// </summary>
public sealed class RenderMermaidSvg : RenderMermaidModule
{
    /// <summary>
    /// Creates a new <see cref="RenderMermaidSvg"/> module with the default settings.
    /// </summary>
    public RenderMermaidSvg()
    { }

    /// <summary>
    /// Creates a new <see cref="RenderMermaidSvg"/> module with the specified executable path.
    /// </summary>
    /// <param name="executable">The executable path.</param>
    public RenderMermaidSvg(Config<String> executable) : base(executable)
    { }

    /// <summary>
    /// Creates a new <see cref="RenderMermaidSvg"/> module with the specified settings.
    /// </summary>
    /// <param name="executable">The executable path.</param>
    /// <param name="mermaidTimeout">The length of time mermaid CLI can execute before being cancelled.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent mermaid executions allowed.</param>
    public RenderMermaidSvg(Config<String> executable, Config<TimeSpan> mermaidTimeout, Config<Int32> maxDegreeOfParallelism)
        : base(executable, mermaidTimeout, maxDegreeOfParallelism)
    { }
    
    
    /// <inheritdoc />
    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        String? diagram = await RenderSvgAsync(input, context, input.ContentProvider.GetStream());
        if (diagram is null)
            return Enumerable.Empty<IDocument>();

        var svgContentProvider = context.GetContentProvider(diagram, "image/svg+xml");
        return ImmutableArray.Create(context.CreateDocument(input.Source, input.Destination.ChangeExtension("svg"), svgContentProvider)) ;
    }
}
