using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ServiceFactoryBindings : Dictionary<Type, Func<Type, object>> { }

public class Services
{
    static ServicesContainer container;
    private object lockObject = new object();
    public static ServicesContainer Container => container;
    public ServiceFactoryBindings factoryBindings;
    public bool Initialized;

    public event Action<PlayerProfile, bool> OnPlayerProfileLoad; 

    public void Initialize(Action<bool> load)
    {
        factoryBindings = new ServiceFactoryBindings();
        RegisterBindings();
        container = new ServicesContainer(factoryBindings);
        LoadAllResources(load);
    }

    public void LoadAllResources(Action<bool> loadComplete)
    {
        HashSet<Type> completed = new HashSet<Type>();
        HashSet<Type> failed = new HashSet<Type>();
        HashSet<Type> loading = new HashSet<Type>();

        void CheckIfFinsihedLoad()
        {
#if DEBUG_INITIALIZATION
            Debug.Log($"Remaining services to load: {loading.Count} ");
#endif
            if (loading.Count == 0)
            {
                Initialized = true;
                loadComplete?.Invoke(failed.Count == 0);
            }
        }
        
        void processServiceLoad<T>(T instance, Action<Action<bool>> loadFunc)
        {
            loading.Add(typeof(T));
#if DEBUG_INITIALIZATION
            Debug.Log("Started loading " + typeof(T));
#endif
            loadFunc?.Invoke(ok =>
                {
                    lock (lockObject)
                    {
#if DEBUG_INITIALIZATION
                        Debug.Log("Finished loading " + typeof(T));
#endif
                        loading.Remove(typeof(T));
                        if (ok)
                            completed.Add(typeof(T));
                        if (!ok)
                            failed.Add(typeof(T));
                        factoryBindings.Add(typeof(T), t => instance);
                        CheckIfFinsihedLoad();
                    }
                });
        }
        
        var itemDatabaseService = new ItemDatabaseService();
        processServiceLoad(itemDatabaseService, itemDatabaseService.Load);

        var inventoryService = new InventoryService();
        inventoryService.OnPlayerLoaded += profile =>
        {
            OnPlayerProfileLoad?.Invoke(profile, false);
        };
        inventoryService.OnNewPlayerCreated += profile =>
        {
            OnPlayerProfileLoad?.Invoke(profile, true);
        };
        processServiceLoad(inventoryService, inventoryService.Load);

        var droptableDatabase = new DroptableDatabaseService();
        processServiceLoad(droptableDatabase, droptableDatabase.Load);
        
        var nodeDatabase = new NodeDatabaseService();
        processServiceLoad(nodeDatabase, nodeDatabase.Load);
        
        var questDatabase = new QuestDatabaseService();
        processServiceLoad(questDatabase, questDatabase.Load);
        
        var contextualHintSerice = new ContextualHintService();
        processServiceLoad(contextualHintSerice, contextualHintSerice.Load);
        
        var skillService = new SkillService();
        processServiceLoad(skillService, skillService.Load);
    }

    private void RegisterBindings()
    {
        factoryBindings.Add(typeof(CoroutineDispatcher), t => CreateMonobehaviour<CoroutineDispatcher>());
        factoryBindings.Add(typeof(DamageProcessorService), t => new DamageProcessorService());
        factoryBindings.Add(typeof(LocalizationService), t => new LocalizationService());
        factoryBindings.Add(typeof(BuffsManagerService), t=> new BuffsManagerService());
        factoryBindings.Add(typeof(DropGeneratorService), t=> new DropGeneratorService());
        factoryBindings.Add(typeof(Wallet), t=> Container.Resolve<InventoryService>().PlayerProfile.Wallet);
        factoryBindings.Add(typeof(OverkillService), t=> new OverkillService());
        factoryBindings.Add(typeof(ConsumableItemResolver), t => new ConsumableItemResolver());
        factoryBindings.Add(typeof(NPCService), t => new NPCService());
    }

    public static T CreateMonobehaviour<T>() where T : Component
    {
        var newGO = new GameObject($"SERVICE_INSTANCE_" + typeof(T).ToString().ToUpper());
        var newT = newGO.AddComponent<T>();
        GameObject.DontDestroyOnLoad(newGO);
        return newT;
    }
}

public class ServicesContainer
{
    private Dictionary<Type, object> instances ;
    public ServiceFactoryBindings bindings;

    public ServicesContainer(ServiceFactoryBindings bindings)
    {
        this.bindings = bindings;
        instances = new Dictionary<Type, object>();
    }

    T Instantiate<T>() where T : class
    {
        if (bindings.ContainsKey(typeof(T)))
        {
            return bindings[typeof(T)].Invoke(typeof(T)) as T;
        }
        Debug.LogError("Service does not exist. Create default object to not crash the game. But this is an error");
        return Activator.CreateInstance<T>();
    }
    
    public T Resolve<T>() where T : class
    {
        if (!instances.ContainsKey(typeof(T)))
        {
            instances.Add(typeof(T), Instantiate<T>());
        }
        return instances[typeof(T)] as T;
    }
}