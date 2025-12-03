using System.Collections.ObjectModel;
using KeyKeeper.Models;

namespace KeyKeeper.Services;

public interface IRecentFilesService
{
    // files are stored in reverse chronological order
    ObservableCollection<RecentFile> RecentFiles { get; }
    
    void Remember(string filename);
    void Forget(string filename);
    void ForgetAll();

    // TODO load and store
}