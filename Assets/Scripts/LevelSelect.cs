using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class LevelSelect : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    [SerializeField] LevelData[] levelArray;
    public int levelIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        DisplayLevelData();
    }

    public void OnStartButtonPressed() {
        //load level for the LevelData found in levelArray[levelIndex]
        GameManager.Instance.LoadLevel(levelIndex);
    }

    public void OnNextPressed() {
        levelIndex++;
        if (levelIndex >= levelArray.Length || levelIndex > GlobalData.LevelsBeat) {
            levelIndex = 0;
        }
        DisplayLevelData();
    }

    public void OnBackPressed() {
        levelIndex--;
        if (levelIndex < 0) {
            levelIndex = GlobalData.LevelsBeat;
        }
        DisplayLevelData();
    }

    private void DisplayLevelData() {

        int minutes = levelArray[levelIndex].timeLimit / 60;
        int remainingSeconds = levelArray[levelIndex].timeLimit % 60;
        string time = $"\nTime Limit: {minutes:D2}:{remainingSeconds:D2}\n";

        string audience = (levelArray[levelIndex].spawnRate > 1.1f) ? "Popularity: High\n" :
            ((levelArray[levelIndex].spawnRate < 0.9f) ? "Popularity: Low\n" : "Popularity: Medium\n");

        string reputation = $"Reputation: {levelArray[levelIndex].startingReputation}%";

        levelText.text = levelArray[levelIndex].levelName + "\n" +
        "Dish: " + levelArray[levelIndex].recipe.recipeName + time + audience + reputation;
    }
}
