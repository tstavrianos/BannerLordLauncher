using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using BannerLord.Common;
using BannerLordLauncher.Views;
using ReactiveUI;
using Steam.Common;

namespace BannerLordLauncher.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        public ModManager Manager { get; }
        private readonly MainWindow _window;
        private readonly ListBox _modList;
        private ListBoxItem _dragItem;
        private int _selectedIndex = -1;
        private bool _validate;
        private bool _backup;

        public int SelectedIndex
        {
            get => this._selectedIndex;
            set => this.RaiseAndSetIfChanged(ref this._selectedIndex, value);
        }

        public bool Validate
        {
            get => this._validate;
            set
            {
                this.RaiseAndSetIfChanged(ref this._validate, value);
                this.Manager.AutomaticValidation = value;
                this._window.Configuration.AutoValidate = value;
            }
        }
        
        public bool Backup
        {
            get => this._backup;
            set
            {
                this.RaiseAndSetIfChanged(ref this._backup, value);
                this.Manager.AutoBackup = value;
                this._window.Configuration.AutoBackup = value;
            }
        }

        public MainWindowViewModel(MainWindow window)
        {
            this._window = window;
            this._modList = this._window.Find<ListBox>("ModList");
            this.Manager = new ModManager();
            this.Validate = this._window.Configuration.AutoValidate;
            this.Backup = this._window.Configuration.AutoBackup;

            var moveUp = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x > 0);
            var moveDown = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x >= 0 && x < this.Manager.Mods.Count - 1);
            var validate = this.WhenAnyValue(x => x.Validate).Select(x => !x);

            this.Save = ReactiveCommand.Create(() => this.Manager.Save());
            this.AlphaSort = ReactiveCommand.Create(() => this.Manager.AlphaSort());
            this.ReverseOrder = ReactiveCommand.Create(() => this.Manager.ReverseOrder());
            this.ExperimentalSort = ReactiveCommand.Create(() => this.Manager.TopologicalSort());
            this.MoveToTop = ReactiveCommand.Create(() => this.Manager.MoveToTop(this.SelectedIndex), moveUp.Select(x => x));
            this.MoveUp = ReactiveCommand.Create(() => this.Manager.MoveUp(this.SelectedIndex), moveUp.Select(x => x));
            this.MoveDown = ReactiveCommand.Create(() => this.Manager.MoveDown(this.SelectedIndex), moveDown.Select(x => x));
            this.MoveToBottom = ReactiveCommand.Create(() => this.Manager.MoveToBottom(this.SelectedIndex), moveDown.Select(x => x));
            this.CheckAll = ReactiveCommand.Create(() => this.Manager.CheckAll());
            this.UncheckAll = ReactiveCommand.Create(() => this.Manager.UncheckAll());
            this.InvertCheck = ReactiveCommand.Create(() => this.Manager.InvertCheck());
            this.ValidateCommand = ReactiveCommand.Create(() => this.Manager.Validate(), validate.Select(x => x));
            this.Run = ReactiveCommand.Create(() => this.Manager.RunGame());
        }

        public async void Initialize()
        {
            var game = string.Empty;

            if (!string.IsNullOrEmpty(this._window.Configuration.GamePath) &&
                Directory.Exists(this._window.Configuration.GamePath))
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
                    game = await this.FindGameFolder();
                }

                this._window.Configuration.GamePath = game;
            }

            this.Manager.Initialize(game);
        }

        private async Task<string> FindGameFolder()
        {
            while (true)
            {
                var dialog = new OpenFolderDialog {Title = "Select game folder",};
                var result = await dialog.ShowAsync(this._window);
                if (result is null || !Directory.Exists(result) || !File.Exists(Path.Combine(result, "bin", "Win64_Shipping_Client", "Bannerlord.exe"))) continue;
                return result;
            }
        }

        public void StartDrag(object sender, PointerPressedEventArgs e) =>
            this._dragItem = this._modList.GetLogicalChildren().Cast<ListBoxItem>().Single(x => x.IsPointerOver);

        public void DoDrag(object sender, PointerEventArgs e)
        {
            if (this._dragItem == null) return;
            var list = this._modList.GetLogicalChildren().ToList();

            var hoveredItem = (ListBoxItem)list.FirstOrDefault(x => this._window.GetVisualsAt(e.GetPosition(this._window)).Contains(((IVisual)x).GetVisualChildren().First()));
            var dragItemIndex = list.IndexOf(this._dragItem);
            var hoveredItemIndex = list.IndexOf(hoveredItem);

            this.ClearDropStyling();
            if (!Equals(hoveredItem, this._dragItem)) hoveredItem?.Classes.Add(dragItemIndex > hoveredItemIndex ? "BlackTop" : "BlackBottom");
        }

        public void EndDrag(object sender, PointerReleasedEventArgs e)
        {
            var hoveredItem = (ListBoxItem)this._modList.GetLogicalChildren().FirstOrDefault(x => this._window.GetVisualsAt(e.GetPosition(this._window)).Contains(((IVisual)x).GetVisualChildren().First()));
            if (this._dragItem != null && hoveredItem != null && !Equals(this._dragItem, hoveredItem))
            {
                var a = this._dragItem.DataContext as ModEntry;
                var b = hoveredItem.DataContext as ModEntry;
                this.Manager.Mods.Move(this.Manager.Mods.IndexOf(a),
                    this.Manager.Mods.IndexOf(b));
                if (this.Validate) this.Manager.Validate();

            }

            this.ClearDropStyling();
            this._dragItem = null;
        }

        private void ClearDropStyling()
        {
            foreach (var item in this._modList.GetLogicalChildren().Cast<ListBoxItem>())
            {
                item.Classes.RemoveAll(new[] { "BlackTop", "BlackBottom" });
            }
        }
        
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public ICommand Run { get; }
        public ICommand Save { get; }
        public ICommand AlphaSort { get; }
        public ICommand ReverseOrder { get; }
        public ICommand ExperimentalSort { get; }
        public ICommand MoveToTop { get; }
        public ICommand MoveUp { get; }
        public ICommand MoveDown { get; }
        public ICommand MoveToBottom { get; }
        public ICommand CheckAll { get; }
        public ICommand UncheckAll { get; }
        public ICommand InvertCheck { get; }
        public ICommand ValidateCommand { get; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore MemberCanBePrivate.Global
    }
}
