using System;
using System.ComponentModel;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ISettings : INotifyPropertyChanged
{
    public void Load();
    public void Save();
}

public sealed class IgnoreSetting : Attribute
{

}