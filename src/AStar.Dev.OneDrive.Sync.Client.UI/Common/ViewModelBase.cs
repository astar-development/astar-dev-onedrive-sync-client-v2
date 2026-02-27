using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Common;

/// <summary>
/// Base class for all ViewModels in the application, providing common functionality such as property change notification. This class inherits from ReactiveObject, which is part of the ReactiveUI framework and allows for reactive programming patterns in the ViewModel layer. By using ReactiveObject, ViewModels can easily implement INotifyPropertyChanged and other reactive features without needing to write boilerplate code.
/// </summary>
public abstract class ViewModelBase : ReactiveObject;

