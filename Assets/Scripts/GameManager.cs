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

    public GameObject winText;

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
        if (winText != null) winText.SetActive(false);
    }
    private IEnumerator timerCor() {
        while (!gameOver) {
            yield return new WaitForSecondsRealtime(1);
            timer -= 1;

            int minutes = timer / 60;
            int remainingSeconds = timer % 60;
            timerText.text = $"{minutes:D2}:{remainingSeconds:D2}";
            if (timer <= 0) {
                gameOver = true;
            }
        }
        
    }

    public void WinGame() {
        if (gameOver) return;
        gameOver = true;
        if (winText != null) winText.SetActive(true);
        audioSource.PlayOneShot(winAudio, 1.0f);
        Debug.Log("Game Won!");
    }
}
