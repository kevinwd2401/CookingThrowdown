using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonVR : MonoBehaviour {
    AudioSource sound;
    // Start is called before the first frame update
    void Start() {
        sound = GetComponent<AudioSource>();
    }

    public void onPressed() {
        sound.Play();
    }
}