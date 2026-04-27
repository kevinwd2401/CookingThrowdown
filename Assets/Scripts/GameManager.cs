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

    [SerializeField] int timer;
    private int reputation;

    public int Reputation {get {return reputation;}
        set {
            if (gameOver) return;
            reputation = value;
            repSlider.value = reputation;
            if (reputation <= 0) {
                reputation = 0;
                endGameText.text = "Reputation: Rock bottom...";
                LoseGame();
                //end game
            }
        }}
    bool gameOver;

    void Awake() {
        Instance = this;
        reputation = 100;
        Time.timeScale = 0.75f;
        gameOver = false;
    }

    void Start()
    {
        LoadLevel(levels[currentLevelIndex]);

        StartCoroutine(timerCor());
        audioSource = GetComponent<AudioSource>();
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
                LoseGame();
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
        recipeTitle.text = level.recipe.recipeName;

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
        NPCsLeave();
        Debug.Log("Game Lost! You died/timed out!");
    }

    public int CalculateScore() {
        int score = 0;

        // time bonus
        score += timer * 10;

        // reputation bonus
        score += reputation * 5;

        // TODO: COOKING ACCURACY
        // TODO: ADD THIS WHERE PLAYER CAN SEE
        // score += CalculateCookingAccuracy();

        return score;
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
