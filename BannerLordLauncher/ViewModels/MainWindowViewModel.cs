using System;
using Alphaleonis.Win32.Filesystem;
using System.Reactive.Linq;
using System.Windows.Input;
using BannerLord.Common;
using BannerLordLauncher.Views;
using ReactiveUI;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using Splat;
using Octokit;
using Application = System.Windows.Application;
using System.Windows.Threading;

namespace BannerLordLauncher.ViewModels
{
    using BannerLordLauncher.Controls.MessageBox;

    public sealed class MainWindowViewModel : ViewModelBase, IModManagerClient
    {
        public ModManager Manager { get; }
        private readonly MainWindow _window;
        private int _selectedIndex = -1;

        public int SelectedIndex
        {
            get => this._selectedIndex;
            set => this.RaiseAndSetIfChanged(ref this._selectedIndex, value);
        }

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public ICommand Config { get; }
        public ICommand Run { get; }
        public ICommand Save { get; }
        public ICommand Sort { get; }
        public ICommand MoveToTop { get; }
        public ICommand MoveUp { get; }
        public ICommand MoveDown { get; }
        public ICommand MoveToBottom { get; }
        public ICommand CheckAll { get; }
        public ICommand UncheckAll { get; }
        public ICommand InvertCheck { get; }
        public ICommand Copy { get; }
        public ICommand CopyChecked { get; }

        private string _windowTitle;
        public string WindowTitle
        {
            get => this._windowTitle;
            set => this.RaiseAndSetIfChanged(ref this._windowTitle, value);
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore MemberCanBePrivate.Global

        public MainWindowViewModel(MainWindow window)
        {
            this._window = window;
            this.Manager = new ModManager(this);

            var moveUp = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x > 0);
            var moveDown = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x >= 0 && x < this.Manager.Mods.Count - 1);

            this.Save = ReactiveCommand.Create(this.SaveCmd);
            this.Sort = ReactiveCommand.Create(this.SortCmd);
            this.MoveToTop = ReactiveCommand.Create(this.MoveToTopCmd, moveUp.Select(x => x));
            this.MoveUp = ReactiveCommand.Create(this.MoveUpCmd, moveUp.Select(x => x));
            this.MoveDown = ReactiveCommand.Create(this.MoveDownCmd, moveDown.Select(x => x));
            this.MoveToBottom = ReactiveCommand.Create(this.MoveToBottomCmd, moveDown.Select(x => x));
            this.CheckAll = ReactiveCommand.Create(this.CheckAllCmd);
            this.UncheckAll = ReactiveCommand.Create(this.UncheckAllCmd);
            this.InvertCheck = ReactiveCommand.Create(this.InvertCheckCmd);
            this.Run = ReactiveCommand.Create(this.RunCmd);
            this.Config = ReactiveCommand.Create(this.OpenConfigCmd);
            this.Copy = ReactiveCommand.Create(() => Clipboard.SetText(string.Join(Environment.NewLine, this.Manager.Mods.Select(x => x.Module.Id))));
            this.CopyChecked = ReactiveCommand.Create(() => Clipboard.SetText(string.Join(Environment.NewLine, this.Manager.Mods.Where(x => x.UserModData.IsSelected).Select(x => x.Module.Id))));
        }

