using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool gameEnded = false;

    [SerializeField] float timer;
    private int reputation;
    public int Reputation {get {return reputation;}
        set {
            if (gameOver) return;
            reputation = value;
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
        
    }

    void Update()
    {
        if (gameOver) return;
        timer -= Time.deltaTime;
        if (timer <= 0) {
            gameOver = true;
            timer = 0;
            // end game
        }
    }
}
