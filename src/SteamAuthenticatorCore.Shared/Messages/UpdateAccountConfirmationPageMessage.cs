﻿namespace SteamAuthenticatorCore.Shared.Messages;

public class UpdateAccountConfirmationPageMessage : ValueChangedMessage<SteamGuardAccountConfirmationsModel>
{
    public UpdateAccountConfirmationPageMessage(SteamGuardAccountConfirmationsModel value) : base(value)
    {
    }
}
