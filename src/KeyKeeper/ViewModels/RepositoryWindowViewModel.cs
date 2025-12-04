namespace KeyKeeper.ViewModels;

public class RepositoryWindowViewModel : ViewModelBase
{
    private object currentPage = new LockedRepositoryViewModel();
    public object CurrentPage
    {
        get => currentPage;
        set { currentPage = value; OnPropertyChanged(nameof(CurrentPage)); }
    }
}