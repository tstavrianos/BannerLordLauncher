namespace BannerLordLauncher.Controls.MessageBox
{
    using System.Windows;

    public static class MyMessageBox
    {
        public static MessageBoxResult Show(Window parent, string message, string caption = "", MessageBoxButton buttons = MessageBoxButton.OK)
        {
            var dialog = new MyMessageBoxView
            {
                Title = caption,
                tbMessage = { Text = message },
                Buttons = buttons,
                Owner = parent,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false
            };

            dialog.ShowDialog();
            var result = dialog.Result;

            return result;
        }
    }
}
