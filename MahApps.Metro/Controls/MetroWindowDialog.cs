using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MahApps.Metro.Native;
using System.Windows.Shapes;

namespace MahApps.Metro.Controls
{
    [Flags]
    public enum DialogButtons
    {
        None = 0,
        Ok = 1,
        Cancel = 2,
        Yes = 4,
        No = 8,
        Help = 16
    }

    /// <summary>
    /// An extended, metrofied Window class.
    /// </summary>
    [TemplatePart(Name = PART_Icon, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_TitleBar, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_WindowTitleBackground, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_LeftWindowCommands, Type = typeof(WindowCommands))]
    [TemplatePart(Name = PART_RightWindowCommands, Type = typeof(WindowCommands))]
    [TemplatePart(Name = PART_WindowButtonCommands, Type = typeof(WindowButtonCommands))]
    [TemplatePart(Name = PART_OverlayBox, Type = typeof(Grid))]
    [TemplatePart(Name = PART_MetroActiveDialogContainer, Type = typeof(Grid))]
    [TemplatePart(Name = PART_MetroInactiveDialogsContainer, Type = typeof(Grid))]
    [TemplatePart(Name = PART_FlyoutModal, Type = typeof(Rectangle))]
    [TemplatePart(Name = PART_HelpButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_OkButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_CancelButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_YesButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_NoButton, Type = typeof(Button))]
    public class MetroWindowDialog : MetroWindow
    {
        private const string PART_Icon = "PART_Icon";
        private const string PART_TitleBar = "PART_TitleBar";
        private const string PART_WindowTitleBackground = "PART_WindowTitleBackground";
        private const string PART_LeftWindowCommands = "PART_LeftWindowCommands";
        private const string PART_RightWindowCommands = "PART_RightWindowCommands";
        private const string PART_WindowButtonCommands = "PART_WindowButtonCommands";
        private const string PART_OverlayBox = "PART_OverlayBox";
        private const string PART_MetroActiveDialogContainer = "PART_MetroActiveDialogContainer";
        private const string PART_MetroInactiveDialogsContainer = "PART_MetroInactiveDialogsContainer";
        private const string PART_FlyoutModal = "PART_FlyoutModal";
        private const string PART_HelpButton = "PART_HelpButton";
        private const string PART_OkButton = "PART_OkButton";
        private const string PART_CancelButton = "PART_CancelButton";
        private const string PART_YesButton = "PART_YesButton";
        private const string PART_NoButton = "PART_NoButton";

        UIElement icon;
        UIElement titleBar;
        UIElement titleBarBackground;
        Rectangle flyoutModal;
        Thumb windowTitleThumb;

        public Button HelpButton { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelButton { get; private set; }
        public Button YesButton { get; private set; }
        public Button NoButton { get; private set; }
        public DialogButtons Buttons { get; set; }

        public event EventHandler HelpClicked;

        private static void OnEnableDWMDropShadowPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && (bool)e.NewValue)
            {
                var window = (MetroWindowDialog)d;
                window.UseDropShadow();
            }
        }

        private void UseDropShadow()
        {
            this.BorderThickness = new Thickness(0);
            this.BorderBrush = null;
            this.GlowBrush = Brushes.Black;
        }

