using System.Threading.Tasks.Dataflow;
using Statiq.Common;

namespace Ociaw.StatiqMermaid;

/// <summary>
/// A base class for modules that can execute in parallel, using TPL Dataflow. The maximum degree of Parallelism can be
/// limited by setting <see cref="MaxDegreeOfParallelism"/>.
/// </summary>
/// <remarks>
/// Documents are not executed in parallel if <see cref="Parallel"/> is <c>false</c> or if
/// <see cref="IExecutionState.SerialExecution"/> is <c>true</c>.
/// </remarks>
public abstract class TplParallelModule : Module, IParallelModule
{
    /// <inheritdoc cref="ExecutionDataflowBlockOptions.MaxDegreeOfParallelism"/>
    /// <remarks>Defaults to <see cref="Environment.ProcessorCount"/>.</remarks>
    public Config<Int32> MaxDegreeOfParallelism { get; init; } = Config.FromSetting(nameof(MaxDegreeOfParallelism), Environment.ProcessorCount);

    /// <inheritdoc />
    public Boolean Parallel { get; set; } = true;

    /// <inheritdoc />
    protected sealed override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
    {
        // Limit the parallelism to 1 if Parallel execution is disabled
        if (!Parallel || context.SerialExecution)
            return await base.ExecuteContextAsync(context);

        var maxParallelism = await MaxDegreeOfParallelism.GetValueAsync(null, context);
        var results = new List<IDocument>(context.Inputs.Length);
        var inputBlock = new BufferBlock<IDocument>();
        var renderBlock = new TransformBlock<IDocument, IEnumerable<IDocument>>(
            async input => await ExecuteInputFuncAsync(input, context, ExecuteInputAsync),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelism, CancellationToken = context.CancellationToken }
        );
        var resultBlock = new ActionBlock<IEnumerable<IDocument>>(docs => results.AddRange(docs), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        inputBlock.LinkTo(renderBlock, linkOptions);
        renderBlock.LinkTo(resultBlock, linkOptions);

        foreach (var doc in context.Inputs)
        {
            if (!inputBlock.Post(doc))
                context.LogError(doc, $"Input block did not accept document: {doc.Source}. Is the buffer full?");
        }
        inputBlock.Complete();

        await resultBlock.Completion;
        return results;
    }

    // The following function is pulled from Statiq Framework since it isn't exposed to the public
    // https://github.com/statiqdev/Statiq.Framework/blob/e990cb1b4d0884dc1e3f9371aa1d55551c5ecf18/src/core/Statiq.Common/Modules/Module.cs#L157
    private static async Task<IEnumerable<IDocument>> ExecuteInputFuncAsync(
        IDocument input,
        IExecutionContext context,
        Func<IDocument, IExecutionContext, Task<IEnumerable<IDocument>?>> executeFunc)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        try
        {
            return (await executeFunc(input, context)) ?? Array.Empty<IDocument>();
        }
        catch (Exception ex)
        {
            throw input.LogAndWrapException(ex);
        }
    }
}