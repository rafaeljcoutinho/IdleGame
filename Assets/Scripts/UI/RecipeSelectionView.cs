using System;
using Cooking;
using UnityEngine;

public class RecipeSelectionView : MonoBehaviour
{
    [SerializeField] private RecipeSelectionItemView recipeSelectionItemViewPrefab;
    [SerializeField] private Transform container;
    private bool initialized;

    private Action<CookingRecipe> callback;

    public void Show(Action<CookingRecipe> callback)
    {
        this.callback = callback;
        if (!initialized)
        {
            Initialize();
        }
        gameObject.SetActive(true);
    }

    public void Close()
    {
        callback = null;
        gameObject.SetActive(false);
    }

    private void Initialize()
    {
        foreach (var nodeData in Services.Container.Resolve<NodeDatabaseService>().NodeDatabase.AllNodes)
        {
            if (nodeData is CookingRecipe recipe)
            {
                var instance = Instantiate(recipeSelectionItemViewPrefab, container);
                instance.OnClick += OnClick;
                instance.SetRecipe(recipe);
            }
        }

        initialized = true;
    }

    private void OnClick(CookingRecipe obj)
    {
        callback?.Invoke(obj);
        Close();
    }
}
