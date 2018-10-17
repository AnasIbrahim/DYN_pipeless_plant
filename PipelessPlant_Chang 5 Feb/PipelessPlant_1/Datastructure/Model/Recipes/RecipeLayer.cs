using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Recipes
{
    public class RecipeLayer
    {
        private String colorName;
        public String theColorName
        {
            get { return colorName; }
            set { colorName = value; }
        }
        private List<Ingredient> ingredients;
        public List<Ingredient> theIngredients
        {
            get { return ingredients; }
            set { ingredients = value; }
        }

        public RecipeLayer(String colorName)
        {
            this.colorName = colorName;
            ingredients = new List<Ingredient>();
        }

        public int getMixTimeLayer()
        {
            double mixTime = 0;
            for (int i = 0; i < ingredients.Count; i++)
            {
                mixTime += ingredients[i].theMixTime;
            }
            return Convert.ToInt32(mixTime);
        }
        public int getMixFillTimeLayer()
        {
            double mixFillTime = 0;
            for (int i = 0; i < ingredients.Count; i++)
            {
                mixFillTime += ingredients[i].theMixFillTime;
            }
            return Convert.ToInt32(mixFillTime);
        }

        public void addIngredient(int colorID, double fillTime, double mixTime, double mixFillTime, String name, double volume)
        {
            if (ingredients == null)
            {
                ingredients = new List<Ingredient>();
            }
            ingredients.Add(new Ingredient(colorID, fillTime, mixTime, mixFillTime, name, volume));
        }

    }
}
