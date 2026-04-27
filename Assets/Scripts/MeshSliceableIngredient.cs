using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MeshSliceableIngredient : MonoBehaviour
{
    [Header("Slice Settings")]
    [SerializeField] private Material crossSectionMaterial;
    [SerializeField] private int maxSliceCount = 3;

    [Header("Physics")]
    [SerializeField] private float pieceMass = 0.2f;

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

        Vector3 originalWorldPos = transform.position;
        Quaternion originalWorldRot = transform.rotation;
        Vector3 originalWorldScale = transform.lossyScale;

        Ingredient originalIngredient = GetComponentInParent<Ingredient>();
        Throwable originalThrowable = GetComponentInParent<Throwable>();
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

        Debug.Log("Slice success. Pieces created: " + pieces.Length);

        foreach (GameObject piece in pieces)
        {
            SetupPiece(
                piece,
                originalWorldPos,
                originalWorldRot,
                originalWorldScale,
                originalIngredient,
                originalThrowable,
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

        rb.mass = pieceMass;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Ingredient newIngredient = piece.GetComponent<Ingredient>();
        if (newIngredient == null)
            newIngredient = piece.AddComponent<Ingredient>();

        if (originalIngredient != null)
        {
            newIngredient.cookProgress = originalIngredient.cookProgress;
            newIngredient.cookSpeed = originalIngredient.cookSpeed;
            newIngredient.cookStatus = originalIngredient.cookStatus;
            newIngredient.isRotten = originalIngredient.isRotten;
            newIngredient.ingredientId = originalIngredient.ingredientId;

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
            newAudio.mute = originalAudio.mute;
            newAudio.bypassEffects = originalAudio.bypassEffects;
            newAudio.bypassListenerEffects = originalAudio.bypassListenerEffects;
            newAudio.bypassReverbZones = originalAudio.bypassReverbZones;
            newAudio.playOnAwake = false;
            newAudio.loop = originalAudio.loop;
            newAudio.priority = originalAudio.priority;
            newAudio.volume = originalAudio.volume;
            newAudio.pitch = originalAudio.pitch;
            newAudio.panStereo = originalAudio.panStereo;
            newAudio.spatialBlend = originalAudio.spatialBlend;
            newAudio.reverbZoneMix = originalAudio.reverbZoneMix;
        }

        if (piece.GetComponent<XRGrabInteractable>() == null)
            piece.AddComponent<XRGrabInteractable>();

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
        newSliceable.pieceMass = pieceMass;

        Debug.Log("Created visible slice piece: " + piece.name + " at " + piece.transform.position);
    }
}