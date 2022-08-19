using System;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Mobile.Helpers;

public interface IBackButtonAction
{
    public Func<Task<bool>>? OnBackActionAsync { get; set; }
}