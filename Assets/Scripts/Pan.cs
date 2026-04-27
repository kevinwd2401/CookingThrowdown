using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class Pan : MonoBehaviour {
    public bool isHot;
    private List<Ingredient> ingredientsInPan = new List<Ingredient>();
    private List<Burner> burnersInRange = new List<Burner>();
    public Transform heatPoint;
    public float snapDistance = 0.5f;

    public AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        // check if the burner is on
        Burner closest = null;
        float minDist = Mathf.Infinity;

        foreach (Burner burner in burnersInRange) {
            float dist = Vector3.Distance(heatPoint.position, burner.transform.position);
            if (dist < minDist) {
                minDist = dist;
                closest = burner;
            }
        }

        isHot = closest != null && closest.isHot;

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
        // is hot and there is at least one ingredient in the pan
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
            Burner burner = other.GetComponent<Burner>();
            if (burner != null && !burnersInRange.Contains(burner)) {
                burnersInRange.Add(burner);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        // remove ingredients
        if (other.TryGetComponent<Ingredient>(out Ingredient food)) {
            ingredientsInPan.Remove(food);
        }

        // stop heating if we leave the burner
        if (other.CompareTag("Burner")) {
            Burner burner = other.GetComponent<Burner>();
            if (burner != null) {
                burnersInRange.Remove(burner);
            }
        }
    }

    public void OnGrabbed() {
        foreach (Ingredient food in ingredientsInPan) {
            food.transform.SetParent(transform);
            Rigidbody rb = food.GetComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.constraints = RigidbodyConstraints.FreezeAll;

            Physics.SyncTransforms();
        }
    }

    public void OnReleased() {
        foreach (Ingredient food in ingredientsInPan) {
            food.transform.SetParent(null);
            Rigidbody rb = food.GetComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }
    }
}