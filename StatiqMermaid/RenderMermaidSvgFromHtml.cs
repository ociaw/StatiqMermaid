using System.Text;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using IDocument = Statiq.Common.IDocument;

namespace Ociaw.StatiqMermaid;

/// <summary>
/// Builds SVGs from mermaid diagrams found in HTML tags and embedding the SVG into the document. 
/// </summary>
/// <remarks>
/// <para>
/// This module finds all <c>.mermaid</c> blocks and renders them to SVG via mermaid.
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
public sealed class RenderMermaidSvgFromHtml : ParallelModule
{
    private readonly MermaidCliExecutor _executor = new("mmdc");
    private String _codeQuerySelector = ".mermaid";

    /// <summary>
    /// Sets the query selector to use to find mermaid blocks.
    /// </summary>
    /// <param name="querySelector">
    /// The query selector to use to select mermaid blocks. The default value is <c>.mermaid</c>.
    /// </param>
    /// <returns>The current instance.</returns>
    public RenderMermaidSvgFromHtml WithCodeQuerySelector(String querySelector)
    {
        _codeQuerySelector = querySelector;
        return this;
    }

    /// <summary>
    /// Sets the file path to the mermaid CLI executable. If not set <c>mmdc</c> is used.
    /// </summary>
    /// <param name="executable">The executable path.</param>
    /// <returns>The current instance.</returns>
    public RenderMermaidSvgFromHtml WithMermaidExecutable(String executable)
    {
        _executor.Executable = executable;
        return this;
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        var htmlDocument = await input.ParseHtmlAsync();
        Boolean dirty = false;
        foreach (var element in htmlDocument.QuerySelectorAll(_codeQuerySelector))
        {
            // Don't mermaid anything that potentially has already been turned into an svg
            if (element.ClassList.Contains("svg"))
                continue;

            String diagram;
            try
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(element.TextContent));
                diagram = await _executor.RenderSvgAsync(stream, context.CancellationToken);
            }
            catch (Exception innerEx)
            {
                context.LogInformation("Exception while generating SVG from mermaid definition: {message}", innerEx.Message);
                continue;
            }

            dirty = true;
            element.InnerHtml = diagram;
            element.ClassList.Add("svg");
            // xmlns:xlink is XHTML, not HTML and will cause MinifyHtml to throw a fit
            element.FirstElementChild!.RemoveAttribute("xmlns:xlink");
        }

        if (!dirty)
            return new[] { input };

        return new[] { input.Clone(context.GetContentProvider(htmlDocument)) };
    }
}
