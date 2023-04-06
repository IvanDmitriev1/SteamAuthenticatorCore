namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ISettings : INotifyPropertyChanged
{
    public void Load();
    public void Save();
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class IgnoreSetting : Attribute
{

}