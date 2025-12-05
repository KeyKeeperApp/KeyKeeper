using System;
using System.Collections.Generic;
using System.Linq;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.ViewModels;

public class UnlockedRepositoryViewModel : ViewModelBase
{
    private IPassStore passStore;

    public IEnumerable<PassStoreEntryPassword> Passwords
    {
        get
        {
            return passStore.GetRootDirectory()
                .Where(entry => entry is PassStoreEntryPassword)
                .Select(entry => (entry as PassStoreEntryPassword)!);
        }
    }

    public UnlockedRepositoryViewModel(IPassStore store)
    {
        passStore = store;
    }

    public void AddEntry(PassStoreEntry entry)
    {
        if (entry is PassStoreEntryPassword)
        {
            (passStore.GetRootDirectory() as PassStoreEntryGroup)!.ChildEntries.Add(entry);
        }
    }
}