        private static void SafeMessage(string message)
        {
            Debug.Assert(Application.Current.Dispatcher != null, "Application.Current.Dispatcher != null");
            if (Application.Current.Dispatcher.CheckAccess())
            {
                MyMessageBox.Show(Application.Current.MainWindow, message);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => MyMessageBox.Show(Application.Current.MainWindow, message)));
            }
        }

        private async void CheckForUpdates()
        {
            try
            {
                var github = new GitHubClient(new ProductHeaderValue("BannerLordLauncher"));
                var currentVersion = typeof(MainWindowViewModel).Assembly.GetName().Version;
                var result = await github.Repository.Release.GetAll("tstavrianos", "BannerLordLauncher")
                                 .ConfigureAwait(false);
                var latestRelease = result.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                if (latestRelease == null) return;
                this.WindowTitle = currentVersion.ToString();
                var latestReleaseVersion = new Version(latestRelease.TagName);
                if (latestReleaseVersion <= currentVersion) return;
                var message = $"Version {latestReleaseVersion} is available to download";
                SafeMessage(message);
            }
            catch (Exception e)
            {
                this.Log().Error(e);
            }
        }

        private void CheckAllCmd()
        {
            if (this.Manager.CheckAll(out var error)) return;
            if (!string.IsNullOrEmpty(error)) SafeMessage(error);
        }
        private void UncheckAllCmd()
        {
            if (this.Manager.UncheckAll(out var error)) return;
            if (!string.IsNullOrEmpty(error)) SafeMessage(error);
        }
        private void InvertCheckCmd()
        {
            if (this.Manager.InvertCheck(out var error)) return;
            if (!string.IsNullOrEmpty(error)) SafeMessage(error);
        }

        private void SortCmd()
        {
            var idx = this._window.ModList.SelectedIndex;
            ModEntry it = null;
            if (idx != -1)
                it = this.Manager.Mods[idx];
            this._window.ModList.SelectedIndex = -1;

            if(Program.Configuration.Sorting != null && Program.Configuration.Sorting.Count > 0){
                var mods = this.Manager.Mods.ToArray();
                this.Manager.Mods.Clear();

                var sorted = mods.OrderBy(x => 0);
                foreach (var sorting in Program.Configuration.Sorting)
                {
                    switch (sorting.Type)
                    {
                        case SortType.Id:
                            sorted = sorting.Ascending ? sorted.ThenBy(x => x.Module.Id) : sorted.ThenByDescending(x => x.Module.Id);
                            break;
                        case SortType.Name:
                            sorted = sorting.Ascending ? sorted.ThenBy(x => x.Module.Name) : sorted.ThenByDescending(x => x.Module.Name);
                            break;
                        case SortType.Version:
                            sorted = sorting.Ascending ? sorted.ThenBy(x => x.Module.Version) : sorted.ThenByDescending(x => x.Module.Version);
                            break;
                        case SortType.Official:
                            sorted = sorting.Ascending ? sorted.ThenBy(x => x.Module.Official ? 0 : 1) : sorted.ThenBy(x => x.Module.Official ? 1 : 0);
                            break;
                        case SortType.Native:
                            sorted = sorting.Ascending ? sorted.ThenBy(x => x.Module.Id == "Native" ? 0 : 1) : sorted.ThenBy(x => x.Module.Id == "Native" ? 1 : 0);
                            break;
                        case SortType.Selected:
                            sorted = sorting.Ascending ? sorted.ThenBy(x => x.UserModData.IsSelected ? 0 : 1) : sorted.ThenBy(x => x.UserModData.IsSelected ? 1 : 0);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                foreach (var m in sorted)
                {
                    this.Manager.Mods.Add(m);
                }
            }

            if (!this.Manager.Sort(out var errorMessage))
            {
                this._window.ModList.SelectedIndex = idx;
                if (!string.IsNullOrEmpty(errorMessage)) SafeMessage(errorMessage);
                return;
            }
            if (idx != -1) this._window.ModList.SelectedIndex = this.Manager.Mods.IndexOf(it);
        }

        private void MoveToTopCmd()
        {
            var idx = this._window.ModList.SelectedIndex;
            var it = this.Manager.Mods[idx];
            this._window.ModList.SelectedIndex = -1;
            if (!this.Manager.MoveToTop(idx, out var errorMessage))
            {
                this._window.ModList.SelectedIndex = idx;
                if (!string.IsNullOrEmpty(errorMessage)) SafeMessage(errorMessage);
                return;
            }
            this._window.ModList.SelectedIndex = this.Manager.Mods.IndexOf(it);
        }

        private void MoveUpCmd()
        {
            var idx = this._window.ModList.SelectedIndex;
            var it = this.Manager.Mods[idx];
            this._window.ModList.SelectedIndex = -1;
            if (!this.Manager.MoveUp(idx, out var errorMessage))
            {
                this._window.ModList.SelectedIndex = idx;
                if (!string.IsNullOrEmpty(errorMessage)) SafeMessage(errorMessage);
                return;
            }
            this._window.ModList.SelectedIndex = this.Manager.Mods.IndexOf(it);
        }

        private void MoveDownCmd()
        {
            var idx = this._window.ModList.SelectedIndex;
            var it = this.Manager.Mods[idx];
            this._window.ModList.SelectedIndex = -1;
            if (!this.Manager.MoveDown(idx, out var errorMessage))
            {
                this._window.ModList.SelectedIndex = idx;
                if (!string.IsNullOrEmpty(errorMessage)) SafeMessage(errorMessage);
                return;
            }
            this._window.ModList.SelectedIndex = this.Manager.Mods.IndexOf(it);
        }

        private void MoveToBottomCmd()
        {
            var idx = this._window.ModList.SelectedIndex;
            var it = this.Manager.Mods[idx];
            this._window.ModList.SelectedIndex = -1;
            if (!this.Manager.MoveToBottom(idx, out var errorMessage))
            {
                this._window.ModList.SelectedIndex = idx;
                if (!string.IsNullOrEmpty(errorMessage)) SafeMessage(errorMessage);
                return;
            }
            this._window.ModList.SelectedIndex = this.Manager.Mods.IndexOf(it);
        }

        public void Initialize()
        {
            if (!this.Manager.Initialize(Program.Configuration.ConfigPath, Program.Configuration.GamePath, out var error))
            {
                if (!string.IsNullOrEmpty(error)) SafeMessage(error);
            }

            if (Program.Configuration.CheckForUpdates) this.CheckForUpdates();
        }

        private void RunCmd()
        {
            if (!this.Manager.Run(Program.Configuration.GameExeId == 1 ? "Bannerlord.Native.exe" : "Bannerlord.exe", Program.Configuration.ExtraGameArguments, out var error))
            {
                if (!string.IsNullOrEmpty(error)) SafeMessage(error);
                return;
            }

            if (Program.Configuration.CloseWhenRunningGame) Application.Current.Shutdown();
        }

        private void SaveCmd()
        {

            if (!this.Manager.Save(out var error))
            {
                if (!string.IsNullOrEmpty(error)) SafeMessage(error);
            }
            SafeMessage("Saved successfully");
        }

        private void OpenConfigCmd()
        {

            if (this.Manager.OpenConfig(out var error)) return;
            if (!string.IsNullOrEmpty(error)) SafeMessage(error);
        }

        public bool CanInitialize(string configPath, string gamePath)
        {
            return true;
        }

        public bool CanRun(string gameExe, string extraGameArguments)
        {
            if (this.Manager.Mods.Any(x => x.HasConflicts) && Program.Configuration.WarnOnConflict)
            {
                if (MyMessageBox.Show(
                        this._window,
                        "Your mod list has existing conflicts, are you sure that you want to run the game?",
                        "Warning",
                        MessageBoxButton.YesNo
                        ) == MessageBoxResult.No)
                {
                    return false;
                }
            }

            var acutalGameExe = Path.Combine(this.Manager.GameExeFolder, gameExe);
            if (!File.Exists(acutalGameExe))
            {
                this.Log().Error($"{acutalGameExe} could not be found");
                return false;
            }

            try
            {
                if (UACChecker.RequiresElevation(acutalGameExe))
                {
                    if (!UacUtil.IsElevated)
                    {
                        SafeMessage("The application must be run as admin, to allow launching the game");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                this.Log().Error(e);
                SafeMessage(e.Message);
            }

            return true;
        }

        public bool CanSave()
        {
            if (!this.Manager.Mods.Any(x => x.HasConflicts)) return true;
            if (!Program.Configuration.WarnOnConflict) return true;
            return MyMessageBox.Show(
                this._window,
                "Your mod list has existing conflicts, are you sure that you want to save it?",
                "Warning",
                MessageBoxButton.YesNo
            ) != MessageBoxResult.No;
        }

        public bool CanMoveToTop(int idx)
        {
            return idx > 0;
        }

        public bool CanMoveUp(int idx)
        {
            return idx > 0;
        }

        public bool CanMoveDown(int idx)
        {
            return idx >= 0 && idx < this.Manager.Mods.Count - 1;
        }

        public bool CanMoveToBottom(int idx)
        {
            return idx >= 0 && idx < this.Manager.Mods.Count - 1;
        }

        public bool CanCheckAll()
        {
            return this.Manager.Mods.Count > 0;
        }

        public bool CanUncheckAll()
        {
            return this.Manager.Mods.Count > 0;
        }

        public bool CanInvertCheck()
        {
            return this.Manager.Mods.Count > 0;
        }

        public bool CanSort()
        {
            return this.Manager.Mods.Count > 0;
        }
    }
}
