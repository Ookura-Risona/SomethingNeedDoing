namespace SomethingNeedDoing.Core;

public class MacroException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MacroException"/> class.
    /// </summary>
    /// <param name="message">Message to show.</param>
    public MacroException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroException"/> class.
    /// </summary>
    /// <param name="message">Message to show.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MacroException(string message, Exception? innerException) : base(message, innerException) { }
}
