using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Interactor Tracking")]
    [Tooltip("Optional. Drag your Near-Far Interactor here if direct catch still does not shoot.")]
    [SerializeField] private XRBaseInteractor[] extraHandInteractors;

    [Header("Catch Rules")]
    [SerializeField] private bool onlyCatchThrownIngredients = true;

    [Header("Mode Toggle")]
    [SerializeField] private bool allowAButtonToggleOnThisHand = true;
    [SerializeField] private bool aimShootMode = false;

    [Header("Mode UI")]
    [SerializeField] private TextMeshProUGUI modeText;

    [Header("Aim And Shoot Settings")]
    [SerializeField] private float shootSpeed = 8f;
    [SerializeField] private float straightFlightTime = 1.2f;
    [SerializeField] private float aimDistance = 8f;
    [SerializeField] private float aimStartOffset = 0.25f;
    [SerializeField] private LayerMask aimHitLayer = ~0;
    [SerializeField] private Transform aimSource;
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private Transform aimReticle;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip splat;

    private InputDevice device;
    private bool gripWasPressedLastFrame;
    private bool triggerWasPressedLastFrame;
    private bool aWasPressedLastFrame;

    private XRGrabInteractable currentlyHeld;
    private XRInteractionManager currentManager;
    private bool originalThrowOnDetach = true;
    private bool hasStoredThrowSetting = false;

    private readonly List<XRBaseInteractor> handInteractors = new List<XRBaseInteractor>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        TryInitializeDevice();

        CacheHandInteractors();

        SetupAimVisuals();
        HideAimVisuals();
        UpdateModeUI();
    }

    void OnDestroy()
    {
        UnregisterCurrentHeld();
    }

    void Update()
    {
        if (!device.isValid)
            TryInitializeDevice();

        bool gripPressed = false;
        bool triggerPressed = false;
        bool aPressed = false;

        if (device.isValid)
        {
            device.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);
            device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);
            device.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed);
        }

        if (allowAButtonToggleOnThisHand && handNode == XRNode.RightHand)
        {
            bool aPressedThisFrame = aPressed && !aWasPressedLastFrame;

            if (aPressedThisFrame)
            {
                aimShootMode = !aimShootMode;
                Debug.Log("Aim/Shoot Mode: " + aimShootMode);
                UpdateModeUI();
            }
        }

        bool anyPressed = gripPressed || triggerPressed;
        bool anyWasPressed = gripWasPressedLastFrame || triggerWasPressedLastFrame;

        bool pressedThisFrame = anyPressed && !anyWasPressed;
        bool releasedThisFrame = !anyPressed && anyWasPressed;

        RememberAnyHeldIngredient();
        UpdateHeldThrowMode();

        if (pressedThisFrame)
            TryCatchNearest();

        if (currentlyHeld != null && aimShootMode)
            UpdateAimVisuals();
        else
            HideAimVisuals();

        if (releasedThisFrame)
            ReleaseHeldObject();

        gripWasPressedLastFrame = gripPressed;
        triggerWasPressedLastFrame = triggerPressed;
        aWasPressedLastFrame = aPressed;
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

    private void CacheHandInteractors()
    {
        handInteractors.Clear();

        XRBaseInteractor[] found = GetComponentsInChildren<XRBaseInteractor>(true);
        foreach (XRBaseInteractor interactor in found)
        {
            if (interactor != null && !handInteractors.Contains(interactor))
                handInteractors.Add(interactor);
        }

        if (directInteractor != null && !handInteractors.Contains(directInteractor))
            handInteractors.Add(directInteractor);

        if (extraHandInteractors != null)
        {
            foreach (XRBaseInteractor interactor in extraHandInteractors)
            {
                if (interactor != null && !handInteractors.Contains(interactor))
                    handInteractors.Add(interactor);
            }
        }
    }

    private void RememberAnyHeldIngredient()
    {
        if (currentlyHeld != null && currentlyHeld.isSelected)
            return;

        foreach (XRBaseInteractor interactor in handInteractors)
        {
            if (interactor == null) continue;
            if (!interactor.hasSelection) continue;

            foreach (IXRSelectInteractable selected in interactor.interactablesSelected)
            {
                XRGrabInteractable grab = selected as XRGrabInteractable;
                if (grab == null) continue;

                // Only control shoot/throw mode for ingredients.
                if (grab.GetComponentInParent<Throwable>() == null) continue;

                RegisterHeld(grab);
                return;
            }
        }
    }

    private void RegisterHeld(XRGrabInteractable grab)
    {
        if (grab == null) return;
        if (currentlyHeld == grab) return;

        UnregisterCurrentHeld();

        currentlyHeld = grab;
        currentManager = grab.interactionManager;

        if (currentManager == null)
            currentManager = FindObjectOfType<XRInteractionManager>();

        originalThrowOnDetach = grab.throwOnDetach;
        hasStoredThrowSetting = true;

        currentlyHeld.selectExited.AddListener(OnHeldIngredientReleased);

        UpdateHeldThrowMode();
    }

    private void UnregisterCurrentHeld()
    {
        if (currentlyHeld != null)
        {
            currentlyHeld.selectExited.RemoveListener(OnHeldIngredientReleased);

            if (hasStoredThrowSetting)
                currentlyHeld.throwOnDetach = originalThrowOnDetach;
        }

        currentlyHeld = null;
        currentManager = null;
        hasStoredThrowSetting = false;
    }

    private void UpdateHeldThrowMode()
    {
        if (currentlyHeld == null) return;

        // In shoot mode, disable XR's normal throw before release.
        // In normal mode, restore XR default throw.
        if (aimShootMode)
            currentlyHeld.throwOnDetach = false;
        else
            currentlyHeld.throwOnDetach = originalThrowOnDetach;
    }

    private void OnHeldIngredientReleased(SelectExitEventArgs args)
    {
        XRGrabInteractable grab = args.interactableObject as XRGrabInteractable;
        if (grab == null) return;

        bool shouldShoot = aimShootMode && grab.GetComponentInParent<Throwable>() != null;

        if (shouldShoot)
        {
            Vector3 shootDirection = GetAimDirection();
            StartCoroutine(ShootAfterRelease(grab.gameObject, shootDirection));
        }

        if (grab == currentlyHeld)
            UnregisterCurrentHeld();

        HideAimVisuals();
    }

    private void TryCatchNearest()
    {
        if (directInteractor == null) return;

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

        RegisterHeld(closestGrab);

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

        Debug.Log("Sphere catch: " + closestThrowable.name);
    }

    private void ReleaseHeldObject()
    {
        if (currentlyHeld == null)
            return;

        XRGrabInteractable grab = currentlyHeld;

        if (!grab.isSelected)
            return;

        IXRSelectInteractor realInteractor = null;

        if (grab.interactorsSelecting != null && grab.interactorsSelecting.Count > 0)
            realInteractor = grab.interactorsSelecting[0];

        if (realInteractor == null && directInteractor != null)
            realInteractor = (IXRSelectInteractor)directInteractor;

        XRInteractionManager manager = grab.interactionManager;
        if (manager == null)
            manager = currentManager;

        if (manager == null)
            manager = FindObjectOfType<XRInteractionManager>();

        if (manager != null && realInteractor != null)
        {
            manager.SelectExit(
                realInteractor,
                (IXRSelectInteractable)grab
            );
        }
    }

    private IEnumerator ShootAfterRelease(GameObject obj, Vector3 shootDirection)
    {
        if (obj == null)
            yield break;

        yield return null;
        yield return new WaitForFixedUpdate();

        if (obj == null)
            yield break;

        StraightShotProjectile shot = obj.GetComponent<StraightShotProjectile>();
        if (shot == null)
            shot = obj.AddComponent<StraightShotProjectile>();

        shot.Shoot(shootDirection, shootSpeed, straightFlightTime);

        Debug.Log("Aim shoot on release: " + obj.name);
    }

    private Transform GetAimTransform()
    {
        if (aimSource != null)
            return aimSource;

        if (catchAttachPoint != null)
            return catchAttachPoint;

        if (directInteractor != null)
            return directInteractor.transform;

        return transform;
    }

    private Vector3 GetAimDirection()
    {
        return GetAimTransform().forward.normalized;
    }

    private Vector3 GetAimOrigin()
    {
        Transform aimTransform = GetAimTransform();
        return aimTransform.position + aimTransform.forward.normalized * aimStartOffset;
    }

    private bool IsHitHeldObject(RaycastHit hit)
    {
        if (currentlyHeld == null) return false;

        return hit.collider.GetComponentInParent<XRGrabInteractable>() == currentlyHeld;
    }

    private void SetupAimVisuals()
    {
        if (aimLine == null)
        {
            GameObject lineObj = new GameObject("Aim Line");
            lineObj.transform.SetParent(transform);

            aimLine = lineObj.AddComponent<LineRenderer>();
            aimLine.positionCount = 2;
            aimLine.startWidth = 0.02f;
            aimLine.endWidth = 0.02f;
            aimLine.useWorldSpace = true;

            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = Color.green;
            aimLine.material = mat;
        }

        if (aimReticle == null)
        {
            GameObject reticleObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reticleObj.name = "Aim Reticle";

            Collider col = reticleObj.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            reticleObj.transform.localScale = Vector3.one * 0.12f;

            Renderer r = reticleObj.GetComponent<Renderer>();
            if (r != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.red;
                r.material = mat;
            }

            aimReticle = reticleObj.transform;
        }
    }

    private void UpdateAimVisuals()
    {
        Vector3 origin = GetAimOrigin();
        Vector3 direction = GetAimDirection();
        Vector3 endPoint = origin + direction * aimDistance;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction,
            aimDistance,
            aimHitLayer,
            QueryTriggerInteraction.Ignore
        );

        float closestDist = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            if (IsHitHeldObject(hit))
                continue;

            if (hit.distance < closestDist)
            {
                closestDist = hit.distance;
                endPoint = hit.point;
            }
        }

        if (aimLine != null)
        {
            aimLine.enabled = true;
            aimLine.SetPosition(0, origin);
            aimLine.SetPosition(1, endPoint);
        }

        if (aimReticle != null)
        {
            aimReticle.gameObject.SetActive(true);
            aimReticle.position = endPoint;
        }
    }

    private void HideAimVisuals()
    {
        if (aimLine != null)
            aimLine.enabled = false;

        if (aimReticle != null)
            aimReticle.gameObject.SetActive(false);
    }

    private void UpdateModeUI()
    {
        if (modeText == null) return;

        if (aimShootMode)
        {
            modeText.text = "Mode: Shoot";
            modeText.color = Color.green;
        }
        else
        {
            modeText.text = "Mode: Throw";
            modeText.color = Color.red;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchRadius);
    }
}