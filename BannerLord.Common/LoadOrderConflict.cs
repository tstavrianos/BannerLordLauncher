using ReactiveUI;

namespace BannerLord.Common
{
    public sealed class LoadOrderConflict : ReactiveObject
    {
        private string _dependsOnId;

        public string DependsOnId
        {
            get => this._dependsOnId;
            set => this.RaiseAndSetIfChanged(ref this._dependsOnId, value);
        }

        private string _dependsOnName;

        public string DependsOnName
        {
            get => this._dependsOnName;
            set => this.RaiseAndSetIfChanged(ref this._dependsOnName, value);
        }

        public string DependsOn => this.DependsOnName ?? this.DependsOnId;

        private bool _isUp;

        public bool IsUp
        {
            get => this._isUp;
            set => this.RaiseAndSetIfChanged(ref this._isUp, value);
        }

        private bool _isDown;

        public bool IsDown
        {
            get => this._isDown;
            set => this.RaiseAndSetIfChanged(ref this._isDown, value);
        }

        private bool _isMissing;

        public bool IsMissing
        {
            get => this._isMissing;
            set => this.RaiseAndSetIfChanged(ref this._isMissing, value);
        }
    }
}
