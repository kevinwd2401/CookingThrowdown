using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Plate : MonoBehaviour
{
    public List<Ingredient> ingredientsOnPlate = new List<Ingredient>();

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Ingredient>(out Ingredient item)) {
            if (!ingredientsOnPlate.Contains(item)) {
                ingredientsOnPlate.Add(item);
                Debug.Log($"{item.name} entered the plate.");
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.TryGetComponent<Ingredient>(out Ingredient item)) {
            if (ingredientsOnPlate.Contains(item)) {
                ingredientsOnPlate.Remove(item);
                Debug.Log($"{item.name} left the plate.");
            }
        }
    }

    public void CheckRecipeStatus() {
        Debug.Log("checking recipe status!");
        ingredientsOnPlate.RemoveAll(item => item == null);

        // count what's on the plate
        Dictionary<(Ingredient.IngredientType, Ingredient.CookState), int> counts
        = new Dictionary<(Ingredient.IngredientType, Ingredient.CookState), int>();

        foreach (Ingredient item in ingredientsOnPlate) {

            var key = (item.ingredient, item.cookStatus);

            if (!counts.ContainsKey(key)) {
                counts[key] = 0;
            }

            counts[key]++;
        }

        // TODO: compare sliced
        // compare against recipe
        LevelData level = GameManager.Instance.levels[GlobalData.currentLevelIndex];
        foreach (var req in level.recipe.requirements) {

            var key = (req.ingredient, req.cookStatus);

            if (!counts.ContainsKey(key) || counts[key] < req.quantity) {
                Debug.Log("not enough ingredients");
                return;
            }
        }

        GameManager.Instance.WinGame();
    }
}
