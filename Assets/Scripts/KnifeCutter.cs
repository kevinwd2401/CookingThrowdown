using UnityEngine;

public class KnifeCutter : MonoBehaviour
{
    [Header("Cut Settings")]
    [SerializeField] private float minCutSpeed = 1.0f;

    private Rigidbody knifeRb;

    private void Awake()
    {
        knifeRb = GetComponent<Rigidbody>();
    }

    public float GetKnifeSpeed()
    {
        if (knifeRb == null) return 0f;
        return knifeRb.velocity.magnitude;
    }

    public bool CanCut()
    {
        return GetKnifeSpeed() >= minCutSpeed;
    }
}