using System.Collections.ObjectModel;
using AstarOneDrive.UI.Common;
using ReactiveUI;

namespace AstarOneDrive.UI.FolderTrees;

public class FolderTreeViewModel : ViewModelBase
{
    public ObservableCollection<FolderNodeViewModel> FolderTree { get; } = new();

    public FolderTreeViewModel()
    {
        // Placeholder sample data
        FolderTree.Add(new FolderNodeViewModel("Documents")
        {
            Children =
            {
                new FolderNodeViewModel("Projects"),
                new FolderNodeViewModel("Invoices")
            }
        });

        FolderTree.Add(new FolderNodeViewModel("Pictures"));
    }
}

public class FolderNodeViewModel : ViewModelBase
{
    public string Name { get; }
    public bool IsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ObservableCollection<FolderNodeViewModel> Children { get; } = new();

    public FolderNodeViewModel(string name)
    {
        Name = name;
    }
}
