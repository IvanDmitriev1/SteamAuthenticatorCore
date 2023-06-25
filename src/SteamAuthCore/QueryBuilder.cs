namespace SteamAuthCore;

internal sealed class QueryBuilder
{
	private readonly List<KeyValuePair<string, string>> _keyValuePairs = new();

	public override string ToString()
	{
		var sb = new StringBuilder(135);

		foreach (var (key, value) in _keyValuePairs)
		{
			sb.Append(key);
			sb.Append('=');
			sb.Append(value);
			sb.Append('&');
		}

		sb.Remove(sb.Length - 1, 1);

		return sb.ToString();
	}

	public IReadOnlyList<KeyValuePair<string, string>> ToKeyValuePairs() => _keyValuePairs.AsReadOnly();

	public QueryBuilder AddParameter(string key, string? value)
	{
		if (value is null)
			return this;

		_keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
		return this;
	}
}