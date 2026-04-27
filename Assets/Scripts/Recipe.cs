using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject {
    public string recipeName;
    public List<string> steps;
    public List<RecipeRequirement> requirements;
}
