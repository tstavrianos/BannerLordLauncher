using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using BannerLord.Common;
using BannerLordLauncher.Views;
using ReactiveUI;
using Steam.Common;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using System.Linq;
using Splat;
using Octokit;
using Application = System.Windows.Application;
using System.Windows.Threading;

namespace BannerLordLauncher.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase, IDropTarget, IModManagerClient
    {
        public ModManager Manager { get; }
        private readonly MainWindow _window;
        private int _selectedIndex = -1;

        private bool _ignoredWarning;

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
        //public ICommand AlphaSort { get; }
        //public ICommand ReverseOrder { get; }
        public ICommand Sort { get; }
        public ICommand MoveToTop { get; }
        public ICommand MoveUp { get; }
        public ICommand MoveDown { get; }
        public ICommand MoveToBottom { get; }
        public ICommand CheckAll { get; }
        public ICommand UncheckAll { get; }
        public ICommand InvertCheck { get; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore MemberCanBePrivate.Global

        private readonly GitHubClient _github;

        public MainWindowViewModel(MainWindow window)
        {
            this._github = new GitHubClient(new ProductHeaderValue("BannerLordLauncher"));

            this._ignoredWarning = true;
            this._window = window;
            this.Manager = new ModManager(this);

            var moveUp = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x > 0);
            var moveDown = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x >= 0 && x < this.Manager.Mods.Count - 1);

            this.Save = ReactiveCommand.Create(this.SaveCmd);
            //this.AlphaSort = ReactiveCommand.Create(() => this.Manager.AlphaSort());
            //this.ReverseOrder = ReactiveCommand.Create(() => this.Manager.ReverseOrder());
            this.Sort = ReactiveCommand.Create(this.SortCmd);
            this.MoveToTop = ReactiveCommand.Create(this.MoveToTopCmd, moveUp.Select(x => x));
            this.MoveUp = ReactiveCommand.Create(this.MoveUpCmd, moveUp.Select(x => x));
            this.MoveDown = ReactiveCommand.Create(this.MoveDownCmd, moveDown.Select(x => x));
            this.MoveToBottom = ReactiveCommand.Create(this.MoveToBottomCmd, moveDown.Select(x => x));
            this.CheckAll = ReactiveCommand.Create(this.CheckAllCmd);
            this.UncheckAll = ReactiveCommand.Create(this.UncheckAllCmd);
            this.InvertCheck = ReactiveCommand.Create(this.InvertCheckCmd);
            this.Run = ReactiveCommand.Create(this.RunCmd);
            this.Config = ReactiveCommand.Create(() => this.Manager.OpenConfig());
        }

        private void SafeMessage(string message)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                MessageBox.Show(this._window, message);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        MessageBox.Show(this._window, message);
                    }));
            }
        }

        private async void CheckForUpdates()
        {
            var currentVersion = typeof(MainWindowViewModel).Assembly.GetName().Version;
            var result = await this._github.Repository.Release.GetAll("tstavrianos", "BannerLordLauncher").ConfigureAwait(false);
            var latestRelease = result.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
            if (latestRelease == null) return;
            var latestReleaseVersion = new Version(latestRelease.TagName);
            if (latestReleaseVersion <= currentVersion) return;
            var message = $"Version {latestReleaseVersion} is available to download";
            this.SafeMessage(message);
        }

        private void CheckAllCmd()
        {
            if (this.Manager.CheckAll(out var error)) return;
            if (!string.IsNullOrEmpty(error)) this.SafeMessage(error);
        }
        private void UncheckAllCmd()
        {
            if (this.Manager.UncheckAll(out var error)) return;
            if (!string.IsNullOrEmpty(error)) this.SafeMessage(error);
        }
        private void InvertCheckCmd()
        {
            if (this.Manager.InvertCheck(out var error)) return;
            if (!string.IsNullOrEmpty(error)) this.SafeMessage(error);
        }

        private void SortCmd()
        {
            var idx = this._window.ModList.SelectedIndex;
            ModEntry it = null;
            if (idx != -1)
                it = this.Manager.Mods[idx];
            this._window.ModList.SelectedIndex = -1;
            if (!this.Manager.Sort(out var errorMessage))
            {
                this._window.ModList.SelectedIndex = idx;
                if (!string.IsNullOrEmpty(errorMessage)) this.SafeMessage(errorMessage);
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
                if (!string.IsNullOrEmpty(errorMessage)) this.SafeMessage(errorMessage);
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
                if (!string.IsNullOrEmpty(errorMessage)) this.SafeMessage(errorMessage);
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
                if (!string.IsNullOrEmpty(errorMessage)) this.SafeMessage(errorMessage);
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
                if (!string.IsNullOrEmpty(errorMessage)) this.SafeMessage(errorMessage);
                return;
            }
            this._window.ModList.SelectedIndex = this.Manager.Mods.IndexOf(it);
        }

        public void Initialize()
        {
            var config = string.Empty;
            if (!string.IsNullOrEmpty(this._window.Configuration.ConfigPath) && Directory.Exists(this._window.Configuration.ConfigPath))
            {
                config = this._window.Configuration.ConfigPath;
            }
            else
            {
                try
                {
                    var basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    if (Directory.Exists(basePath))
                    {
                        basePath = Path.Combine(basePath, "Mount and Blade II Bannerlord");
                        if (Directory.Exists(basePath))
                        {
                            basePath = Path.Combine(basePath, "Configs");
                            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
                            config = basePath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex);
                    config = null;
                }
            }

            if (string.IsNullOrEmpty(config))
            {
                config = this.FindConfigFolder();
            }

            this._window.Configuration.ConfigPath = config;


            var game = string.Empty;

            if (!string.IsNullOrEmpty(this._window.Configuration.GamePath) &&
                Directory.Exists(this._window.Configuration.GamePath) && File.Exists(Path.Combine(this._window.Configuration.GamePath, "bin", "Win64_Shipping_Client", "Bannerlord.exe")))
            {
                game = this._window.Configuration.GamePath;
            }
            else
            {
                var steamFinder = new SteamFinder();
                if (steamFinder.FindSteam())
                {
                    game = steamFinder.FindGameFolder(261550);
                    if (string.IsNullOrEmpty(game) || !Directory.Exists(game))
                    {
                        game = null;
                    }
                }

                if (string.IsNullOrEmpty(game))
                {
                    game = this.FindGameFolder();
                }

                this._window.Configuration.GamePath = game;
            }

            if (!this.Manager.Initialize(config, game, out var error))
            {
                if (!string.IsNullOrEmpty(error)) this.SafeMessage(error);
            }

            this.CheckForUpdates();
        }

        private string FindGameFolder()
        {
            while (true)
            {
                var dialog = new VistaFolderBrowserDialog
                {
                    Description = "Select game root folder",
                    UseDescriptionForTitle = true
                };
                var result = dialog.ShowDialog(this._window);
                if (result is null) Environment.Exit(0);
                if (!Directory.Exists(dialog.SelectedPath) || !File.Exists(Path.Combine(dialog.SelectedPath, "bin", "Win64_Shipping_Client", "Bannerlord.exe"))) continue;
                return dialog.SelectedPath;
            }
        }

        private string FindConfigFolder()
        {
            while (true)
            {
                var dialog = new VistaFolderBrowserDialog
                {
                    Description = "Select game config folder, in documents",
                    UseDescriptionForTitle = true
                };
                var result = dialog.ShowDialog(this._window);
                if (result is null) Environment.Exit(0);
                if (!Directory.Exists(dialog.SelectedPath)) continue;
                return dialog.SelectedPath;
            }
        }

        private void RunCmd()
        {
            if (!this.Manager.Run(out var error))
            {
                if (!string.IsNullOrEmpty(error)) this.SafeMessage(error);
                return;
            }

            Application.Current.Shutdown();
        }

        private void SaveCmd()
        {

            if (this.Manager.Save(out var error)) return;
            if (!string.IsNullOrEmpty(error)) this.SafeMessage(error);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as ModEntry;
            var targetItem = dropInfo.TargetItem as ModEntry;

            if (sourceItem == null || targetItem == null) return;
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo?.DragInfo == null)
            {
                return;
            }

            var insertIndex = dropInfo.UnfilteredInsertIndex;
            if (dropInfo.VisualTarget is ItemsControl itemsControl)
            {
                if (itemsControl.Items is IEditableCollectionView editableItems)
                {
                    var newItemPlaceholderPosition = editableItems.NewItemPlaceholderPosition;
                    switch (newItemPlaceholderPosition)
                    {
                        case NewItemPlaceholderPosition.AtBeginning when insertIndex == 0:
                            ++insertIndex;
                            break;
                        case NewItemPlaceholderPosition.AtEnd when insertIndex == itemsControl.Items.Count:
                            --insertIndex;
                            break;
                    }
                }
            }
            var sourceItem = dropInfo.Data as ModEntry;
            var index = this.Manager.Mods.IndexOf(sourceItem);
            if (index < insertIndex) insertIndex--;
            this._window.ModList.SelectedIndex = -1;
            this.Manager.Mods.Remove(sourceItem);
            this.Manager.Mods.Insert(insertIndex, sourceItem);
            this._window.ModList.SelectedIndex = this.Manager.Mods.IndexOf(sourceItem);
            this.Manager.Validate();
        }

        public bool CanInitialize(string configPath, string gamePath)
        {
            return true;
        }

        public bool CanRun()
        {
            if (this._ignoredWarning && this.Manager.Mods.Any(x => x.HasConflicts))
            {
                if (MessageBox.Show(
                        this._window,
                        "Your mod list has existing conflicts, are you sure that you want to run the game?",
                        "Warning",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return false;
                }
            }
            if (!File.Exists(this.Manager.GameExe))
            {
                this.Log().Error($"{this.Manager.GameExe} could not be found");
                return false;
            }

            if (UACChecker.RequiresElevation(this.Manager.GameExe))
            {
                if (!UacUtil.IsElevated)
                {
                    this.SafeMessage("The application must be run as admin, to allow launching the game");
                    return false;
                }
            }

            return true;
        }

        public bool CanSave()
        {
            this._ignoredWarning = false;
            if (this.Manager.Mods.Any(x => x.HasConflicts))
            {
                if (MessageBox.Show(
                        this._window,
                        "Your mod list has existing conflicts, are you sure that you want to save it?",
                        "Warning",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return false;
                }

                this._ignoredWarning = true;
            }

            return true;
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
