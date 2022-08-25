using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAuthCore;
using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Shared.Messages;

public class UpdateAccountInLoginPageMessage : ValueChangedMessage<SteamGuardAccount>
{
    public UpdateAccountInLoginPageMessage(SteamGuardAccount value) : base(value)
    {
    }
}
