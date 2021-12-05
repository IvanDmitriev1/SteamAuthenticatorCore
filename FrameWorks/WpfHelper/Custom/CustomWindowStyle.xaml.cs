using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfHelper.Custom
{
    public enum WindowStyleButtonStates
    {
        Close,
        Hide,
        Minimize,
        Maximize,
        Normal
    }

    /// <summary>
    /// Use CustomWindowStyle.OnWindowLoaded method for proper work
    /// </summary>
    public partial class CustomWindowStyle : UserControl
    {
        public CustomWindowStyle()
        {
            InitializeComponent();
        }

        public Window? Window { get; set; }

        #region Poperties

        #region CloseButton

        public WindowStyleButtonStates CloseButtonShouldDo
        {
            get => (WindowStyleButtonStates)GetValue(CloseButtonShouldDoProperty);
            set => SetValue(CloseButtonShouldDoProperty, value);
        }

        public Visibility CloseButtonVisibility
        {
            get => (Visibility)GetValue(CloseButtonVisibilityProperty);
            set => SetValue(CloseButtonVisibilityProperty, value);
        }

        public string CloseButtonToolTip
        {
            get => (string)GetValue(CloseButtonToolTipProperty);
            set => SetValue(CloseButtonToolTipProperty, value);
        }

        #endregion

        #region MinimizeButton

        public WindowStyleButtonStates MinimizeButtonShouldDo
        {
            get => (WindowStyleButtonStates)GetValue(MinimizeButtonShouldDoProperty);
            set => SetValue(MinimizeButtonShouldDoProperty, value);
        }

        public Visibility MinimizeButtonVisibility
        {
            get => (Visibility)GetValue(MinimizeButtonVisibilityProperty);
            set => SetValue(MinimizeButtonVisibilityProperty, value);
        }

        public string MinimizeButtonToolTip
        {
            get => (string)GetValue(MinimizeButtonToolTipProperty);
            set => SetValue(MinimizeButtonToolTipProperty, value);
        }

        public Thickness MinimizeButtonMargin
        {
            get => (Thickness)GetValue(MinimizeButtonMarginProperty);

            set => SetValue(MinimizeButtonMarginProperty, value);
        }

        #endregion

        #endregion

        #region DependencyPropery

        #region CloseButton

        public static readonly DependencyProperty CloseButtonShouldDoProperty =
            DependencyProperty.Register(nameof(CloseButtonShouldDo), typeof(WindowStyleButtonStates), typeof(CustomWindowStyle), new PropertyMetadata(WindowStyleButtonStates.Close));

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register(nameof(CloseButtonVisibility), typeof(Visibility), typeof(CustomWindowStyle), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty CloseButtonToolTipProperty =
            DependencyProperty.Register(nameof(CloseButtonToolTip), typeof(string), typeof(CustomWindowStyle), new PropertyMetadata("Close"));

        #endregion

        #region MinimizeButton

        public static readonly DependencyProperty MinimizeButtonShouldDoProperty =
            DependencyProperty.Register(nameof(MinimizeButtonShouldDo), typeof(WindowStyleButtonStates), typeof(CustomWindowStyle), new PropertyMetadata(WindowStyleButtonStates.Minimize));

        public static readonly DependencyProperty MinimizeButtonVisibilityProperty =
            DependencyProperty.Register(nameof(MinimizeButtonVisibility), typeof(Visibility), typeof(CustomWindowStyle), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty MinimizeButtonToolTipProperty =
            DependencyProperty.Register(nameof(MinimizeButtonToolTip), typeof(string), typeof(CustomWindowStyle), new PropertyMetadata("Minimize"));

        public static readonly DependencyProperty MinimizeButtonMarginProperty =
            DependencyProperty.Register(nameof(MinimizeButtonMargin), typeof(Thickness), typeof(CustomWindowStyle), new PropertyMetadata(new Thickness(0, 0, 55, 0)));

        #endregion

        #endregion

        public override void OnApplyTemplate()
        {
            CloseButtonBindingSetUp();

            MinimizeButtonBindingSetUp();

            base.OnApplyTemplate();
        }

        public void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window window) return;

            Window = window;

            window.MouseLeftButtonDown += (sender, args) =>
            {
                if (sender is not Window window1) return;

                window1.DragMove();
                e.Handled = true;
            };
        }

        #region RoutedEvents

        private void CloseButtonOnClick(object sender, MouseButtonEventArgs e)
        {
            ButtonCanDo(CloseButtonShouldDo);
            e.Handled = true;
        }

        private void MinimizeButtonOnClick(object sender, MouseButtonEventArgs e)
        {
            ButtonCanDo(MinimizeButtonShouldDo);
            e.Handled = true;
        }

        #endregion

        #region PrivateMethods

        private void ButtonCanDo(WindowStyleButtonStates buttonStates)
        {
            switch (buttonStates)
            {
                case WindowStyleButtonStates.Close:
                    Window?.Close();
                    break;
                case WindowStyleButtonStates.Hide:
                    Window?.Hide();
                    break;
                case WindowStyleButtonStates.Minimize:
                    Window!.WindowState = WindowState.Minimized;
                    break;
                case WindowStyleButtonStates.Maximize:
                    Window!.WindowState = WindowState.Maximized;
                    break;
                case WindowStyleButtonStates.Normal:
                    Window!.WindowState = WindowState.Normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CloseButtonBindingSetUp()
        {
            Binding visibilityBinding = new()
            {
                Path = new PropertyPath(nameof(CloseButtonVisibility)),
                Source = this
            };

            Binding toolTipBinding = new()
            {
                Path = new PropertyPath(nameof(CloseButtonToolTip)),
                Source = this
            };

            CloseButton.SetBinding(VisibilityProperty, visibilityBinding);

            CloseButton.SetBinding(ToolTipProperty, toolTipBinding);
            CloseButtonImage.SetBinding(ToolTipProperty, toolTipBinding);
        }

        private void MinimizeButtonBindingSetUp()
        {
            Binding visibilityBinding = new()
            {
                Path = new PropertyPath(nameof(MinimizeButtonVisibility)),
                Source = this
            };

            Binding toolTipBinding = new()
            {
                Path = new PropertyPath(nameof(MinimizeButtonToolTip)),
                Source = this
            };

            Binding marginBinding = new()
            {
                Path = new PropertyPath(nameof(MinimizeButtonMargin)),
                Source = this
            };

            MinimizeButton.SetBinding(VisibilityProperty, visibilityBinding);

            MinimizeButton.SetBinding(ToolTipProperty, toolTipBinding);
            MinimizeButtonImage.SetBinding(ToolTipProperty, toolTipBinding);

            MinimizeButton.SetBinding(MarginProperty, marginBinding);
        }

        #endregion
    }
}