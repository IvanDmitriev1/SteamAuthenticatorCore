using CommunityToolkit.Mvvm.Messaging.Messages;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Messages;

public class UpdateAccountConfirmationPageMessage : ValueChangedMessage<ConfirmationAccountModelBase>
{
    public UpdateAccountConfirmationPageMessage(ConfirmationAccountModelBase value) : base(value)
    {
    }
}
