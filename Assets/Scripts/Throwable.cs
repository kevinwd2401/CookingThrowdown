using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Ingredient))]
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class Throwable : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Ingredient ing;

    public bool hurtsPlayer = false;

    [Header("Catch Settings")]
    [SerializeField] private float maxCatchDistance = 3.0f;

    [Header("Default XR Throw Settings")]
    [SerializeField] private float throwVelocityScale = 1.5f;
    [SerializeField] private float throwAngularVelocityScale = 1.0f;
    [SerializeField] private float throwSmoothingDuration = 0.18f;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        ing = GetComponent<Ingredient>();

        grabInteractable.throwOnDetach = true;
        grabInteractable.throwVelocityScale = throwVelocityScale;
        grabInteractable.throwAngularVelocityScale = throwAngularVelocityScale;
        grabInteractable.throwSmoothingDuration = throwSmoothingDuration;

        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    public void InitializeThrown()
    {
        hurtsPlayer = true;
    }

    void OnCollisionEnter(Collision c)
    {
        if (ing != null)
            ing.ThrowableOnCollide(c);

        if (c.gameObject.CompareTag("Floor"))
        {
            RemoveIngredient();
        }
        else
        {
            hurtsPlayer = false;
        }
    }

    public void RemoveIngredient()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.npcTransforms.Remove(this.transform);

        if (ps != null)
        {
            ps.transform.parent = null;
            ps.Play();
            Destroy(ps.gameObject, 3);
        }

        Destroy(gameObject);
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        float dist = Vector3.Distance(args.interactorObject.transform.position, transform.position);

        if (dist > maxCatchDistance)
        {
            if (args.interactorObject is IXRSelectInteractor interactor)
            {
                XRInteractionManager manager = grabInteractable.interactionManager != null
                    ? grabInteractable.interactionManager
                    : FindObjectOfType<XRInteractionManager>();

                if (manager != null)
                    manager.SelectExit(interactor, grabInteractable);
            }

            return;
        }

        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

        }

        hurtsPlayer = false;
        Debug.Log("Ingredient Grabbed");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

        }
    }

    public void MagnetCatch(Transform hand)
    {
        hurtsPlayer = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.None;
        }

        transform.position = hand.position;
        transform.rotation = hand.rotation;
        transform.SetParent(hand);
    }
}
