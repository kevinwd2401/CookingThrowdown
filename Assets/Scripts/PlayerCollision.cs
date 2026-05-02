using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerCollision : MonoBehaviour
{
    [Header("Sphere Catch Settings")]
    [SerializeField] private float catchRadius = 1.25f;
    [SerializeField] private LayerMask catchLayer;
    [SerializeField] private XRNode handNode = XRNode.RightHand;
    [SerializeField] private XRDirectInteractor directInteractor;
    [SerializeField] private Transform catchAttachPoint;

    [Header("Important")]
    [SerializeField] private bool onlyCatchThrownIngredients = true;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip splat;

    private InputDevice device;
    private bool gripWasPressedLastFrame;
    private bool triggerWasPressedLastFrame;

    private XRGrabInteractable currentlyHeld;
    private XRInteractionManager currentManager;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        TryInitializeDevice();
    }

    void Update()
    {
        if (!device.isValid)
            TryInitializeDevice();

        bool gripPressed = false;
        bool triggerPressed = false;

        if (device.isValid)
        {
            device.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);
            device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);
        }

        bool anyPressed = gripPressed || triggerPressed;
        bool anyWasPressed = gripWasPressedLastFrame || triggerWasPressedLastFrame;

        bool pressedThisFrame = anyPressed && !anyWasPressed;
        bool releasedThisFrame = !anyPressed && anyWasPressed;

        if (pressedThisFrame)
            TryCatchNearest();

        if (releasedThisFrame)
            ReleaseHeldObject();

        gripWasPressedLastFrame = gripPressed;
        triggerWasPressedLastFrame = triggerPressed;
    }

    void OnTriggerEnter(Collider c)
    {
        Throwable t = c.GetComponentInParent<Throwable>();

        if (t != null && t.hurtsPlayer)
        {
            GameManager.Instance.Reputation -= 10;
            t.RemoveIngredient();

            if (audioSource != null && splat != null)
                audioSource.PlayOneShot(splat, 1.0f);

            Debug.Log("ow");
        }
    }

    private void TryInitializeDevice()
    {
        device = InputDevices.GetDeviceAtXRNode(handNode);
    }

    private void TryCatchNearest()
    {
        if (directInteractor == null) return;

        // If the hand already grabbed something, don't sphere catch another.
        if (currentlyHeld != null || directInteractor.hasSelection)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, catchRadius, catchLayer);

        Throwable closestThrowable = null;
        XRGrabInteractable closestGrab = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            Throwable t = hit.GetComponentInParent<Throwable>();
            if (t == null) continue;

            // IMPORTANT:
            // Only sphere-catch ingredients thrown by NPC.
            // Ingredients sitting in pan usually have hurtsPlayer = false.
            if (onlyCatchThrownIngredients && !t.hurtsPlayer)
                continue;

            XRGrabInteractable grab = t.GetComponentInParent<XRGrabInteractable>();
            if (grab == null) continue;

            if (grab.isSelected) continue;

            float dist = Vector3.Distance(transform.position, t.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestThrowable = t;
                closestGrab = grab;
            }
        }

        if (closestThrowable == null || closestGrab == null)
            return;

        XRInteractionManager manager = closestGrab.interactionManager;
        if (manager == null)
            manager = FindObjectOfType<XRInteractionManager>();

        if (manager == null)
        {
            Debug.LogWarning("No XRInteractionManager found.");
            return;
        }

        currentlyHeld = closestGrab;
        currentManager = manager;

        closestThrowable.hurtsPlayer = false;

        Rigidbody rb = closestThrowable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Transform snapTarget = catchAttachPoint != null ? catchAttachPoint : transform;

        closestThrowable.transform.position = snapTarget.position;
        closestThrowable.transform.rotation = snapTarget.rotation;

        manager.SelectEnter(
            (IXRSelectInteractor)directInteractor,
            (IXRSelectInteractable)closestGrab
        );

        Debug.Log("Sphere catch nearest thrown ingredient only: " + closestThrowable.name);
    }

    private void ReleaseHeldObject()
    {
        if (currentlyHeld != null && currentManager != null && directInteractor != null)
        {
            currentManager.SelectExit(
                (IXRSelectInteractor)directInteractor,
                (IXRSelectInteractable)currentlyHeld
            );
        }

        currentlyHeld = null;
        currentManager = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchRadius);
    }
}