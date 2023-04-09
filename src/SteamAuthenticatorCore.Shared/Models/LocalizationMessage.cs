namespace SteamAuthenticatorCore.Shared.Models;

public enum LocalizationMessage
{
    None,
    HelloMessage,

    CopyMessage,
    CopiedMessage,
    UpdateMessage,
    ErrorMessage,
    YesMessage,
    NoMessage,

    //Menu items
    Token,
    Settings,
    Confirmations,
    Language,

    //Token page
    FileMessage,
    ImportAccountsMessage,
    ShowAccountsFileFolder,
    SelectedAccountMessage,
    LoginMessage,
    ForceRefreshSessionMessage,
    DeleteMessage,
    SearchPlaceholderMessage,
    NoItemsToDisplayMessage,
    ShowGoogleAccountFilesFolderContentMessage,
    DeletingAccountMessage,
    DeletingAccountContentMessage,

    //Settings page
    SelectLanguageMessage,
    SelectMaFilesLocationMessage,
    EnableAutoConfirmMarketTransactionsMessage,
    SecondsBetweenCheckingForConfirmationsMessage,
    CurrentVersionMessage,
    CheckForUpdatesMessage,

    //AccountConfirmations page
    ConfirmMessage,
    CancelMessage,

    //Login page
    AccountNameMessage,
    PasswordMessage,
    RefreshSessionMessage,
    FailedToRefreshSessionMessage,
    SessionHasBeenRefreshedMessage,
}