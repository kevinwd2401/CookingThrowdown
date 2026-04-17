using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool gameEnded = false;
    [SerializeField] private Slider repSlider;
    [SerializeField] private TextMeshProUGUI timerText;
    public GameObject[] throwingList;
    public Transform playerTrans; 
    public HashSet<Transform> npcTransforms = new HashSet<Transform>();

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
    }

    void Start()
    {
        StartCoroutine(timerCor());
    }
    private IEnumerator timerCor() {
        while (!gameOver) {
            yield return new WaitForSeconds(1);
            timer -= 1;

            int minutes = timer / 60;
            int remainingSeconds = timer % 60;
            timerText.text = $"{minutes:D2}:{remainingSeconds:D2}";
            if (timer <= 0) {
                gameOver = true;
            }
        }
        
    }
}
