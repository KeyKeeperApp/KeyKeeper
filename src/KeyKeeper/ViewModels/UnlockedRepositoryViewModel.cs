using System;
using System.Collections.Generic;
using System.Linq;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.ViewModels;

public class UnlockedRepositoryViewModel : ViewModelBase
{
    private IPassStore passStore;
    private IPassStoreDirectory currentDirectory;
    private bool hasUnsavedChanges;

    public IEnumerable<PassStoreEntryPassword> Passwords
    {
        get
        {
            return currentDirectory
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

    public UnlockedRepositoryViewModel(IPassStore store, IPassStoreDirectory directory)
    {
        passStore = store;
        currentDirectory = directory;
        HasUnsavedChanges = false;
    }

    public void AddEntry(PassStoreEntry entry)
    {
        if (entry is PassStoreEntryPassword)
        {
            currentDirectory.AddEntry(entry);
            HasUnsavedChanges = true;
            OnPropertyChanged(nameof(Passwords));
        }
    }

    public void DeleteEntry(Guid id)
    {
        currentDirectory.DeleteEntry(id);
        HasUnsavedChanges = true;
        OnPropertyChanged(nameof(Passwords));
    }

    public void UpdateEntry(PassStoreEntryPassword updatedEntry)
    {
        currentDirectory.DeleteEntry(updatedEntry.Id);
        currentDirectory.AddEntry(updatedEntry);
        OnPropertyChanged(nameof(Passwords));
    }

    public void Save()
    {
        passStore.Save();
        HasUnsavedChanges = false;
    }
}
