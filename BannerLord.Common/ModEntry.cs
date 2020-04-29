using System;
using System.Collections.ObjectModel;
using BannerLord.Common.Xml;
using ReactiveUI;

namespace BannerLord.Common
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class ModEntry : ReactiveObject, IEquatable<ModEntry>
    {
        public bool Equals(ModEntry other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ModEntry other && this == other;
        }

        public override int GetHashCode()
        {
            return this._module.Id.ToLowerInvariant().GetHashCode();
        }

        private Module _module;
        private UserModData _userModData;
        private bool _isChecked;
        private int _originalSpot;
        private bool _isPointerOver;
        private ObservableCollection<LoadOrderConflict> _loadOrderConflicts;
        public HashSet<string> MyAssemblies { get; } = new HashSet<string>();
        public HashSet<string> DependOnAssemblies { get; } = new HashSet<string>();
        public HashSet<ModEntry> DependsOn { get; } = new HashSet<ModEntry>();

        public bool IsCheckboxEnabled => this._module.Official == false && this._module.SingleplayerModule == true;

        public Module Module
        {
            get => this._module;
            set => this.RaiseAndSetIfChanged(ref this._module, value);
        }

        public void NotifyChanged(string property)
        {
            this.RaisePropertyChanged(property);
        }

        public UserModData UserModData
        {
            get => this._userModData;
            set
            {
                this.RaiseAndSetIfChanged(ref this._userModData, value);
                this._isChecked = this._userModData.IsSelected;
            }
        }

        public ObservableCollection<LoadOrderConflict> LoadOrderConflicts
        {
            get => this._loadOrderConflicts;
            set => this.RaiseAndSetIfChanged(ref this._loadOrderConflicts, value);
        }

        public bool HasConflicts => this._loadOrderConflicts.Any(x => !x.Optional);

        public string Conflicts
        {
            get
            {
                var ret = string.Empty;
                foreach (var conflict in this._loadOrderConflicts)
                {
                    if (!string.IsNullOrEmpty(ret)) ret += Environment.NewLine;
                    ret += conflict.Optional ? "(Optional)" : "";
                    if (conflict.IsUp) ret += $"{conflict.DependsOn} depends on this";
                    else if (conflict.IsDown) ret += $"This depends on {conflict.DependsOn}";
                    else ret += $"{conflict.DependsOn} is missing";
                }

                return ret;
            }
        }

        public string DisplayName => this.Module.Name;

        public bool IsChecked
        {
            get => this._isChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref this._isChecked, value);
                this._userModData.IsSelected = this._isChecked;
            }
        }

        public bool IsPointerOver
        {
            get => this._isPointerOver;
            set => this.RaiseAndSetIfChanged(ref this._isPointerOver, value);
        }

        public int OriginalSpot
        {
            get => this._originalSpot;
            set => this.RaiseAndSetIfChanged(ref this._originalSpot, value);
        }

        public static bool operator ==(ModEntry a, ModEntry b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            if (ReferenceEquals(b, null)) return false;


            return a.Module.Id.Equals(b.Module.Id,
                    StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(ModEntry a, ModEntry b)
        {
            return !(a == b);
        }
    }
}