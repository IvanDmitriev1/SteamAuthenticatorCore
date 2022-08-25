using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Messages;

public class UpdateAccountConfirmationPageMessage : ValueChangedMessage<IConfirmationViewModel>
{
    public UpdateAccountConfirmationPageMessage(IConfirmationViewModel value) : base(value)
    {
    }
}
