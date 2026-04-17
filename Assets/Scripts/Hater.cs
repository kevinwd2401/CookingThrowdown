using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hater : MonoBehaviour
{
    public bool hostile = true;
    public int spawnIndex;
    //[SerializeField] bool stunned;

    [SerializeField] private int hitsUntilLeave = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Stun() {
        if (hitsUntilLeave <= 0) return;

        hitsUntilLeave--;
        if (hitsUntilLeave == 0) {
            Leave();
        }
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.TryGetComponent<Throwable>(out Throwable t) && !t.hurtsPlayer) {
            //stun NPC
            Stun();
            t.RemoveIngredient();
        }
    }
    private void Leave() {
        
    }
}
