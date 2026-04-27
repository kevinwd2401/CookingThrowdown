using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    public bool isHot;

    private List<Ingredient> ingredientsInPan = new List<Ingredient>();
    private List<Burner> burnersInRange = new List<Burner>();

    public Transform heatPoint;
    public float snapDistance = 0.5f;

    public AudioSource audioSource;

    private bool panGrabbed = false;

    private class HeldIngredientData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Rigidbody rb;
    }

    private Dictionary<Ingredient, HeldIngredientData> heldData = new Dictionary<Ingredient, HeldIngredientData>();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        Burner closest = null;
        float minDist = Mathf.Infinity;

        foreach (Burner burner in burnersInRange)
        {
            if (burner == null) continue;

            float dist = Vector3.Distance(heatPoint.position, burner.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = burner;
            }
        }

        isHot = closest != null && closest.isHot;

        if (isHot)
        {
            foreach (Ingredient food in ingredientsInPan)
            {
                if (food != null)
                    food.Cook(1.0f);
            }
        }

        HandleSizzleSound();
    }

    private void LateUpdate()
    {
        if (!panGrabbed) return;

        foreach (var pair in heldData)
        {
            Ingredient food = pair.Key;
            HeldIngredientData data = pair.Value;

            if (food == null) continue;

            food.transform.position = transform.TransformPoint(data.localPosition);
            food.transform.rotation = transform.rotation * data.localRotation;
        }
    }

    void HandleSizzleSound()
    {
        bool shouldSizzle = isHot && ingredientsInPan.Count > 0;

        if (audioSource == null) return;

        if (shouldSizzle)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Ingredient food = other.GetComponentInParent<Ingredient>();

        if (food != null)
        {
            if (!ingredientsInPan.Contains(food))
                ingredientsInPan.Add(food);

            Debug.Log("Ingredient Added to Pan!");

            if (panGrabbed)
                LockIngredientToPan(food);
        }

        if (other.CompareTag("Burner"))
        {
            Burner burner = other.GetComponent<Burner>();

            if (burner != null && !burnersInRange.Contains(burner))
                burnersInRange.Add(burner);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Ingredient food = other.GetComponentInParent<Ingredient>();

        if (food != null)
        {
            ingredientsInPan.Remove(food);

            if (!panGrabbed)
                heldData.Remove(food);
        }

        if (other.CompareTag("Burner"))
        {
            Burner burner = other.GetComponent<Burner>();

            if (burner != null)
                burnersInRange.Remove(burner);
        }
    }

    public void OnGrabbed()
    {
        panGrabbed = true;
        heldData.Clear();

        foreach (Ingredient food in ingredientsInPan)
        {
            if (food != null)
                LockIngredientToPan(food);
        }
    }

    private void LockIngredientToPan(Ingredient food)
    {
        Rigidbody foodRb = food.GetComponent<Rigidbody>();

        if (foodRb != null)
        {
            foodRb.velocity = Vector3.zero;
            foodRb.angularVelocity = Vector3.zero;
            foodRb.isKinematic = true;
            foodRb.useGravity = false;
            foodRb.constraints = RigidbodyConstraints.None;
        }

        HeldIngredientData data = new HeldIngredientData();
        data.localPosition = transform.InverseTransformPoint(food.transform.position);
        data.localRotation = Quaternion.Inverse(transform.rotation) * food.transform.rotation;
        data.rb = foodRb;

        heldData[food] = data;
    }

    public void OnReleased()
    {
        panGrabbed = false;

        foreach (var pair in heldData)
        {
            Ingredient food = pair.Key;
            HeldIngredientData data = pair.Value;

            if (food == null || data.rb == null) continue;

            data.rb.isKinematic = false;
            data.rb.useGravity = true;
            data.rb.constraints = RigidbodyConstraints.None;
            data.rb.velocity = Vector3.zero;
            data.rb.angularVelocity = Vector3.zero;
        }

        heldData.Clear();
    }
}