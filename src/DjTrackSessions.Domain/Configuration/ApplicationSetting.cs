namespace DjTrackSessions.Domain.Configuration;

public sealed class ApplicationSetting
{
    private ApplicationSetting()
    {
    }

    private ApplicationSetting(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public int Id { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public static ApplicationSetting Create(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A setting key is required.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A setting value is required.", nameof(value));
        }

        return new ApplicationSetting(key.Trim(), value.Trim());
    }

    public void ChangeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A setting value is required.", nameof(value));
        }

        Value = value.Trim();
    }
}
