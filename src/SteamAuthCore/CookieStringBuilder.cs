namespace SteamAuthCore;

internal class CookieStringBuilder
{
    public CookieStringBuilder(int capacity)
    {
        _stringBuilder = new StringBuilder(capacity);
    }

    private readonly StringBuilder _stringBuilder;

    public override string ToString() => _stringBuilder.ToString();

    public CookieStringBuilder AddCookie(string key, string? value)
    {
        if (value is null)
            return this;

        _stringBuilder.Append(key);
        _stringBuilder.Append('=');
        _stringBuilder.Append(value);
        _stringBuilder.Append(';');

        return this;
    }
}