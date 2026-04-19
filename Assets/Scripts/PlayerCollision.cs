using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerCollision : MonoBehaviour
{
    [Header("Catch Settings")]
    [SerializeField] private float catchRadius = 1.25f;   // bigger catch range
    [SerializeField] private LayerMask catchLayer;
    [SerializeField] private XRNode handNode = XRNode.RightHand;
    [SerializeField] private XRDirectInteractor directInteractor;

    private InputDevice device;
    private bool gripWasPressedLastFrame;

    void Start()
    {
        TryInitializeDevice();
    }

    void Update()
    {
        if (!device.isValid)
            TryInitializeDevice();

        bool gripPressed = false;
        if (device.isValid)
            device.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);

        // Detect grip press this frame
        if (gripPressed && !gripWasPressedLastFrame)
        {
            TryCatchNearest();
        }

        gripWasPressedLastFrame = gripPressed;
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.transform.parent != null &&
            c.transform.parent.gameObject.TryGetComponent<Throwable>(out Throwable t) &&
            t.hurtsPlayer)
        {
            GameManager.Instance.Reputation -= 10;
            t.RemoveIngredient();
            Debug.Log("ow");
        }
    }

    private void TryInitializeDevice()
    {
        device = InputDevices.GetDeviceAtXRNode(handNode);
    }

    private void TryCatchNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, catchRadius, catchLayer);

        Throwable closestThrowable = null;
        XRGrabInteractable closestGrab = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            Throwable t = hit.GetComponentInParent<Throwable>();
            if (t == null) continue;
            if (!t.hurtsPlayer) continue;

            XRGrabInteractable grab = t.GetComponent<XRGrabInteractable>();
            if (grab == null) continue;

            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestThrowable = t;
                closestGrab = grab;
            }
        }

        if (closestThrowable == null || closestGrab == null || directInteractor == null)
            return;

        XRInteractionManager manager = closestGrab.interactionManager;
        if (manager == null)
            manager = FindObjectOfType<XRInteractionManager>();

        if (manager == null)
        {
            Debug.LogWarning("No XRInteractionManager found.");
            return;
        }

        closestThrowable.hurtsPlayer = false;

        Rigidbody rb = closestThrowable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        closestThrowable.transform.position = Vector3.Lerp(
            closestThrowable.transform.position,
            transform.position,
            0.65f
        );

        manager.SelectEnter(
    (IXRSelectInteractor)directInteractor,
    (IXRSelectInteractable)closestGrab
);
        Debug.Log("Magnet catch: " + closestThrowable.name);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchRadius);
    }
}
