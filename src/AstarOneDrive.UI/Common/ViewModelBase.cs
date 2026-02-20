using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace AstarOneDrive.UI.Common;

public abstract class ViewModelBase : INotifyPropertyChanged { public event PropertyChangedEventHandler? PropertyChanged; protected void RaisePropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
