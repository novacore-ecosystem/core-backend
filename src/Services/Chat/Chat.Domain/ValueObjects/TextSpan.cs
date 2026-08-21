namespace NovaCore.Chat.Domain.ValueObjects;

/// <summary>Character span within a message's Content, used to anchor a MessageMention to the text it highlights.</summary>
public sealed class TextSpan : ValueObject
{
    public int StartOffset { get; }
    public int Length { get; }

    private TextSpan(int startOffset, int length)
    {
        StartOffset = startOffset;
        Length = length;
    }

    public static TextSpan Create(int startOffset, int length)
    {
        if (startOffset < 0)
            throw ExceptionFactory.ValueTooSmall("Mention start offset cannot be negative.");

        if (length <= 0)
            throw ExceptionFactory.ValueTooSmall("Mention length must be greater than zero.");

        return new TextSpan(startOffset, length);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartOffset;
        yield return Length;
    }
}
