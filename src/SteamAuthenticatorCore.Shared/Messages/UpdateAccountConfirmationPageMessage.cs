using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Messages;

public class UpdateAccountConfirmationPageMessage : ValueChangedMessage<ConfirmationModel>
{
    public UpdateAccountConfirmationPageMessage(ConfirmationModel value) : base(value)
    {
    }
}
