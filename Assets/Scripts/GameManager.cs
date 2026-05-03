using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool gameEnded = false;

    public List<LevelData> levels;
    public static int currentLevelIndex = 0;

    [SerializeField] private UnityEngine.UI.Slider repSlider;
    [SerializeField] private TextMeshProUGUI timerText;
    public GameObject[] throwingList;
    public Transform playerTrans; 
    public HashSet<Transform> npcTransforms = new HashSet<Transform>();

    public TextMeshProUGUI endGameText;
    public TextMeshProUGUI recipeTitle;
    public TextMeshProUGUI recipeSteps;

    public AudioSource audioSource;
    public AudioClip winAudio;
    public AudioClip loseAudio;

    [SerializeField] int timer;
    private int reputation;

    [Header("End Game UI")]
    public GameObject endGamePanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI breakdownText;

    public static bool endUI = false; // true is default old, false is new

    public int Reputation {get {return reputation;}
        set {
            if (gameOver) return;
            reputation = value;
            repSlider.value = reputation;
            if (reputation <= 0) {
                reputation = 0;
                endGameText.text = "Reputation: Rock bottom...";
                breakdownText.text = "You got hit too many times. Your reputation is in shambles, no one will hire you again...";
                
                if (GameManager.endUI) {
                    LoseGame();
                } else {
                    AltLoseGame();
                }
            }
        }}
    bool gameOver;

    void Awake() {
        Instance = this;
        reputation = 100;
        Time.timeScale = 0.75f;
        gameOver = false;

        endGamePanel.SetActive(false);
    }

    void Start()
    {
        LoadLevel(levels[currentLevelIndex]);

        StartCoroutine(timerCor());
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        // switch the endUI
        if (Input.GetKeyDown(KeyCode.U)) {
            endUI = !endUI;
            Debug.Log("U was pressed");
        }
    }

    private IEnumerator timerCor() {
        while (!gameOver) {
            yield return new WaitForSecondsRealtime(1);
            timer -= 1;

            int minutes = timer / 60;
            int remainingSeconds = timer % 60;
            timerText.text = $"Time Left: {minutes:D2}:{remainingSeconds:D2}";
            if (timer <= 0) {
                endGameText.text = "You ran out of time...";
                breakdownText.text = "You were too slow. Your audience got so bored that they left...";

                // TODO: choose one
                if (GameManager.endUI) {
                    LoseGame();
                } else {
                    AltLoseGame();
                } 
            }
        }
    }

    void LoadLevel(LevelData level) {
        npcTransforms.Clear();

        // update timer and rep
        timer = level.timeLimit;
        reputation = level.startingReputation;

        // update throwable spawners
        throwingList = level.spawnableIngredients;

        // update player facing recipe
        recipeTitle.text = level.recipe.recipeName + " Recipe";

        SetNPCSpawnRate(level.spawnRate);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < level.recipe.steps.Count; i++) {
            sb.AppendLine($"{i + 1}. {level.recipe.steps[i]}");
        }
        recipeSteps.text = sb.ToString();

        Debug.Log("Loaded" + level.levelName);
    }

    public void WinGame() {
        if (gameOver) return;
        gameOver = true;
        endGameText.text = "You win!!";
        audioSource.PlayOneShot(winAudio, 1.0f);
        NPCsLeave();
        Debug.Log("Game Won!");
        StartCoroutine(NextLevelDelay());
    }

    public void LoseGame() {
        if (gameOver) return;
        gameOver = true;
        audioSource.PlayOneShot(loseAudio, 1.0f);
        NPCsLeave();
        Debug.Log("Game Lost!");
    }

    // new win/lose game with new ui
    public void AltWinGame() {
        if (gameOver) return;
        gameOver = true;
        audioSource.PlayOneShot(winAudio, 1.0f);
        NPCsLeave();

        // calculate scores
        int timeScore = CalculateTimeScore();
        int repScore = CalculateReputationScore();
        int accuracyScore = CalculateCookingAccuracy();
        int total = timeScore + repScore + accuracyScore;

        // format text
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Time Bonus: +{timeScore}");
        sb.AppendLine($"Reputation: +{repScore}");
        sb.AppendLine($"Cooking Quality: +{accuracyScore}");
        sb.AppendLine($"TOTAL: {total}");

        breakdownText.text = sb.ToString();

        // set ui text
        endGameText.text = "You win!!";
        titleText.text = "You win!!";

        // set ui panels
        endGamePanel.SetActive(true);
        losePanel.SetActive(false);
        winPanel.SetActive(true);

        Debug.Log("Game Won!");
    }

    public void AltLoseGame() {
        if (gameOver) return;
        gameOver = true;
        audioSource.PlayOneShot(loseAudio, 1.0f);
        NPCsLeave();

        // set ui text
        endGameText.text = "You Lose :(";
        titleText.text = "You Lose :(";

        // set ui panels
        endGamePanel.SetActive(true);
        losePanel.SetActive(true);
        winPanel.SetActive(false);

        Debug.Log("Game Lost!");
    }

    public int CalculateTimeScore() {
        float score = 0;
        float maxTime = (float)levels[currentLevelIndex].timeLimit;

        // 25 total points from time
        if (timer >= maxTime * 0.75f) score += 25;
        else if (timer >= maxTime * 0.5f) score += 15;
        else if (timer > 0) score += 5;

        return Mathf.RoundToInt(score);
    }

    public int CalculateReputationScore() {
        float score = 0;
        float maxRep = (float)levels[currentLevelIndex].startingReputation;

        // 25 total points from reputation
        if (reputation >= maxRep * 0.8f) score += 25;
        else if (reputation >= maxRep * 0.4f) score += 15;
        else if (reputation > 0) score += 5;

        return Mathf.RoundToInt(score);
    }

    public int CalculateCookingAccuracy() {
        int accuracyScore = 50;
        List<Ingredient> plateItems = FindObjectOfType<Plate>().ingredientsOnPlate;

        foreach (Ingredient item in plateItems) {
            // penalty for burnt
            if (item.cookStatus == Ingredient.CookState.Burnt) {
                accuracyScore -= 10;
            }
            // penalty for rotten
            if (item.isRotten == true) {
                accuracyScore -= 20;
            }
        }

        return Mathf.Clamp(accuracyScore, 0, 50); // between 0 and 50
    }

    public void SetNPCSpawnRate(float spawnRate) {
        GetComponent<NPCManager>().SpawnRate = spawnRate;
    }

    private void NPCsLeave() {
        foreach (Transform t in npcTransforms) {
            if (t != null) {
                Hater h = t.gameObject.GetComponent<Hater>();
                if (h != null) {
                    h.Stun();
                }
            }
        }
    }

    IEnumerator NextLevelDelay() {
        yield return new WaitForSeconds(3f);
        LoadNextLevel();
    }

    public void LoadNextLevel() {
        currentLevelIndex++;
        if (currentLevelIndex < levels.Count) {
            // Reloads the active scene
            Time.timeScale = 0.75f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } else {
            endGameText.text = "Completed all levels!";
            Debug.Log("Completed all levels!");
            currentLevelIndex = 0;
        }
    }
}
