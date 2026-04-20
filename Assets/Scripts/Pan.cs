using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class Pan : MonoBehaviour
{
    public bool isHot;
    private List<Ingredient> ingredientsInPan = new List<Ingredient>();
    private Burner currentBurner;

    public AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        // check if the burner is on
        if (currentBurner != null && currentBurner.isHot) {
            isHot = true;
        } else {
            isHot = false;
        }

        // cook ingredients in pan
        if (isHot) {
            foreach (Ingredient food in ingredientsInPan) {
                food.Cook(1.0f);
            }
        }

        // sizzle
        HandleSizzleSound();
    }

    void HandleSizzleSound() {
        // is hot and there is at leasy one ingredient in the pan
        bool shouldSizzle = isHot && ingredientsInPan.Count > 0;

        if (shouldSizzle) {
            if (!audioSource.isPlaying) {
                audioSource.Play();
            }
        } else {
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        // add ingredients
        if (other.TryGetComponent<Ingredient>(out Ingredient food)) {
            if (!ingredientsInPan.Contains(food)) ingredientsInPan.Add(food);
            Debug.Log("Ingredient Added to Pan!");
        }

        // detect burner
        if (other.CompareTag("Burner")) {
            currentBurner = other.GetComponent<Burner>();
            Debug.Log("Hot Pan!");
        }
    }

    void OnTriggerExit(Collider other) {
        // remove ingredients
        if (other.TryGetComponent<Ingredient>(out Ingredient food)) {
            ingredientsInPan.Remove(food);
        }

        // stop heating if we leave the burner
        if (other.CompareTag("Burner")) {
            currentBurner = null;
            isHot = false;
        }
    }
}
