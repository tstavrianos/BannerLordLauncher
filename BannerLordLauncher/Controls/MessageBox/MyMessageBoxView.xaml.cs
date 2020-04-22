namespace BannerLordLauncher.Controls.MessageBox
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    public partial class MyMessageBoxView
    {
        #region Constructor
        public MyMessageBoxView()
        {
            this.InitializeComponent();
        }
        #endregion

        #region Private Variables
        private MessageBoxButton _Buttons;

        #endregion

        #region internal Properties
        internal MessageBoxButton Buttons
        {
            get => this._Buttons;
            set
            {
                this._Buttons = value;
                // Set all Buttons Visibility Properties
                this.SetButtonsVisibility();
            }
        }

        internal MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        #endregion

        #region SetButtonsVisibility Method

        private void SetButtonsVisibility()
        {
            switch (this._Buttons)
            {
                case MessageBoxButton.OK:
                    this.btnOk.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Collapsed;
                    this.btnYes.Visibility = Visibility.Collapsed;
                    this.btnNo.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    this.btnOk.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.btnYes.Visibility = Visibility.Collapsed;
                    this.btnNo.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    this.btnOk.Visibility = Visibility.Collapsed;
                    this.btnCancel.Visibility = Visibility.Collapsed;
                    this.btnYes.Visibility = Visibility.Visible;
                    this.btnNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    this.btnOk.Visibility = Visibility.Collapsed;
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.btnYes.Visibility = Visibility.Visible;
                    this.btnNo.Visibility = Visibility.Visible;
                    break;
            }
        }
        #endregion

        #region Button Click Events
        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.No;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.OK;
            this.Close();
        }
        #endregion

        #region Windows Drag Event
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }
        #endregion

        #region Deactivated Event
        private void Window_Deactivated(object sender, EventArgs e)
        {
            // If only an OK button is displayed, 
            // allow the user to just move away from this dialog box
            /*if (this.Buttons == MessageBoxButton.OK)
                this.Close();*/
        }
        #endregion
    }
}
