using System.Collections;
using System.Collections.Generic;
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
        // TODO: check recipe, could do win condition here? 
        Debug.Log("Items on plate: " + ingredientsOnPlate.Count);
    }
}
