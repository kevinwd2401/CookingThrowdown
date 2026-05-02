using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MeshSliceableIngredient : MonoBehaviour
{
    [Header("Slice Settings")]
    [SerializeField] private Material crossSectionMaterial;
    [SerializeField] private int maxSliceCount = 3;

    [Header("Physics")]
    [SerializeField] private float fallbackPieceMass = 0.2f;

    private int sliceCount = 0;
    private bool isSlicing = false;

    public void Slice(Vector3 slicePosition, Vector3 sliceNormal)
    {
        if (isSlicing) return;
        if (sliceCount >= maxSliceCount) return;

        if (GetComponent<MeshFilter>() == null || GetComponent<MeshRenderer>() == null)
        {
            Debug.LogWarning(name + " cannot slice: missing MeshFilter or MeshRenderer.");
            return;
        }

        isSlicing = true;

        Vector3 originalWorldPos = transform.position;
        Quaternion originalWorldRot = transform.rotation;
        Vector3 originalWorldScale = transform.lossyScale;

        Ingredient originalIngredient = GetComponentInParent<Ingredient>();
        Throwable originalThrowable = GetComponentInParent<Throwable>();
        Rigidbody originalRb = GetComponentInParent<Rigidbody>();
        XRGrabInteractable originalGrab = GetComponentInParent<XRGrabInteractable>();
        AudioSource originalAudio = originalIngredient != null
            ? originalIngredient.GetComponent<AudioSource>()
            : GetComponentInParent<AudioSource>();

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

        foreach (GameObject piece in pieces)
        {
            SetupPiece(
                piece,
                originalWorldPos,
                originalWorldRot,
                originalWorldScale,
                originalIngredient,
                originalThrowable,
                originalRb,
                originalGrab,
                originalAudio
            );
        }

        if (originalIngredient != null)
            Destroy(originalIngredient.gameObject);
        else
            Destroy(gameObject);
    }

    private void SetupPiece(
        GameObject piece,
        Vector3 worldPos,
        Quaternion worldRot,
        Vector3 worldScale,
        Ingredient originalIngredient,
        Throwable originalThrowable,
        Rigidbody originalRb,
        XRGrabInteractable originalGrab,
        AudioSource originalAudio
    )
    {
        piece.transform.SetParent(null);
        piece.transform.position = worldPos;
        piece.transform.rotation = worldRot;
        piece.transform.localScale = worldScale;

        MeshCollider col = piece.GetComponent<MeshCollider>();
        if (col == null)
            col = piece.AddComponent<MeshCollider>();

        col.convex = true;
        col.isTrigger = false;

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb == null)
            rb = piece.AddComponent<Rigidbody>();

        if (originalRb != null)
        {
            rb.mass = originalRb.mass;
            rb.drag = originalRb.drag;
            rb.angularDrag = originalRb.angularDrag;
        }
        else
        {
            rb.mass = fallbackPieceMass;
        }

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.None;

        Ingredient newIngredient = piece.GetComponent<Ingredient>();
        if (newIngredient == null)
            newIngredient = piece.AddComponent<Ingredient>();

        if (originalIngredient != null)
        {
            newIngredient.cookProgress = originalIngredient.cookProgress;
            newIngredient.cookSpeed = originalIngredient.cookSpeed;

            newIngredient.isRotten = originalIngredient.isRotten;
            newIngredient.cookStatus = originalIngredient.cookStatus;
            newIngredient.ingredient = originalIngredient.ingredient;
            newIngredient.isSliced = true;
            
            newIngredient.cookedMat = originalIngredient.cookedMat;
            newIngredient.burntMat = originalIngredient.burntMat;
            newIngredient.ding = originalIngredient.ding;
            newIngredient.burn = originalIngredient.burn;
        }

        AudioSource newAudio = piece.GetComponent<AudioSource>();
        if (newAudio == null)
            newAudio = piece.AddComponent<AudioSource>();

        if (originalAudio != null)
        {
            newAudio.outputAudioMixerGroup = originalAudio.outputAudioMixerGroup;
            newAudio.playOnAwake = false;
            newAudio.loop = originalAudio.loop;
            newAudio.priority = originalAudio.priority;
            newAudio.volume = originalAudio.volume;
            newAudio.pitch = originalAudio.pitch;
            newAudio.spatialBlend = originalAudio.spatialBlend;
            newAudio.reverbZoneMix = originalAudio.reverbZoneMix;
        }

        XRGrabInteractable grab = piece.GetComponent<XRGrabInteractable>();
        if (grab == null)
            grab = piece.AddComponent<XRGrabInteractable>();

        if (originalGrab != null)
        {
            grab.movementType = originalGrab.movementType;
            grab.throwOnDetach = originalGrab.throwOnDetach;
            grab.throwVelocityScale = originalGrab.throwVelocityScale;
            grab.throwAngularVelocityScale = originalGrab.throwAngularVelocityScale;
            grab.throwSmoothingDuration = originalGrab.throwSmoothingDuration;
            grab.trackPosition = originalGrab.trackPosition;
            grab.trackRotation = originalGrab.trackRotation;
        }

        Throwable newThrowable = piece.GetComponent<Throwable>();
        if (newThrowable == null)
            newThrowable = piece.AddComponent<Throwable>();

        if (originalThrowable != null)
            newThrowable.hurtsPlayer = originalThrowable.hurtsPlayer;

        MeshSliceableIngredient newSliceable = piece.GetComponent<MeshSliceableIngredient>();
        if (newSliceable == null)
            newSliceable = piece.AddComponent<MeshSliceableIngredient>();

        newSliceable.crossSectionMaterial = crossSectionMaterial;
        newSliceable.maxSliceCount = maxSliceCount;
        newSliceable.sliceCount = sliceCount + 1;
        newSliceable.fallbackPieceMass = fallbackPieceMass;
    }
}