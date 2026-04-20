using UnityEngine;

public class CuttableIngredient : MonoBehaviour
{
    [Header("Cut Result")]
    [SerializeField] private GameObject cutResultPrefab;

    [Header("Cut Settings")]
    [SerializeField] private float minCutSpeed = 1.0f;
    [SerializeField] private bool destroyOriginal = true;

    private bool hasBeenCut = false;

    public void TryCut(float knifeSpeed, Vector3 cutDirection)
    {
        if (hasBeenCut) return;
        if (cutResultPrefab == null) return;
        if (knifeSpeed < minCutSpeed) return;

        hasBeenCut = true;

        GameObject cutObj = Instantiate(
            cutResultPrefab,
            transform.position,
            transform.rotation
        );

        Rigidbody rb = cutObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(cutDirection * 0.5f, ForceMode.Impulse);
        }

        if (destroyOriginal)
        {
            Destroy(gameObject);
        }
    }
}