        private static void WindowCommandsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && e.NewValue != null)
            {
                ((MetroWindowDialog)dependencyObject).RightWindowCommands = (WindowCommands)e.NewValue;
            }
        }

        private static void OnShowIconOnTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (MetroWindowDialog)d;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibiltyForIcon();
            }
        }

        private static void OnShowTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (MetroWindowDialog)d;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibiltyForAllTitleElements((bool)e.NewValue);
            }
        }

        private static object OnShowTitleBarCoerceValueCallback(DependencyObject d, object value)
        {
            // if UseNoneWindowStyle = true no title bar should be shown
            if (((MetroWindowDialog)d).UseNoneWindowStyle)
            {
                return false;
            }
            return value;
        }

        private static void OnUseNoneWindowStylePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                // if UseNoneWindowStyle = true no title bar should be shown
                var useNoneWindowStyle = (bool)e.NewValue;
                var window = (MetroWindowDialog)d;
                window.ToggleNoneWindowStyle(useNoneWindowStyle);
            }
        }

        private void ToggleNoneWindowStyle(bool useNoneWindowStyle)
        {
            // UseNoneWindowStyle means no title bar, window commands or min, max, close buttons
            if (useNoneWindowStyle)
            {
                ShowTitleBar = false;
            }
            if (LeftWindowCommandsPresenter != null)
            {
                LeftWindowCommandsPresenter.Visibility = useNoneWindowStyle ? Visibility.Collapsed : Visibility.Visible;
            }
            if (RightWindowCommandsPresenter != null)
            {
                RightWindowCommandsPresenter.Visibility = useNoneWindowStyle ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private static void TitlebarHeightPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = (MetroWindowDialog)dependencyObject;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibiltyForAllTitleElements((int)e.NewValue > 0);
            }
        }

        private void SetVisibiltyForIcon()
        {
            if (this.icon != null)
            {
                var isVisible = (this.IconOverlayBehavior.HasFlag(WindowCommandsOverlayBehavior.HiddenTitleBar) && !this.ShowTitleBar)
                                || (this.ShowIconOnTitleBar && this.ShowTitleBar);
                var iconVisibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                this.icon.Visibility = iconVisibility;
            }
        }

        private void SetVisibiltyForAllTitleElements(bool visible)
        {
            this.SetVisibiltyForIcon();
            var newVisibility = visible && this.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
            if (this.titleBar != null)
            {
                this.titleBar.Visibility = newVisibility;
            }
            if (this.titleBarBackground != null)
            {
                this.titleBarBackground.Visibility = newVisibility;
            }
            if (this.LeftWindowCommandsPresenter != null)
            {
                this.LeftWindowCommandsPresenter.Visibility = this.LeftWindowCommandsOverlayBehavior.HasFlag(WindowCommandsOverlayBehavior.HiddenTitleBar) ?
                    Visibility.Visible : newVisibility;
            }
            if (this.RightWindowCommandsPresenter != null)
            {
                this.RightWindowCommandsPresenter.Visibility = this.RightWindowCommandsOverlayBehavior.HasFlag(WindowCommandsOverlayBehavior.HiddenTitleBar) ?
                    Visibility.Visible : newVisibility;
            }
            if (this.WindowButtonCommands != null)
            {
                this.WindowButtonCommands.Visibility = this.WindowButtonCommandsOverlayBehavior.HasFlag(WindowCommandsOverlayBehavior.HiddenTitleBar) ?
                    Visibility.Visible : newVisibility;
            }

            SetWindowEvents();
        }

        /// <summary>
        /// Initializes a new instance of the MahApps.Metro.Controls.MetroWindowDialog class.
        /// </summary>
        public MetroWindowDialog()
        {
            Loaded += MetroWindowDialog_Loaded;
            Buttons = DialogButtons.Ok | DialogButtons.Cancel;
        }

        private void MetroWindowDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (EnableDWMDropShadow)
            {
                this.UseDropShadow();
            }

            if (this.WindowTransitionsEnabled)
            {
                VisualStateManager.GoToState(this, "AfterLoaded", true);
            }

            this.ToggleNoneWindowStyle(this.UseNoneWindowStyle);

            if (this.Flyouts == null)
            {
                this.Flyouts = new FlyoutsControl();
            }

            this.ResetAllWindowCommandsBrush();

            ThemeManager.IsThemeChanged += ThemeManagerOnIsThemeChanged;
            this.Unloaded += (o, args) => ThemeManager.IsThemeChanged -= ThemeManagerOnIsThemeChanged;
        }

        private void MetroWindowDialog_SizeChanged(object sender, RoutedEventArgs e)
        {
            // this all works only for CleanWindow style

            var titleBarGrid = (Grid)titleBar;
            var titleBarLabel = (Label)titleBarGrid.Children[0];
            var titleControl = (ContentControl)titleBarLabel.Content;
            var iconContentControl = (ContentControl)icon;

            // Half of this MetroWindowDialog
            var halfDistance = this.Width / 2;
            // Distance between center and left/right
            var distanceToCenter = titleControl.ActualWidth / 2;
            // Distance between right edge from LeftWindowCommands to left window side
            var distanceFromLeft = iconContentControl.ActualWidth + LeftWindowCommands.ActualWidth;
            // Distance between left edge from RightWindowCommands to right window side
            var distanceFromRight = WindowButtonCommands.ActualWidth + RightWindowCommands.ActualWidth;
            // Margin
            const double horizontalMargin = 5.0;

            if ((distanceFromLeft + distanceToCenter + horizontalMargin < halfDistance) && (distanceFromRight + distanceToCenter + horizontalMargin < halfDistance))
            {
                Grid.SetColumn(titleBarGrid, 0);
                Grid.SetColumnSpan(titleBarGrid, 5);
            }
            else
            {
                Grid.SetColumn(titleBarGrid, 2);
                Grid.SetColumnSpan(titleBarGrid, 1);
            }
        }

        private void ThemeManagerOnIsThemeChanged(object sender, OnThemeChangedEventArgs e)
        {
            if (e.Accent != null)
            {
                var flyouts = this.Flyouts.GetFlyouts().ToList();
                // since we disabled the ThemeManager OnThemeChanged part, we must change all children flyouts too
                // e.g if the FlyoutsControl is hosted in a UserControl
                var allChildFlyouts = (this.Content as DependencyObject).FindChildren<FlyoutsControl>(true).ToList();
                if (allChildFlyouts.Any())
                {
                    flyouts.AddRange(allChildFlyouts.SelectMany(flyoutsControl => flyoutsControl.GetFlyouts()));
                }

                if (!flyouts.Any())
                {
                    // we must update the window command brushes!!!
                    this.ResetAllWindowCommandsBrush();
                    return;
                }

                foreach (var flyout in flyouts)
                {
                    flyout.ChangeFlyoutTheme(e.Accent, e.AppTheme);
                }
                this.HandleWindowCommandsForFlyouts(flyouts);
            }
        }

        private void FlyoutsPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (e.OriginalSource as DependencyObject);
            if (element != null)
            {
                // no preview if we just clicked these elements
                if (element.TryFindParent<Flyout>() != null
                    || Equals(element.TryFindParent<ContentControl>(), this.icon)
                    || element.TryFindParent<WindowCommands>() != null
                    || element.TryFindParent<WindowButtonCommands>() != null)
                {
                    return;
                }
            }

            if (Flyouts.OverrideExternalCloseButton == null)
            {
                foreach (var flyout in Flyouts.GetFlyouts().Where(x => x.IsOpen && x.ExternalCloseButton == e.ChangedButton && (!x.IsPinned || Flyouts.OverrideIsPinned)))
                {
                    flyout.IsOpen = false;
                    e.Handled = true;
                }
            }
            else if (Flyouts.OverrideExternalCloseButton == e.ChangedButton)
            {
                foreach (var flyout in Flyouts.GetFlyouts().Where(x => x.IsOpen && (!x.IsPinned || Flyouts.OverrideIsPinned)))
                {
                    flyout.IsOpen = false;
                    e.Handled = true;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            Close();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpClicked?.Invoke(null, null);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        static MetroWindowDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroWindowDialog), new FrameworkPropertyMetadata(typeof(MetroWindowDialog)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (LeftWindowCommands == null)
                LeftWindowCommands = new WindowCommands();
            if (RightWindowCommands == null)
                RightWindowCommands = new WindowCommands();

            LeftWindowCommands.ParentWindow = this;
            RightWindowCommands.ParentWindow = this;

            LeftWindowCommandsPresenter = GetTemplateChild(PART_LeftWindowCommands) as ContentPresenter;
            RightWindowCommandsPresenter = GetTemplateChild(PART_RightWindowCommands) as ContentPresenter;
            WindowButtonCommands = GetTemplateChild(PART_WindowButtonCommands) as WindowButtonCommands;

            if (WindowButtonCommands != null)
                WindowButtonCommands.ParentWindow = this;

            overlayBox = GetTemplateChild(PART_OverlayBox) as Grid;
            metroActiveDialogContainer = GetTemplateChild(PART_MetroActiveDialogContainer) as Grid;
            metroInactiveDialogContainer = GetTemplateChild(PART_MetroInactiveDialogsContainer) as Grid;
            flyoutModal = (Rectangle)GetTemplateChild(PART_FlyoutModal);
            flyoutModal.PreviewMouseDown += FlyoutsPreviewMouseDown;
            this.PreviewMouseDown += FlyoutsPreviewMouseDown;

            icon = GetTemplateChild(PART_Icon) as UIElement;
            titleBar = GetTemplateChild(PART_TitleBar) as UIElement;
            titleBarBackground = GetTemplateChild(PART_WindowTitleBackground) as UIElement;

            SetVisibiltyForAllTitleElements(TitlebarHeight > 0);

            SetDialogButtons();
        }

        private void SetDialogButtons()
        {
            // Dialog buttons
            HelpButton = GetTemplateChild(PART_HelpButton) as Button;
            if (HelpButton != null)
                HelpButton.Click += HelpButton_Click; ;

            OkButton = GetTemplateChild(PART_OkButton) as Button;
            if (OkButton != null)
                OkButton.Click += OkButton_Click;

            CancelButton = GetTemplateChild(PART_CancelButton) as Button;
            if (CancelButton != null)
                CancelButton.Click += CancelButton_Click;

            YesButton = GetTemplateChild(PART_YesButton) as Button;
            if (YesButton != null)
                YesButton.Click += OkButton_Click;

            NoButton = GetTemplateChild(PART_NoButton) as Button;
            if (NoButton != null)
                NoButton.Click += NoButton_Click;

            HelpButton.Visibility = Visibility.Collapsed;
            OkButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            YesButton.Visibility = Visibility.Collapsed;
            NoButton.Visibility = Visibility.Collapsed;


            if (Buttons.HasFlag(DialogButtons.Ok))
                OkButton.Visibility = Visibility.Visible;

            if (Buttons.HasFlag(DialogButtons.Cancel))
                CancelButton.Visibility = Visibility.Visible;

            if (Buttons.HasFlag(DialogButtons.Yes))
                YesButton.Visibility = Visibility.Visible;

            if (Buttons.HasFlag(DialogButtons.No))
                NoButton.Visibility = Visibility.Visible;

            if (Buttons.HasFlag(DialogButtons.Help))
                HelpButton.Visibility = Visibility.Visible;
        }

        private void ClearWindowEvents()
        {
            // clear all event handlers first:
            if (this.windowTitleThumb != null)
            {
                this.windowTitleThumb.DragDelta -= this.WindowTitleThumbMoveOnDragDelta;
                this.windowTitleThumb.MouseDoubleClick -= this.WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
                this.windowTitleThumb.MouseRightButtonUp -= this.WindowTitleThumbSystemMenuOnMouseRightButtonUp;
            }
            if (icon != null)
            {
                icon.MouseDown -= IconMouseDown;
            }
            SizeChanged -= MetroWindowDialog_SizeChanged;
        }

        private void SetWindowEvents()
        {
            // clear all event handlers first
            this.ClearWindowEvents();

            // set mouse down/up for icon
            if (icon != null && icon.Visibility == Visibility.Visible)
            {
                icon.MouseDown += IconMouseDown;
            }

            if (this.windowTitleThumb != null)
            {
                this.windowTitleThumb.DragDelta += this.WindowTitleThumbMoveOnDragDelta;
                this.windowTitleThumb.MouseDoubleClick += this.WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
                this.windowTitleThumb.MouseRightButtonUp += this.WindowTitleThumbSystemMenuOnMouseRightButtonUp;
            }

            // handle size if we have a Grid for the title (e.g. clean window have a centered title)
            if (titleBar != null && titleBar.GetType() == typeof(Grid))
            {
                SizeChanged += MetroWindowDialog_SizeChanged;
            }
        }

        private void IconMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    Close();
                }
                else
                {
                    ShowSystemMenuPhysicalCoordinates(this, PointToScreen(new Point(0, TitlebarHeight)));
                }
            }
        }

        private void WindowTitleThumbMoveOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
        {
            DoWindowTitleThumbMoveOnDragDelta(this, dragDeltaEventArgs);
        }

        private void WindowTitleThumbChangeWindowStateOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            DoWindowTitleThumbChangeWindowStateOnMouseDoubleClick(this, mouseButtonEventArgs);
        }

        private void WindowTitleThumbSystemMenuOnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DoWindowTitleThumbSystemMenuOnMouseRightButtonUp(this, e);
        }

        private static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
        {
            if (window == null) return;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero || !UnsafeNativeMethods.IsWindow(hwnd))
                return;

            var hmenu = UnsafeNativeMethods.GetSystemMenu(hwnd, false);

            var cmd = UnsafeNativeMethods.TrackPopupMenuEx(hmenu, Constants.TPM_LEFTBUTTON | Constants.TPM_RETURNCMD,
                (int)physicalScreenLocation.X, (int)physicalScreenLocation.Y, hwnd, IntPtr.Zero);
            if (0 != cmd)
                UnsafeNativeMethods.PostMessage(hwnd, Constants.SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
        }
    }
}
