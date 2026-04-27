using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Plate : MonoBehaviour
{
    public List<Ingredient> ingredientsOnPlate = new List<Ingredient>();

    void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Ingredient>(out Ingredient food)) {
            if (!ingredientsOnPlate.Contains(food)) {
                ingredientsOnPlate.Add(food);
                CheckRecipeStatus();
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.TryGetComponent<Ingredient>(out Ingredient food)) {
            ingredientsOnPlate.Remove(food);
            CheckRecipeStatus();
        }
    }
    void CheckRecipeStatus() {
        Dictionary<(Ingredient.IngredientType, Ingredient.CookState), int> counts
        = new Dictionary<(Ingredient.IngredientType, Ingredient.CookState), int>();

        // count what's on the plate
        foreach (Ingredient item in ingredientsOnPlate) {

            var key = (item.ingredient, item.cookStatus);

            if (!counts.ContainsKey(key)) {
                counts[key] = 0;
            }

            counts[key]++;
        }

        // TODO: compare sliced
        // compare against recipe
        LevelData level = GameManager.Instance.levels[GameManager.currentLevelIndex];
        foreach (var req in level.recipe.requirements) {

            var key = (req.ingredient, req.cookStatus);

            if (!counts.ContainsKey(key) || counts[key] < req.quantity) {
                return;
            }
        }

        GameManager.Instance.WinGame();
    }
}
