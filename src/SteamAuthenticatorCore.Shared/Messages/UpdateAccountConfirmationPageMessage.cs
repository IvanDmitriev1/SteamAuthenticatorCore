using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Messages;

public class UpdateAccountConfirmationPageMessage : ValueChangedMessage<ConfirmationViewModelBase>
{
    public UpdateAccountConfirmationPageMessage(ConfirmationViewModelBase value) : base(value)
    {
    }
}
