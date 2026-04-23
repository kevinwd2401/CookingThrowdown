
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Ingredient))]
public class Throwable : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    public bool hurtsPlayer = false;
    Ingredient ing;
    void Start()
    {
        ing = GetComponent<Ingredient>();
    }

    public void InitializeThrown() {
        hurtsPlayer = true;
    }

    void OnCollisionEnter(Collision c) {
        ing.ThrowableOnCollide(c);
        if (c.gameObject.tag == "Floor") {
            RemoveIngredient();
        }
        else {
            hurtsPlayer = false;
        }
    }


    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
    }

    public void RemoveIngredient() {
        GameManager.Instance.npcTransforms.Remove(this.transform);
        Destroy(gameObject);
    }

    [SerializeField] private float maxCatchDistance = 3.0f;

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

        Debug.Log("Ingredient Grabbed");
        hurtsPlayer = false;
    }

    public void MagnetCatch(Transform hand)
    {
        hurtsPlayer = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        transform.position = hand.position;
        transform.rotation = hand.rotation;
        transform.SetParent(hand);
    }
}
