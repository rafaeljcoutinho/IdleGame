using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    public class RecipeSelectionItemView : MonoBehaviour
    {
        [SerializeField] private List<ImageWithText> Inputs;
        [SerializeField] private List<ImageWithText> Outputs;
        private CookingRecipe recipe;
        
        public Action<CookingRecipe> OnClick;
        
        public void SetRecipe(CookingRecipe recipe)
        {
            this.recipe = recipe;
            for (var i = 0; i < Inputs.Count; i++)
            {
                if (i >= recipe.Inputs.Count)
                {
                    Inputs[i].Image.gameObject.SetActive(false);
                    continue;
                }

                Inputs[i].Image.sprite = recipe.Inputs[i].item.SmallThumbnail;
                Inputs[i].Text.text = recipe.Inputs[i].quantity.ToString();
                Inputs[i].Image.gameObject.SetActive(true);
            }

            for (var i = 0; i < Outputs.Count; i++)
            {
                if (i >= recipe.Output.Count)
                {
                    Outputs[i].Image.gameObject.SetActive(false);
                    continue;
                }

                Outputs[i].Image.sprite = recipe.Output[i].item.SmallThumbnail;
                Outputs[i].Text.text = recipe.Output[i].quantity.ToString();
                Outputs[i].Image.gameObject.SetActive(true);
            }
        }

        public void ButtonClicked()
        {
            OnClick?.Invoke(recipe);
        }
    }
}