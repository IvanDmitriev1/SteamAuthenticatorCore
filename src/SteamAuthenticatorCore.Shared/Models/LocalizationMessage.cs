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
    ChangeMessage,
    CancelMessage,
    ConfirmMessage,

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
    YouAreUsingTheLatestVersionMessage,
    UpdaterMessage,

    //Login page
    AccountNameMessage,
    PasswordMessage,
    RefreshSessionMessage,
    FailedToRefreshSessionMessage,
    SessionHasBeenRefreshedMessage,
}