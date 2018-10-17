using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Recipes
{
    public class Recipe
    {
        private int id;
        public int theId
        {
            get { return id; }
            set { id = value; }
        }
        private String name;
        public String theName
        {
            get { return name; }
            set { name = value; }
        }
        private String description;
        public String theDescription
        {
            get { return description; }
            set { description = value; }
        }
        private List<RecipeLayer> layers;
        public List<RecipeLayer> theLayers
        {
            get { return layers; }
            set { layers = value; }
        }

      
        public Recipe(int id, String name, String description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            layers = new List<RecipeLayer>();
        }

        public void addLayer(String name)
        {
            if (layers == null)
            {
                layers = new List<RecipeLayer>();
            }
            layers.Add(new RecipeLayer(name));
        }

        public void addIngredient(int layer, int colorID, double fillTime, double mixTime, double mixFillTime, String name, double volume)
        {
            if (layer < layers.Count)
            {
                layers[layer].addIngredient(colorID, fillTime, mixTime, mixFillTime, name, volume);
            }
        }

    }
}
