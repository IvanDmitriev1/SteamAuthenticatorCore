using System.Windows;
using Wpf.Ui.TaskBar;

namespace SteamAuthenticatorCore.Desktop.Services;

public class TaskBarService : Wpf.Ui.Services.TaskBarService
{
    public static TaskBarService Default { get; } = new TaskBarService();

    public void SetState(TaskBarProgressState state)
    {
        SetState(Application.Current.MainWindow!, state);
    }
}