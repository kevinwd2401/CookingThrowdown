using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public float cookProgress = 0f;
    public float cookSpeed = 0.1f;
    public int cookStatus = 0; // 0 = raw, 1 = cooked, 2 = burnt  

    // called when ingredient is on hot pan
    public void Cook(float heatIntensity) {
        cookProgress += cookSpeed * heatIntensity * Time.deltaTime;

        if (cookProgress >= 2.0f && cookStatus != 2) { // burnt
            cookStatus = 2;
            OnBurnt();
        } else if (cookProgress >= 1.0f && cookStatus != 1) { // cooked
            cookStatus = 1;
            OnCooked();
        }
    }
        
    private void OnCooked() {
        Debug.Log(gameObject.name + " is cooked!");
        // TODO: change mesh/mat/color/make sound
    }

    private void OnBurnt() {
        Debug.Log(gameObject.name + " is burnt!");
        // TODO: change mesh/mat/color/make sound
    }

    public virtual void ThrowableOnCollide(Collision c){

    }

}
