using UnityEngine;

[System.Serializable]
public class RecipeRequirement {
    public Ingredient.IngredientType ingredient;
    public Ingredient.CookState cookStatus;
    public int quantity;
    public bool isSliced;
}