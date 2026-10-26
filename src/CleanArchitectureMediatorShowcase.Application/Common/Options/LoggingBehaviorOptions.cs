namespace CleanArchitectureMediatorShowcase.Application.Common.Options;

/// <summary>
/// Configuration options for <see cref="CleanArchitectureMediatorShowcase.Application.Common.Behaviors.LoggingBehavior{TMessage, TResponse}"/>.
/// Bound from the "LoggingBehavior" configuration section.
/// </summary>
public sealed class LoggingBehaviorOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "LoggingBehavior";

    /// <summary>
    /// Threshold in milliseconds above which a request is logged with a Warning level as a slow request.
    /// Default is 500ms.
    /// </summary>
    public int SlowRequestThresholdMilliseconds { get; set; } = 500;
}
