/// <summary>
/// A struct for passing a message and a progress indicator
/// </summary>
public struct LevelLoadProgress
{
    /// <summary>
    /// Simply a message saying what is happening
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Progress should be a normalized value ranging from 0f - 1.0f
    /// </summary>
    public float Progress { get; set; }

    /// <summary>
    /// Level load progress
    /// </summary>
    /// <param name="message">What is happening?</param>
    /// <param name="progress">How complete are you. Range 0f - 1.0f</param>
    public LevelLoadProgress(string message, float progress)
        : this()
    {
        Message = message;
        Progress = progress;
    }
}