using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Level")]
public class LevelData : ScriptableObject {
    public string levelName;

    public Recipe recipe;

    public int timeLimit;
    public int startingReputation;

    public GameObject[] spawnableIngredients;
}