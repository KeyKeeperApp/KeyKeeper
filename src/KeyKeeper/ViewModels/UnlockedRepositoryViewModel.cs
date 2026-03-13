using System;
using System.Collections.Generic;
using System.Linq;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.ViewModels;

public class UnlockedRepositoryViewModel : ViewModelBase
{
    private IPassStore passStore;
    private bool hasUnsavedChanges;

    public IEnumerable<PassStoreEntryPassword> Passwords
    {
        get
        {
            return passStore.GetRootDirectory()
                .Where(entry => entry is PassStoreEntryPassword)
                .Select(entry => (entry as PassStoreEntryPassword)!);
        }
    }

    public bool HasUnsavedChanges
    {
        get => hasUnsavedChanges;
        private set
        {
            hasUnsavedChanges = value;
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
    }

    public UnlockedRepositoryViewModel(IPassStore store)
    {
        passStore = store;
        HasUnsavedChanges = false;
    }

    public void AddEntry(PassStoreEntry entry)
    {
        if (entry is PassStoreEntryPassword)
        {
            (passStore.GetRootDirectory() as PassStoreEntryGroup)!.ChildEntries.Add(entry);
            HasUnsavedChanges = true;
            OnPropertyChanged(nameof(Passwords));
        }
    }

    public void DeleteEntry(Guid id)
    {
        (passStore.GetRootDirectory() as PassStoreEntryGroup)!.DeleteEntry(id);
        HasUnsavedChanges = true;
        OnPropertyChanged(nameof(Passwords));
    }

    public void UpdateEntry(PassStoreEntryPassword updatedEntry)
    {
        var root = passStore.GetRootDirectory() as PassStoreEntryGroup;
        if (root == null) return;

        root.DeleteEntry(updatedEntry.Id);
        root.ChildEntries.Add(updatedEntry);
        OnPropertyChanged(nameof(Passwords));
    }

    public void Save()
    {
        passStore.Save();
        HasUnsavedChanges = false;
    }
}