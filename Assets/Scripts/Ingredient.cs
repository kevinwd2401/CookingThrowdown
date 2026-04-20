using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public float cookProgress = 0f;
    public float cookSpeed = 0.1f;
    public int cookStatus = 0; // 0 = raw, 1 = cooked, 2 = burnt  

    public Material cookedMat;
    public Material burntMat;

    public AudioSource audioSource;
    public AudioClip ding;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    // called when ingredient is on hot pan
    public void Cook(float heatIntensity) {
        cookProgress += cookSpeed * heatIntensity * Time.deltaTime;
        Debug.Log(cookProgress);
        if (cookProgress >= 2.0f) { // burnt
            if (cookStatus != 2) {
                cookStatus = 2;
                OnBurnt();
            }
        } else if (cookProgress >= 1.0f) { // cooked
            if (cookStatus != 1) {
                cookStatus = 1;
                OnCooked();
            }
        }
    }
        
    private void OnCooked() {
        Debug.Log(gameObject.name + " is cooked!");

        // TODO: change mesh/mat/color/make sound
        transform.Find("Mesh").GetComponent<Renderer>().material = cookedMat;
        audioSource.PlayOneShot(ding, 1.0f);
    }

    private void OnBurnt() {
        Debug.Log(gameObject.name + " is burnt!");

        // TODO: change mesh/mat/color/make sound
        transform.Find("Mesh").GetComponent<Renderer>().material = cookedMat;
        audioSource.PlayOneShot(ding, 1.0f);
    }

    public virtual void ThrowableOnCollide(Collision c){

    }

}
