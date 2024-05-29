using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    public class CookingPopupView : MonoBehaviour
    {
        [SerializeField] private List<CookingSlotView> slots;
        [SerializeField] private RecipeSelectionView recipeSelectionView;
        
        public void Setup()
        {
            var cookingManager = Services.Container.Resolve<InventoryService>().PlayerProfile.CookingManager;
            for (var i = 0; i < slots.Count; i++)
            {
                slots[i].OnChangeRecipe = OnOnChangeRecipe;
                if (cookingManager.CookingRecords == null || cookingManager.CookingRecords.Count <= i || cookingManager.CookingRecords[i] == null)
                {
                    slots[i].ViewData = null;
                    slots[i].Draw();
                    slots[i].OnClaim = null;
                    continue;
                }
                var record = cookingManager.CookingRecords[i];
                var recipe = Services.Container.Resolve<NodeDatabaseService>().NodeDatabase.GetNode(record.recipeId) as CookingRecipe;
                slots[i].ViewData.Recipe = recipe;
                slots[i].ViewData.CookingProgress = cookingManager.GetCookingSummary(i);
                slots[i].OnClaim = OnClaim;
                slots[i].Draw();
            }
        }

        private void OnClaim(int slot)
        {
            var outs = Services.Container.Resolve<InventoryService>().PlayerProfile.CookingManager.Collect(slot);
            Setup();
            if (outs != null && outs.Count > 0)
                OverlayCanvas.Instance.ShowDrops(outs, true, true);
        }

        private void OnOnChangeRecipe(int slot)
        {
            recipeSelectionView.Show(recipe =>
            {
                var cookingManager = Services.Container.Resolve<InventoryService>().PlayerProfile.CookingManager; 
                cookingManager.SetRecipe(recipe.Uuid, slot);
                Setup();
            });
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}