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
        Debug.Log("Items on plate: " + ingredientsOnPlate.Count);

        // TODO: change this for each recipe
        bool recipeComplete = false;
        int count = 0;
        foreach (Ingredient item in ingredientsOnPlate) {
            // check for cooked steak
            if (item.ingredientId == 0 && item.cookStatus == 1) {
                count += 1;
            }
        }

        if (count == 5 && ingredientsOnPlate.Count == 5) {
            Debug.Log("recipe complete = true");
            recipeComplete = true;
        }

        if (recipeComplete) {
            GameManager.Instance.WinGame();
        }
    }
}
