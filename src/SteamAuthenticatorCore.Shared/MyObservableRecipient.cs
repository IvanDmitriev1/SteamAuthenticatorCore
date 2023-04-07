namespace SteamAuthenticatorCore.Shared;

public abstract class MyObservableRecipient : ObservableObject
{
    protected MyObservableRecipient() :this(WeakReferenceMessenger.Default) { }

    protected MyObservableRecipient(IMessenger messenger)
    {
        Messenger = messenger;

        Messenger.RegisterAll(this);
        _isMessengerRegistered = true;
    }

    protected IMessenger Messenger { get; }
    private bool _isMessengerRegistered;
    private bool _isActive;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;

            if (value)
                OnActivated();
            else
                OnDeactivated();
        }
    }

    protected virtual void OnActivated()
    {
        if (_isMessengerRegistered)
            return;

        Messenger.RegisterAll(this);
        _isMessengerRegistered = true;
    }

    protected virtual void OnDeactivated()
    {
        Messenger.UnregisterAll(this);
        _isMessengerRegistered = false;
    }
}