using System;
using System.Collections.Generic;
using Cooking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ImageWithText
{
    public Image Image;
    public TextMeshProUGUI Text;
}

public class CookingSlotView : MonoBehaviour
{
    [SerializeField] private int slot;
    [SerializeField] private List<ImageWithText> Inputs;
    [SerializeField] private List<ImageWithText> Outputs;

    private _ViewData viewData;

    public _ViewData ViewData
    {
        get => viewData ??= new _ViewData(); 
        set => viewData = value;
    }

    public int Slot => slot;
    
    private _ViewData LastDrawnViewData;
    private RecipeSelectionView recipeSelectionView;
    
    public class _ViewData
    {
        public CookingRecipe Recipe;
        public CookingManager.CookingSummary CookingProgress;
    }

    public Action<int> OnClaim;
    public Action<int> OnChangeRecipe;

    void Update()
    {
        Draw();
    }

    public void Draw()
    {
        for (var i = 0; i < Inputs.Count; i++)
        {
            if (ViewData == null || ViewData.Recipe == null || i >= ViewData.Recipe.Inputs.Count)
            {
                Inputs[i].Image.gameObject.SetActive(false);
                continue;
            }

            Inputs[i].Image.sprite = ViewData.Recipe.Inputs[i].item.SmallThumbnail;
            Inputs[i].Text.text = ViewData.Recipe.Inputs[i].quantity.ToString();
            Inputs[i].Image.gameObject.SetActive(true);
        }

        for (var i = 0; i < Outputs.Count; i++)
        {
            if (ViewData == null || ViewData.Recipe == null || i >= ViewData.Recipe.Output.Count)
            {
                Outputs[i].Image.gameObject.SetActive(false);
                continue;
            }

            Outputs[i].Image.sprite = ViewData.Recipe.Output[i].item.SmallThumbnail;
            Outputs[i].Text.text = (ViewData.Recipe.Output[i].quantity * ViewData.CookingProgress.TotalCookedItems).ToString();
            Outputs[i].Image.gameObject.SetActive(true);
        }
    }

    public void ClaimButtonPressed()
    {
        OnClaim?.Invoke(slot);
    }

    public void ChangeRecipePressed()
    {
        OnChangeRecipe?.Invoke(slot);
    }
}
