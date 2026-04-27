using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public float cookProgress = 0f;
    public float cookSpeed = 0.1f;
    public int cookStatus = 0; // 0 = raw, 1 = cooked, 2 = burnt
    public bool isRotten = false;
    public int ingredientId = 0;
    // steak == 0
    // cheese == 1
    // tomato == 2

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
            if (cookStatus != 2)
            {
                cookStatus = 2;
                OnBurnt();
            }
        }
        else if (cookProgress >= 1.0f)
        {
            if (cookStatus != 1)
            {
                cookStatus = 1;
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