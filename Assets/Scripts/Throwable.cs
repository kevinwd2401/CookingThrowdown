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

    [Header("Manual Player Throw")]
    [SerializeField] private float manualThrowMultiplier = 1.6f;
    [SerializeField] private float maxManualThrowSpeed = 18f;

    private Vector3 lastPosition;
    private Vector3 estimatedVelocity;
    private bool isHeld = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        ing = GetComponent<Ingredient>();

        // We apply throw velocity ourselves.
        grabInteractable.throwOnDetach = false;
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (isHeld)
        {
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            estimatedVelocity = (transform.position - lastPosition) / dt;
        }

        lastPosition = transform.position;
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
        // NPCThrowing controls gravity/velocity.
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

        isHeld = true;
        lastPosition = transform.position;
        estimatedVelocity = Vector3.zero;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }

        transform.SetParent(null);

        Debug.Log("Ingredient Grabbed");
        hurtsPlayer = false;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

            Vector3 throwVelocity = estimatedVelocity * manualThrowMultiplier;

            if (throwVelocity.magnitude > maxManualThrowSpeed)
                throwVelocity = throwVelocity.normalized * maxManualThrowSpeed;

            rb.velocity = throwVelocity;
        }
    }

    public void MagnetCatch(Transform hand)
    {
        hurtsPlayer = false;

        isHeld = true;
        lastPosition = transform.position;
        estimatedVelocity = Vector3.zero;

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