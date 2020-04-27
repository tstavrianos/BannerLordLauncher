using ReactiveUI;

namespace BannerLord.Common
{
    public sealed class LoadOrderConflict : ReactiveObject
    {
        private string _dependsOn;

        public string DependsOn
        {
            get => this._dependsOn;
            set => this.RaiseAndSetIfChanged(ref this._dependsOn, value);
        }

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

        private bool _optional;

        public bool Optional
        {
            get => this._optional;
            set => this.RaiseAndSetIfChanged(ref this._optional, value);
        }
    }
}
