using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MeshSliceableIngredient : MonoBehaviour
{
    [Header("Slice Settings")]
    [SerializeField] private Material crossSectionMaterial;
    [SerializeField] private int maxSliceCount = 3;
    [SerializeField] private float pieceForce = 0.2f;

    private int sliceCount = 0;
    private bool isSlicing = false;

    public void Slice(Vector3 slicePosition, Vector3 sliceNormal)
    {
        if (isSlicing) return;
        if (sliceCount >= maxSliceCount) return;

        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (mf == null || mr == null)
        {
            Debug.LogWarning(name + " cannot slice: missing MeshFilter or MeshRenderer.");
            return;
        }

        isSlicing = true;

        GameObject[] pieces = gameObject.SliceInstantiate(
            slicePosition,
            sliceNormal,
            crossSectionMaterial
        );

        if (pieces == null || pieces.Length == 0)
        {
            Debug.LogWarning("Slice failed: no pieces created.");
            isSlicing = false;
            return;
        }

        Debug.Log("Slice success. Pieces created: " + pieces.Length);

        foreach (GameObject piece in pieces)
        {
            SetupPiece(piece, sliceNormal);
        }

        Destroy(gameObject);
    }

    private void SetupPiece(GameObject piece, Vector3 forceDirection)
    {
        piece.transform.position = transform.position;
        piece.transform.rotation = transform.rotation;
        piece.transform.localScale = transform.localScale;

        if (piece.GetComponent<MeshCollider>() == null)
        {
            MeshCollider col = piece.AddComponent<MeshCollider>();
            col.convex = true;
        }

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb == null)
            rb = piece.AddComponent<Rigidbody>();

        rb.mass = 0.2f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.AddForce(forceDirection.normalized * pieceForce, ForceMode.Impulse);

        if (piece.GetComponent<Ingredient>() == null)
            piece.AddComponent<Ingredient>();

        MeshSliceableIngredient newSliceable = piece.AddComponent<MeshSliceableIngredient>();
        newSliceable.crossSectionMaterial = crossSectionMaterial;
        newSliceable.maxSliceCount = maxSliceCount;
        newSliceable.sliceCount = sliceCount + 1;
        newSliceable.pieceForce = pieceForce;

        if (piece.GetComponent<XRGrabInteractable>() == null)
            piece.AddComponent<XRGrabInteractable>();

        Debug.Log("Created slice piece: " + piece.name);
    }
}