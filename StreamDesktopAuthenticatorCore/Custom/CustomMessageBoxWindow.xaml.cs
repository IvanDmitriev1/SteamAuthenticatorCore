using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SteamDesktopAuthenticatorCore.Custom
{
    partial class CustomMessageBoxWindow : Window
    {
        public CustomMessageBoxWindow(string text)
        {
            InitializeComponent();

            MainText = text;
            ExitResult = MessageBoxResult.None;
            OkGridVisibility = Visibility.Visible;
        }

        public CustomMessageBoxWindow(string text, string title, MessageBoxButton button) : this(text)
        {
            OkGridVisibility = Visibility.Hidden;
            TitleText = title;

            switch (button)
            {
                case MessageBoxButton.OK:
                    OkGridVisibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    break;
                case MessageBoxButton.YesNoCancel:
                    YesNoCancelGridVisibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    YesNoGridVisibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        public CustomMessageBoxWindow(string text, string title, MessageBoxButton button, MessageBoxImage image) : this(text, title, button)
        {
            if (image == MessageBoxImage.None)
                return;

            Image.Source = image switch
            {
                MessageBoxImage.Warning => BitmapToBitmapImage(SystemIcons.Warning.ToBitmap()),
                MessageBoxImage.Error => BitmapToBitmapImage(SystemIcons.Hand.ToBitmap()),
                MessageBoxImage.Information => BitmapToBitmapImage(SystemIcons.Information.ToBitmap()),
                MessageBoxImage.Question => BitmapToBitmapImage(SystemIcons.Question.ToBitmap()),
                _ => throw new ArgumentOutOfRangeException(nameof(image), image, null)
            };

            Image.Visibility = Visibility.Visible;
            TextBlock.Margin = new Thickness(55, 0, 0, 0);
            Width = 315;
        }

        #region Properties

        public MessageBoxResult ExitResult { get; private set; }

        public TextAlignment MainTextAlignment
        {
            get => (TextAlignment)GetValue(MainTextAlignmentProperty);
            init => SetValue(MainTextAlignmentProperty, value);
        }

        public TextWrapping MainTextWrapping
        {
            get => (TextWrapping)GetValue(MainTextWrappingProperty);
            init => SetValue(MainTextWrappingProperty, value);
        }

        public double MainTextFont
        {
            get => (double)GetValue(MainTextFontProperty);
            init => SetValue(MainTextFontProperty, value);
        }

        private string MainText
        {
            get => (string)GetValue(MainTextProperty);
            init => SetValue(MainTextProperty, value);
        }

        private string TitleText
        {
            get => (string)GetValue(TitleTextProperty);
            init => SetValue(TitleTextProperty, value);
        }

        private Visibility OkGridVisibility
        {
            get => (Visibility)GetValue(OkGridVisibilityProperty);
            init => SetValue(OkGridVisibilityProperty, value);
        }

        private Visibility YesNoGridVisibility
        {
            get => (Visibility)GetValue(YesNoGridVisibilityProperty);
            init => SetValue(YesNoGridVisibilityProperty, value);
        }

        private Visibility YesNoCancelGridVisibility
        {
            get => (Visibility)GetValue(YesNoCancelGridVisibilityProperty);
            init => SetValue(YesNoCancelGridVisibilityProperty, value);
        }

        #endregion

        #region DependencyPropery

        public static readonly DependencyProperty MainTextFontProperty =
            DependencyProperty.Register(nameof(MainTextFont), typeof(double), typeof(CustomMessageBoxWindow), new PropertyMetadata(14.0));

        public static readonly DependencyProperty MainTextAlignmentProperty =
            DependencyProperty.Register(nameof(MainTextAlignment), typeof(TextAlignment), typeof(CustomMessageBoxWindow), new PropertyMetadata(TextAlignment.Justify));

        public static readonly DependencyProperty MainTextWrappingProperty =
            DependencyProperty.Register(nameof(MainTextWrapping), typeof(TextWrapping), typeof(CustomMessageBoxWindow), new PropertyMetadata(TextWrapping.WrapWithOverflow));

        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(CustomMessageBoxWindow), new PropertyMetadata(""));

        public static readonly DependencyProperty MainTextProperty =
            DependencyProperty.Register(nameof(MainText), typeof(string), typeof(CustomMessageBoxWindow), new PropertyMetadata(""));

        #region Grids

        public static readonly DependencyProperty OkGridVisibilityProperty =
            DependencyProperty.Register(nameof(OkGridVisibility), typeof(Visibility), typeof(CustomMessageBoxWindow), new PropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty YesNoGridVisibilityProperty =
            DependencyProperty.Register(nameof(YesNoGridVisibility), typeof(Visibility), typeof(CustomMessageBoxWindow), new PropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty YesNoCancelGridVisibilityProperty =
            DependencyProperty.Register(nameof(YesNoCancelGridVisibility), typeof(Visibility), typeof(CustomMessageBoxWindow), new PropertyMetadata(Visibility.Hidden));

        #endregion

        #endregion

        public override void OnApplyTemplate()
        {
            BindingSetUp();

            GridBindingSetUp();

            base.OnApplyTemplate();
        }

        private void BindingSetUp()
        {
            Binding titleLabelTextBinding = new()
            {
                Path = new PropertyPath(nameof(TitleText)),
                Source = this
            };

            TitleLabel.SetBinding(ContentProperty, titleLabelTextBinding);

            Binding mainTextFontBinding = new()
            {
                Path = new PropertyPath(nameof(MainTextFont)),
                Source = this
            };

            TextBlock.SetBinding(FontSizeProperty, mainTextFontBinding);
        }

        private void GridBindingSetUp()
        {
            Binding okGridVisibilityBinding = new()
            {
                Path = new PropertyPath(nameof(OkGridVisibility)),
                Source = this
            };

            Binding yesNoGridVisibilityBinding = new()
            {
                Path = new PropertyPath(nameof(YesNoGridVisibility)),
                Source = this
            };

            Binding yesNoCancelBinding = new()
            {
                Path = new PropertyPath(nameof(YesNoCancelGridVisibility)),
                Source = this
            };

            OkGrid.SetBinding(VisibilityProperty, okGridVisibilityBinding);
            YesNoGrid.SetBinding(VisibilityProperty, yesNoGridVisibilityBinding);
            YesNoCancelGrid.SetBinding(VisibilityProperty, yesNoCancelBinding);

        }

        #region RutedEvents

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            ExitResult = MessageBoxResult.OK;
            Close();
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            ExitResult = MessageBoxResult.Yes;
            Close();
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            ExitResult = MessageBoxResult.No;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            ExitResult = MessageBoxResult.Cancel;
            Close();
        }

        #endregion

        private static BitmapImage BitmapToBitmapImage(in Bitmap bitmap)
        {
            using MemoryStream stream = new();
            bitmap.Save(stream, ImageFormat.Png); // Pit: When the format is Bmp, no transparency

            stream.Position = 0;
            BitmapImage result = new();
            result.BeginInit();
            // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
            // Force the bitmap to load right now so we can dispose the stream.
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }
    }
    public static class CustomMessageBox
    {
        public static MessageBoxResult Show(string text, TextAlignment mainTextAlignment = TextAlignment.Center, double textFont = 16, TextWrapping mainTextWrapping = TextWrapping.WrapWithOverflow)
        {
            return Show(text, "", MessageBoxButton.OK, MessageBoxImage.None, mainTextAlignment, textFont, mainTextWrapping);
        }

        public static MessageBoxResult Show(string text, string title, MessageBoxButton button, TextAlignment mainTextAlignment = TextAlignment.Left, double textFont = 14, TextWrapping mainTextWrapping = TextWrapping.WrapWithOverflow)
        {
            return Show(text, title, button, MessageBoxImage.None, mainTextAlignment, textFont, mainTextWrapping);
        }

        public static MessageBoxResult Show(string text, string title, MessageBoxButton button, MessageBoxImage image, TextAlignment mainTextAlignment = TextAlignment.Left, double textFont = 14, TextWrapping mainTextWrapping = TextWrapping.WrapWithOverflow)
        {
            CustomMessageBoxWindow messageBoxWindow = new(text, title, button, image)
            {
                MainTextFont = textFont,
                MainTextAlignment = mainTextAlignment,
                MainTextWrapping = mainTextWrapping
            };
            messageBoxWindow.ShowDialog();

            return messageBoxWindow.ExitResult;
        }
    }
}
