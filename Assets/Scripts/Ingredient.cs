using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public enum CookState { Raw, Cooked, Burnt }
    public enum IngredientType { Steak, Bread, Tomato, Cheese, Patty}
    public float cookProgress = 0f; 
    public float cookSpeed = 0.1f; 
    public bool isRotten = false;
    public bool isSliced = false;
    public CookState cookStatus = CookState.Raw; 
    public IngredientType ingredient = IngredientType.Steak;

    public Material cookedMat;
    public Material burntMat;

    public AudioSource audioSource;
    public AudioClip ding;
    public AudioClip burn;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void Cook(float heatIntensity)
    {
        cookProgress += cookSpeed * heatIntensity * Time.deltaTime;
        Debug.Log(cookProgress);

        if (cookProgress >= 2.0f)
        {
            if (cookStatus != CookState.Burnt)
            {
                cookStatus = CookState.Burnt;
                OnBurnt();
            }
        }
        else if (cookProgress >= 1.0f)
        {
            if (cookStatus != CookState.Cooked)
            {
                cookStatus = CookState.Cooked;
                OnCooked();
            }
        }
    }

    private void OnCooked()
    {
        Debug.Log(gameObject.name + " is cooked!");

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null && cookedMat != null)
            r.material = cookedMat;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null && ding != null)
            audioSource.PlayOneShot(ding, 1.0f);
        else
            Debug.LogWarning("Missing ding sound or AudioSource on " + name);
    }

    private void OnBurnt()
    {
        Debug.Log(gameObject.name + " is burnt!");

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null && burntMat != null)
            r.material = burntMat;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null && burn != null)
            audioSource.PlayOneShot(burn, 0.75f);
        else
            Debug.LogWarning("Missing burn sound or AudioSource on " + name);
    }

    public virtual void ThrowableOnCollide(Collision c)
    {
    }
}