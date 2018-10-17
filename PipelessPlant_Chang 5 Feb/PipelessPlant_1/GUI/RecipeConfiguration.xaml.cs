using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MULTIFORM_PCS.GUI
{
    /// <summary>
    /// Interaktionslogik für AddRecipe.xaml
    /// </summary>
    public partial class RecipeConfiguration : Window
    {
        private int selectedRecipe;

        public RecipeConfiguration()
        {
            InitializeComponent();

            selectedRecipe = 0;

            updateUI();
        }

        private void updateUI()
        {
            Datastructure.Model.Recipes.RecipeData d = Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData;

            if (selectedRecipe == 0)
            {
                borderRecipe1.Opacity = 1;
                borderRecipe2.Opacity = 0.6;
                borderRecipe3.Opacity = 0.6;
                borderRecipe4.Opacity = 0.6;
                borderRecipe5.Opacity = 0.6;
                borderRecipe6.Opacity = 0.6;
            }
            else if (selectedRecipe == 1)
            {
                borderRecipe1.Opacity = 0.6;
                borderRecipe2.Opacity = 1;
                borderRecipe3.Opacity = 0.6;
                borderRecipe4.Opacity = 0.6;
                borderRecipe5.Opacity = 0.6;
                borderRecipe6.Opacity = 0.6;
            }
            else if (selectedRecipe == 2)
            {
                borderRecipe1.Opacity = 0.6;
                borderRecipe2.Opacity = 0.6;
                borderRecipe3.Opacity = 1;
                borderRecipe4.Opacity = 0.6;
                borderRecipe5.Opacity = 0.6;
                borderRecipe6.Opacity = 0.6;
            }
            else if (selectedRecipe == 3)
            {
                borderRecipe1.Opacity = 0.6;
                borderRecipe2.Opacity = 0.6;
                borderRecipe3.Opacity = 0.6;
                borderRecipe4.Opacity = 1;
                borderRecipe5.Opacity = 0.6;
                borderRecipe6.Opacity = 0.6;
            }
            else if (selectedRecipe == 4)
            {
                borderRecipe1.Opacity = 0.6;
                borderRecipe2.Opacity = 0.6;
                borderRecipe3.Opacity = 0.6;
                borderRecipe4.Opacity = 0.6;
                borderRecipe5.Opacity = 1;
                borderRecipe6.Opacity = 0.6;
            }
            else if (selectedRecipe == 5)
            {
                borderRecipe1.Opacity = 0.6;
                borderRecipe2.Opacity = 0.6;
                borderRecipe3.Opacity = 0.6;
                borderRecipe4.Opacity = 0.6;
                borderRecipe5.Opacity = 0.6;
                borderRecipe6.Opacity = 1;
            }

            //Bottom layer
            if (d.selectedRecipes[selectedRecipe][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
            {
                borderBottomYellow.Opacity = 1;
                borderBottomBlack.Opacity = 0.6;
                borderBottomRed.Opacity = 0.6;
                borderBottomBlue.Opacity = 0.6;
                borderBottomPurple.Opacity = 0.6;
                borderBottomOrange.Opacity = 0.6;
                borderBottomGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK)
            {
                borderBottomYellow.Opacity = 0.6;
                borderBottomBlack.Opacity = 1;
                borderBottomRed.Opacity = 0.6;
                borderBottomBlue.Opacity = 0.6;
                borderBottomPurple.Opacity = 0.6;
                borderBottomOrange.Opacity = 0.6;
                borderBottomGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED)
            {
                borderBottomYellow.Opacity = 0.6;
                borderBottomBlack.Opacity = 0.6;
                borderBottomRed.Opacity = 1;
                borderBottomBlue.Opacity = 0.6;
                borderBottomPurple.Opacity = 0.6;
                borderBottomOrange.Opacity = 0.6;
                borderBottomGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE)
            {
                borderBottomYellow.Opacity = 0.6;
                borderBottomBlack.Opacity = 0.6;
                borderBottomRed.Opacity = 0.6;
                borderBottomBlue.Opacity = 1;
                borderBottomPurple.Opacity = 0.6;
                borderBottomOrange.Opacity = 0.6;
                borderBottomGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
            {
                borderBottomYellow.Opacity = 0.6;
                borderBottomBlack.Opacity = 0.6;
                borderBottomRed.Opacity = 0.6;
                borderBottomBlue.Opacity = 0.6;
                borderBottomPurple.Opacity = 1;
                borderBottomOrange.Opacity = 0.6;
                borderBottomGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
            {
                borderBottomYellow.Opacity = 0.6;
                borderBottomBlack.Opacity = 0.6;
                borderBottomRed.Opacity = 0.6;
                borderBottomBlue.Opacity = 0.6;
                borderBottomPurple.Opacity = 0.6;
                borderBottomOrange.Opacity = 1;
                borderBottomGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
            {
                borderBottomYellow.Opacity = 0.6;
                borderBottomBlack.Opacity = 0.6;
                borderBottomRed.Opacity = 0.6;
                borderBottomBlue.Opacity = 0.6;
                borderBottomPurple.Opacity = 0.6;
                borderBottomOrange.Opacity = 0.6;
                borderBottomGreen.Opacity = 1;
            }

            //Midde layer
            if (d.selectedRecipes[selectedRecipe][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
            {
                borderMiddleYellow.Opacity = 1;
                borderMiddleBlack.Opacity = 0.6;
                borderMiddleRed.Opacity = 0.6;
                borderMiddleBlue.Opacity = 0.6;
                borderMiddlePurple.Opacity = 0.6;
                borderMiddleOrange.Opacity = 0.6;
                borderMiddleGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK)
            {
                borderMiddleYellow.Opacity = 0.6;
                borderMiddleBlack.Opacity = 1;
                borderMiddleRed.Opacity = 0.6;
                borderMiddleBlue.Opacity = 0.6;
                borderMiddlePurple.Opacity = 0.6;
                borderMiddleOrange.Opacity = 0.6;
                borderMiddleGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED)
            {
                borderMiddleYellow.Opacity = 0.6;
                borderMiddleBlack.Opacity = 0.6;
                borderMiddleRed.Opacity = 1;
                borderMiddleBlue.Opacity = 0.6;
                borderMiddlePurple.Opacity = 0.6;
                borderMiddleOrange.Opacity = 0.6;
                borderMiddleGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE)
            {
                borderMiddleYellow.Opacity = 0.6;
                borderMiddleBlack.Opacity = 0.6;
                borderMiddleRed.Opacity = 0.6;
                borderMiddleBlue.Opacity = 1;
                borderMiddlePurple.Opacity = 0.6;
                borderMiddleOrange.Opacity = 0.6;
                borderMiddleGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
            {
                borderMiddleYellow.Opacity = 0.6;
                borderMiddleBlack.Opacity = 0.6;
                borderMiddleRed.Opacity = 0.6;
                borderMiddleBlue.Opacity = 0.6;
                borderMiddlePurple.Opacity = 1;
                borderMiddleOrange.Opacity = 0.6;
                borderMiddleGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
            {
                borderMiddleYellow.Opacity = 0.6;
                borderMiddleBlack.Opacity = 0.6;
                borderMiddleRed.Opacity = 0.6;
                borderMiddleBlue.Opacity = 0.6;
                borderMiddlePurple.Opacity = 0.6;
                borderMiddleOrange.Opacity = 1;
                borderMiddleGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
            {
                borderMiddleYellow.Opacity = 0.6;
                borderMiddleBlack.Opacity = 0.6;
                borderMiddleRed.Opacity = 0.6;
                borderMiddleBlue.Opacity = 0.6;
                borderMiddlePurple.Opacity = 0.6;
                borderMiddleOrange.Opacity = 0.6;
                borderMiddleGreen.Opacity = 1;
            }

            //Midde layer
            if (d.selectedRecipes[selectedRecipe][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
            {
                borderTopYellow.Opacity = 1;
                borderTopBlack.Opacity = 0.6;
                borderTopRed.Opacity = 0.6;
                borderTopBlue.Opacity = 0.6;
                borderTopPurple.Opacity = 0.6;
                borderTopOrange.Opacity = 0.6;
                borderTopGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK)
            {
                borderTopYellow.Opacity = 0.6;
                borderTopBlack.Opacity = 1;
                borderTopRed.Opacity = 0.6;
                borderTopBlue.Opacity = 0.6;
                borderTopPurple.Opacity = 0.6;
                borderTopOrange.Opacity = 0.6;
                borderTopGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED)
            {
                borderTopYellow.Opacity = 0.6;
                borderTopBlack.Opacity = 0.6;
                borderTopRed.Opacity = 1;
                borderTopBlue.Opacity = 0.6;
                borderTopPurple.Opacity = 0.6;
                borderTopOrange.Opacity = 0.6;
                borderTopGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE)
            {
                borderTopYellow.Opacity = 0.6;
                borderTopBlack.Opacity = 0.6;
                borderTopRed.Opacity = 0.6;
                borderTopBlue.Opacity = 1;
                borderTopPurple.Opacity = 0.6;
                borderTopOrange.Opacity = 0.6;
                borderTopGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
            {
                borderTopYellow.Opacity = 0.6;
                borderTopBlack.Opacity = 0.6;
                borderTopRed.Opacity = 0.6;
                borderTopBlue.Opacity = 0.6;
                borderTopPurple.Opacity = 1;
                borderTopOrange.Opacity = 0.6;
                borderTopGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
            {
                borderTopYellow.Opacity = 0.6;
                borderTopBlack.Opacity = 0.6;
                borderTopRed.Opacity = 0.6;
                borderTopBlue.Opacity = 0.6;
                borderTopPurple.Opacity = 0.6;
                borderTopOrange.Opacity = 1;
                borderTopGreen.Opacity = 0.6;
            }
            else if (d.selectedRecipes[selectedRecipe][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
            {
                borderTopYellow.Opacity = 0.6;
                borderTopBlack.Opacity = 0.6;
                borderTopRed.Opacity = 0.6;
                borderTopBlue.Opacity = 0.6;
                borderTopPurple.Opacity = 0.6;
                borderTopOrange.Opacity = 0.6;
                borderTopGreen.Opacity = 1;
            }

            for (int i = 0; i < 6; i++)
            {
                //BOTTOM
                if (d.selectedRecipes[i][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                {
                    if (i == 0)
                    {
                        R1B.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 1)
                    {
                        R2B.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 2)
                    {
                        R3B.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 3)
                    {
                        R4B.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 4)
                    {
                        R5B.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 5)
                    {
                        R6B.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                }
                else if (d.selectedRecipes[i][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK)
                {
                    if (i == 0)
                    {
                        R1B.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 1)
                    {
                        R2B.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 2)
                    {
                        R3B.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 3)
                    {
                        R4B.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 4)
                    {
                        R5B.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 5)
                    {
                        R6B.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                }
                else if (d.selectedRecipes[i][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED)
                {
                    if (i == 0)
                    {
                        R1B.Fill = (Brush)this.FindResource("LGBRed");
                    }
                    else if (i == 1)
                    {
                        R2B.Fill = (Brush)this.FindResource("LGBRed");
                    }
                    else if (i == 2)
                    {
                        R3B.Fill = (Brush)this.FindResource("LGBRed");
                    }
                    else if (i == 3)
                    {
                        R4B.Fill = (Brush)this.FindResource("LGBRed");
                    }
                    else if (i == 4)
                    {
                        R5B.Fill = (Brush)this.FindResource("LGBRed");
                    }
                    else if (i == 5)
                    {
                        R6B.Fill = (Brush)this.FindResource("LGBRed");
                    }
                }
                else if (d.selectedRecipes[i][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE)
                {
                    if (i == 0)
                    {
                        R1B.Fill = (Brush)this.FindResource("LGBBlue");
                    }
                    else if (i == 1)
                    {
                        R2B.Fill = (Brush)this.FindResource("LGBBlue");
                    }
                    else if (i == 2)
                    {
                        R3B.Fill = (Brush)this.FindResource("LGBBlue");
                    }
                    else if (i == 3)
                    {
                        R4B.Fill = (Brush)this.FindResource("LGBBlue");
                    }
                    else if (i == 4)
                    {
                        R5B.Fill = (Brush)this.FindResource("LGBBlue");
                    }
                    else if (i == 5)
                    {
                        R6B.Fill = (Brush)this.FindResource("LGBBlue");
                    }
                }
                else if (d.selectedRecipes[i][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                {
                    if (i == 0)
                    {
                        R1B.Fill = (Brush)this.FindResource("LGBPurple");
                    }
                    else if (i == 1)
                    {
                        R2B.Fill = (Brush)this.FindResource("LGBPurple");
                    }
                    else if (i == 2)
                    {
                        R3B.Fill = (Brush)this.FindResource("LGBPurple");
                    }
                    else if (i == 3)
                    {
                        R4B.Fill = (Brush)this.FindResource("LGBPurple");
                    }
                    else if (i == 4)
                    {
                        R5B.Fill = (Brush)this.FindResource("LGBPurple");
                    }
                    else if (i == 5)
                    {
                        R6B.Fill = (Brush)this.FindResource("LGBPurple");
                    }
                }
                else if (d.selectedRecipes[i][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
                {
                    if (i == 0)
                    {
                        R1B.Fill = (Brush)this.FindResource("LGBOrange");
                    }
                    else if (i == 1)
                    {
                        R2B.Fill = (Brush)this.FindResource("LGBOrange");
                    }
                    else if (i == 2)
                    {
                        R3B.Fill = (Brush)this.FindResource("LGBOrange");
                    }
                    else if (i == 3)
                    {
                        R4B.Fill = (Brush)this.FindResource("LGBOrange");
                    }
                    else if (i == 4)
                    {
                        R5B.Fill = (Brush)this.FindResource("LGBOrange");
                    }
                    else if (i == 5)
                    {
                        R6B.Fill = (Brush)this.FindResource("LGBOrange");
                    }
                }
                else if (d.selectedRecipes[i][0] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                {
                    if (i == 0)
                    {
                        R1B.Fill = (Brush)this.FindResource("LGBGreen");
                    }
                    else if (i == 1)
                    {
                        R2B.Fill = (Brush)this.FindResource("LGBGreen");
                    }
                    else if (i == 2)
                    {
                        R3B.Fill = (Brush)this.FindResource("LGBGreen");
                    }
                    else if (i == 3)
                    {
                        R4B.Fill = (Brush)this.FindResource("LGBGreen");
                    }
                    else if (i == 4)
                    {
                        R5B.Fill = (Brush)this.FindResource("LGBGreen");
                    }
                    else if (i == 5)
                    {
                        R6B.Fill = (Brush)this.FindResource("LGBGreen");
                    }
                }
                //MIDDLE
                if (d.selectedRecipes[i][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                {
                    if (i == 0)
                    {
                        R1M.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 1)
                    {
                        R2M.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 2)
                    {
                        R3M.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 3)
                    {
                        R4M.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 4)
                    {
                        R5M.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 5)
                    {
                        R6M.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                }
                else if (d.selectedRecipes[i][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK)
                {
                    if (i == 0)
                    {
                        R1M.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 1)
                    {
                        R2M.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 2)
                    {
                        R3M.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 3)
                    {
                        R4M.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 4)
                    {
                        R5M.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 5)
                    {
                        R6M.Fill = Brushes.Black;
                    }
                }
                else if (d.selectedRecipes[i][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED)
                {
                    if (i == 0)
                    {
                        R1M.Fill = (Brush)this.FindResource("LGBRed");
                    }
                    else if (i == 1)
                    {
                        R2M.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 2)
                    {
                        R3M.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 3)
                    {
                        R4M.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 4)
                    {
                        R5M.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 5)
                    {
                        R6M.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                }
                else if (d.selectedRecipes[i][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE)
                {
                    if (i == 0)
                    {
                        R1M.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 1)
                    {
                        R2M.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 2)
                    {
                        R3M.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 3)
                    {
                        R4M.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 4)
                    {
                        R5M.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 5)
                    {
                        R6M.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                }
                else if (d.selectedRecipes[i][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                {
                    if (i == 0)
                    {
                        R1M.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 1)
                    {
                        R2M.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 2)
                    {
                        R3M.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 3)
                    {
                        R4M.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 4)
                    {
                        R5M.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 5)
                    {
                        R6M.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                }
                else if (d.selectedRecipes[i][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
                {
                    if (i == 0)
                    {
                        R1M.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 1)
                    {
                        R2M.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 2)
                    {
                        R3M.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 3)
                    {
                        R4M.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 4)
                    {
                        R5M.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 5)
                    {
                        R6M.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                }
                else if (d.selectedRecipes[i][1] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                {
                    if (i == 0)
                    {
                        R1M.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 1)
                    {
                        R2M.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 2)
                    {
                        R3M.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 3)
                    {
                        R4M.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 4)
                    {
                        R5M.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 5)
                    {
                        R6M.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                }
                //TOP
                if (d.selectedRecipes[i][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                {
                    if (i == 0)
                    {
                        R1T.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 1)
                    {
                        R2T.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 2)
                    {
                        R3T.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 3)
                    {
                        R4T.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 4)
                    {
                        R5T.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                    else if (i == 5)
                    {
                        R6T.Fill = (Brush)this.FindResource("LGBYellow");
                    }
                }
                else if (d.selectedRecipes[i][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK)
                {
                    if (i == 0)
                    {
                        R1T.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 1)
                    {
                        R2T.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 2)
                    {
                        R3T.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 3)
                    {
                        R4T.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 4)
                    {
                        R5T.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                    else if (i == 5)
                    {
                        R6T.Fill = (Brush)this.FindResource("LGBBlack");
                    }
                }
                else if (d.selectedRecipes[i][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED)
                {
                    if (i == 0)
                    {
                        R1T.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 1)
                    {
                        R2T.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 2)
                    {
                        R3T.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 3)
                    {
                        R4T.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 4)
                    {
                        R5T.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                    else if (i == 5)
                    {
                        R6T.Fill = (Brush)this.FindResource("LGBRed");;
                    }
                }
                else if (d.selectedRecipes[i][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE)
                {
                    if (i == 0)
                    {
                        R1T.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 1)
                    {
                        R2T.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 2)
                    {
                        R3T.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 3)
                    {
                        R4T.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 4)
                    {
                        R5T.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                    else if (i == 5)
                    {
                        R6T.Fill = (Brush)this.FindResource("LGBBlue");;
                    }
                }
                else if (d.selectedRecipes[i][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                {
                    if (i == 0)
                    {
                        R1T.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 1)
                    {
                        R2T.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 2)
                    {
                        R3T.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 3)
                    {
                        R4T.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 4)
                    {
                        R5T.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                    else if (i == 5)
                    {
                        R6T.Fill = (Brush)this.FindResource("LGBPurple");;
                    }
                }
                else if (d.selectedRecipes[i][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
                {
                    if (i == 0)
                    {
                        R1T.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 1)
                    {
                        R2T.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 2)
                    {
                        R3T.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 3)
                    {
                        R4T.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 4)
                    {
                        R5T.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                    else if (i == 5)
                    {
                        R6T.Fill = (Brush)this.FindResource("LGBOrange");;
                    }
                }
                else if (d.selectedRecipes[i][2] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                {
                    if (i == 0)
                    {
                        R1T.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 1)
                    {
                        R2T.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 2)
                    {
                        R3T.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 3)
                    {
                        R4T.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 4)
                    {
                        R5T.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                    else if (i == 5)
                    {
                        R6T.Fill = (Brush)this.FindResource("LGBGreen");;
                    }
                }
            }
            PCSMainWindow.getInstance().updateVesselSummaries();
        }

        private void borderRecipe1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedRecipe = 0;
            updateUI();
            e.Handled = true;
        }

        private void borderRecipe2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedRecipe = 1;
            updateUI();
            e.Handled = true;
        }

        private void borderRecipe3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedRecipe = 2;
            updateUI();
            e.Handled = true;
        }

        private void borderRecipe4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedRecipe = 3;
            updateUI();
            e.Handled = true;
        }

        private void borderRecipe5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedRecipe = 4;
            updateUI();
            e.Handled = true;
        }

        private void borderRecipe6_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedRecipe = 5;
            updateUI();
            e.Handled = true;
        }

        private void borderTopYellow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][2] = Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW;
            updateUI();
            e.Handled = true;
        }

        private void borderTopBlack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][2] = Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK;
            updateUI();
            e.Handled = true;
        }

        private void borderTopRed_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][2] = Datastructure.Model.Recipes.RecipeData.RECIPE.RED;
            updateUI();
            e.Handled = true;
        }

        private void borderTopBlue_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][2] = Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE;
            updateUI();
            e.Handled = true;
        }

        private void borderTopPurple_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][2] = Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE;
            updateUI();
            e.Handled = true;
        }

        private void borderTopOrange_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][2] = Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE;
            updateUI();
            e.Handled = true;
        }

        private void borderTopGreen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][2] = Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN;
            updateUI();
            e.Handled = true;
        }

        private void borderMiddleYellow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][1] = Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW;
            updateUI();
            e.Handled = true;
        }

        private void borderMiddleBlack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][1] = Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK;
            updateUI();
            e.Handled = true;
        }

        private void borderMiddleRed_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][1] = Datastructure.Model.Recipes.RecipeData.RECIPE.RED;
            updateUI();
            e.Handled = true;
        }

        private void borderMiddleBlue_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][1] = Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE;
            updateUI();
            e.Handled = true;
        }

        private void borderMiddlePurple_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][1] = Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE;
            updateUI();
            e.Handled = true;
        }

        private void borderMiddleOrange_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][1] = Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE;
            updateUI();
            e.Handled = true;
        }

        private void borderMiddleGreen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][1] = Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN;
            updateUI();
            e.Handled = true;
        }

        private void borderBottomYellow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][0] = Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW;
            updateUI();
            e.Handled = true;
        }

        private void borderBottomBlack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][0] = Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK;
            updateUI();
            e.Handled = true;
        }

        private void borderBottomRed_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][0] = Datastructure.Model.Recipes.RecipeData.RECIPE.RED;
            updateUI();
            e.Handled = true;
        }

        private void borderBottomBlue_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][0] = Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE;
            updateUI();
            e.Handled = true;
        }

        private void borderBottomPurple_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][0] = Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE;
            updateUI();
            e.Handled = true;
        }

        private void borderBottomOrange_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][0] = Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE;
            updateUI();
            e.Handled = true;
        }

        private void borderBottomGreen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData.selectedRecipes[selectedRecipe][0] = Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN;
            updateUI();
            e.Handled = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ControlModules.SchedulingModule.TAOptModel.TAOptModelBuilder.getInstance().buildModel(Gateway.ObserverModule.getInstance().getCurrentPlant().RecipeData, System.IO.Directory.GetCurrentDirectory());
            this.Close();
        }
    }
}
