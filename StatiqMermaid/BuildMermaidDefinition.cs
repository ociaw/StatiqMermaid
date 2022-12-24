using System.Net;
using Statiq.CodeAnalysis;
using Statiq.Common;

namespace Ociaw.StatiqMermaid;

/// <summary>
/// Builds mermaid definitions from analyzed types.
/// </summary>
public sealed class BuildMermaidDefinition : ParallelSyncModule
{
	/// <inheritdoc />
	protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
	{
		IReadOnlyList<IDocument> allInterfaces = input.GetDocumentList(CodeAnalysisKeys.AllInterfaces);
		IReadOnlyList<IDocument> baseTypes = input.GetDocumentList(CodeAnalysisKeys.BaseTypes);
		IReadOnlyList<IDocument> derivedTypes = input.GetDocumentList(CodeAnalysisKeys.DerivedTypes);
		IReadOnlyList<IDocument> implementingTypes = input.GetDocumentList(CodeAnalysisKeys.ImplementingTypes);

		var hierarchy = new System.Text.StringBuilder();
		hierarchy.AppendLine("graph BT");
		if (baseTypes is {Count: > 0})
		{
			for(Int32 b = 0 ; b < baseTypes.Count ; b++)
			{
				hierarchy.AppendLine($"\t{(b == 0 ? "Type" : "Base" + (b - 1))}-->Base{b}[\"{WebUtility.HtmlEncode(baseTypes[b].GetString(CodeAnalysisKeys.DisplayName))}\"]");
				if (!baseTypes[b].Destination.IsNullOrEmpty)
					hierarchy.AppendLine($"\tclick Base{b} \"{context.GetLink(baseTypes[b].Destination.FullPath)}\"");
			}
		}

		if (allInterfaces is {Count: > 0})
		{
			for (Int32 c = 0 ; c < allInterfaces.Count ; c++)
			{
				hierarchy.AppendLine($"\tType-.->Interface{c}[\"{WebUtility.HtmlEncode(allInterfaces[c].GetString(CodeAnalysisKeys.DisplayName))}\"]");
				if (!allInterfaces[c].Destination.IsNullOrEmpty)
					hierarchy.AppendLine($"\tclick Interface{c} \"{context.GetLink(allInterfaces[c].Destination.FullPath)}\"");
			}
		}

		hierarchy.AppendLine($"\tType[\"{WebUtility.HtmlEncode(input.GetString(CodeAnalysisKeys.DisplayName))}\"]");
		hierarchy.AppendLine("class Type type-node");

		if (derivedTypes is {Count: > 0})
		{
			for (Int32 c = 0 ; c < derivedTypes.Count ; c++)
			{
				hierarchy.AppendLine($"\tDerived{c}[\"{WebUtility.HtmlEncode(derivedTypes[c].GetString(CodeAnalysisKeys.DisplayName))}\"]-->Type");
				if (!derivedTypes[c].Destination.IsNullOrEmpty)
					hierarchy.AppendLine($"\tclick Derived{c} \"{(context.GetLink(derivedTypes[c].Destination.FullPath))}\"");
			}
		}

		if (implementingTypes is {Count: > 0})
		{
			for (Int32 c = 0 ; c < implementingTypes.Count ; c++)
			{
				hierarchy.AppendLine($"\tImplementing{c}[\"{WebUtility.HtmlEncode(implementingTypes[c].GetString(CodeAnalysisKeys.DisplayName))}\"]-.->Type");
				if (!implementingTypes[c].Destination.IsNullOrEmpty)
					hierarchy.AppendLine($"\tclick Implementing{c} \"{context.GetLink(implementingTypes[c].Destination.FullPath)}\"");
			}
		}

		yield return context.CreateDocument(input.Source, input.Destination.ChangeFileName("type_diagram.mmd"), new Statiq.Common.StringContent(hierarchy.ToString()));
	}
}
