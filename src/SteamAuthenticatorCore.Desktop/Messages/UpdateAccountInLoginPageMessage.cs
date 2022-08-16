using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAuthCore;

namespace SteamAuthenticatorCore.Desktop.Messages;

public class UpdateAccountInLoginPageMessage : ValueChangedMessage<SteamGuardAccount>
{
    public UpdateAccountInLoginPageMessage(SteamGuardAccount value) : base(value)
    {
    }
}
