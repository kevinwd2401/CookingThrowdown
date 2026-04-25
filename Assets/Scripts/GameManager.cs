using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool gameEnded = false;
    [SerializeField] private UnityEngine.UI.Slider repSlider;
    [SerializeField] private TextMeshProUGUI timerText;
    public GameObject[] throwingList;
    public Transform playerTrans; 
    public HashSet<Transform> npcTransforms = new HashSet<Transform>();

    public TextMeshProUGUI endGameText;

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
    }

    void Start()
    {
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

    public void WinGame() {
        if (gameOver) return;
        gameOver = true;
        endGameText.text = "You win!!";
        audioSource.PlayOneShot(winAudio, 1.0f);
        NPCsLeave();
        Debug.Log("Game Won!");
    }

    public void LoseGame() {
        if (gameOver) return;
        gameOver = true;
        NPCsLeave();
        Debug.Log("Game Lost! You died/timed out!");
    }

    private void NPCsLeave() {
        foreach (Transform t in npcTransforms) {
            Hater h = t.gameObject.GetComponent<Hater>();
            h.Stun();
        }
    }
}
