using System.Drawing;

namespace RoguelikeSkeleton;

/// <summary>
/// A rolling log of recent game events. Game writes to it; the front-end
/// (GameForm) reads Messages and draws them however it likes - the game
/// logic itself stays UI-agnostic.
/// </summary>
public class MessageLog
{
    private const int MaxMessages = 200;

    private readonly List<LogMessage> _messages = new();

    public IReadOnlyList<LogMessage> Messages => _messages;

    public void Add(string text, Color? color = null)
    {
        _messages.Add(new LogMessage(text, color ?? Color.Gainsboro));
        if (_messages.Count > MaxMessages)
            _messages.RemoveAt(0);
    }
}

public readonly struct LogMessage
{
    public readonly string Text;
    public readonly Color Color;

    public LogMessage(string text, Color color)
    {
        Text = text;
        Color = color;
    }
}
