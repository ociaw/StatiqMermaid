using System.Collections.Immutable;
using System.Text;
using Statiq.Common;
using IDocument = Statiq.Common.IDocument;

namespace Ociaw.StatiqMermaid;

/// <summary>
/// Builds SVGs from mermaid diagrams found in HTML tags and embeds the resulting SVG into the document. 
/// </summary>
/// <remarks>
/// <para>
/// This module finds all <c>.mermaid</c> blocks in a document and renders them to SVG via mermaid.
/// </para>
/// <para>
/// Note that because this module parses the document
/// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
/// only place this module after all other template processing has been performed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Pipelines.Add("Mermaid",
///     ReadFiles("*.html"),
///     Mermaid(),
///     WriteFiles(".html")
/// );
/// </code>
/// </example>
/// <category name="Content" />
public sealed class RenderMermaidSvgInsideHtml : RenderMermaidModule
{
    /// <summary>
    /// Creates a new <see cref="RenderMermaidSvgInsideHtml"/> module with the default settings.
    /// </summary>
    public RenderMermaidSvgInsideHtml()
    { }

    /// <summary>
    /// Creates a new <see cref="RenderMermaidSvgInsideHtml"/> module with the specified executable path.
    /// </summary>
    /// <param name="executable">The executable path.</param>
    public RenderMermaidSvgInsideHtml(Config<String> executable) : base(executable)
    { }

    /// <summary>
    /// Creates a new <see cref="RenderMermaidSvgInsideHtml"/> module with the specified settings.
    /// </summary>
    /// <param name="executable">The executable path.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent mermaid executions allowed.</param>
    /// <param name="mermaidTimeout">The length of time mermaid CLI can execute before being cancelled.</param>
    public RenderMermaidSvgInsideHtml(Config<String> executable, Config<TimeSpan> mermaidTimeout, Config<Int32> maxDegreeOfParallelism)
    {
        Executable = executable;
        Timeout = mermaidTimeout;
        MaxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    /// <summary>
    /// The CSS query selector to use to select mermaid blocks within the HTML page.
    /// </summary>
    /// <remarks>Defaults to <c>.mermaid</c></remarks>
    public Config<String> CodeQuerySelector { get; init; } = ".mermaid";

    /// <inheritdoc />
    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        var selector = await CodeQuerySelector.GetValueAsync(input, context);
        var htmlDocument = await input.ParseHtmlAsync();
        Boolean dirty = false;
        foreach (var element in htmlDocument.QuerySelectorAll(selector))
        {
            // Don't mermaid anything that potentially has already been turned into an svg
            if (element.ClassList.Contains("svg"))
                continue;

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(element.TextContent));
            String? diagram = await RenderSvgAsync(input, context, stream);
            if (diagram is null)
                continue;

            dirty = true;
            element.InnerHtml = diagram;
            element.ClassList.Add("svg");
            // xmlns:xlink is XHTML, not HTML and will cause MinifyHtml to throw a fit
            element.FirstElementChild!.RemoveAttribute("xmlns:xlink");
        }

        if (!dirty)
            return ImmutableArray.Create(input);

        return ImmutableArray.Create(input.Clone(context.GetContentProvider(htmlDocument)));
    }
}
