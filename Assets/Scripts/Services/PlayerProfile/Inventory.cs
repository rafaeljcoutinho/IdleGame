using System;
using System.IO;
using UnityEngine;

public class InventoryService
{
    private string FilePath => Path.Combine(Application.persistentDataPath, "wallet");
    private PlayerProfile profile;
    private ILocalPersistencyLayer persistencyLayer;

    private bool isSaving;
    private bool shouldSave;

    public PlayerProfile PlayerProfile => profile;

    public event Action<PlayerProfile> OnNewPlayerCreated;
    public event Action<PlayerProfile> OnPlayerLoaded;
    
    public InventoryService()
    {
        profile = new PlayerProfile();
        persistencyLayer = new LocalPersistencyLayer(FilePath);
    }

    public void Reset() {
        profile = PlayerProfile.Default;
        profile.OnAfterLoad();
        Save();
    }

    public void Load(Action<bool> done)
    {
        persistencyLayer.Load( (profile, loaded) =>
        {
            Debug.Log("On load called");
            this.profile = profile;
            if (profile == null)
            {
                Debug.Log("Profile is null");
                // This is the case where the player local save is not valid anymore
                this.profile = PlayerProfile.Default;
                OnNewPlayerCreated?.Invoke(this.profile);
                done?.Invoke(false);
                return;
            }

            if (loaded)
            {
                OnPlayerLoaded?.Invoke(this.profile);
            }
            else
            {
                OnNewPlayerCreated?.Invoke(this.profile);
            }
            done?.Invoke(true);
        }, PlayerProfile.Default);
    }

    public void Save()
    {
        shouldSave = true;
    }

    public void OnLateUpdate()
    {
        if (shouldSave)
        {
            DoSave();
        }
    }
    
    private void DoSave()
    {
        if (isSaving)
        {
            return;
        }
        shouldSave = false;
        persistencyLayer.Save(profile, ok =>
        {
            isSaving = false;
            shouldSave |= !ok;
        });
    }
}