using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls.Dialogs;

namespace MetroDemo.ExampleWindows
{
    /// <summary>
    /// Interaction logic for WindowDialogDemo.xaml
    /// </summary>
    public partial class WindowDialogDemo
    {
        public WindowDialogDemo()
        {
            InitializeComponent();

            Buttons = DialogButtons.Help | DialogButtons.Yes | DialogButtons.No | DialogButtons.Ok | DialogButtons.Cancel;

            HelpClicked += WindowDialogDemo_HelpClicked;
        }

        private async void WindowDialogDemo_HelpClicked(object sender, EventArgs e)
        {
            await this.ShowMessageAsync("Help Button", "Help Button Clicked ");
        }
    }
}
