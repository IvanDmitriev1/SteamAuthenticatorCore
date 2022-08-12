using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Extensions;

public static class ViewExtensions
{
    public static Task<bool> BackgroundColorTo(this VisualElement self, Color toColor, uint length = 250, Easing? easing = null)
    {
        Func<double, Color> transform = (t) =>
            Color.FromRgba(self.BackgroundColor.R + t * (toColor.R - self.BackgroundColor.R),
                self.BackgroundColor.G + t * (toColor.G - self.BackgroundColor.G),
                self.BackgroundColor.B + t * (toColor.B - self.BackgroundColor.B),
                self.BackgroundColor.A + t * (toColor.A - self.BackgroundColor.A));

        return ColorAnimation(self, "BackgroundColorTo", transform, color =>
        {
            self.BackgroundColor = color;
        }, length, easing);
    }

    public static void CancelAnimation(this VisualElement self)
    {
        self.AbortAnimation("BackgroundColorTo");
    }

    static Task<bool> ColorAnimation(VisualElement element, string name, Func<double, Color> transform, Action<Color> callback, uint length, Easing? easing)
    {
        easing = easing ?? Easing.Linear;
        var taskCompletionSource = new TaskCompletionSource<bool>();

        element.Animate<Color>(name, transform, callback, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));
        return taskCompletionSource.Task;
    }
}