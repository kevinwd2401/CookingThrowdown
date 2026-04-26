using UnityEngine;

public class KnifeMeshSlicer : MonoBehaviour
{
    [Header("Slice Settings")]
    [SerializeField] private Transform sliceDirectionReference;
    [SerializeField] private float minSliceSpeed = 0.2f;
    [SerializeField] private float cooldown = 0.2f;

    private Vector3 lastPosition;
    private float currentSpeed;
    private float lastSliceTime;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Knife trigger touched: " + other.name);
        Debug.Log("Knife speed: " + currentSpeed);

        if (Time.time - lastSliceTime < cooldown)
        {
            Debug.Log("Slice blocked: cooldown");
            return;
        }

        if (currentSpeed < minSliceSpeed)
        {
            Debug.Log("Slice failed: knife too slow.");
            return;
        }

        MeshSliceableIngredient sliceable = other.GetComponent<MeshSliceableIngredient>();

        if (sliceable == null)
            sliceable = other.GetComponentInChildren<MeshSliceableIngredient>();

        if (sliceable == null)
            sliceable = other.GetComponentInParent<MeshSliceableIngredient>();

        if (sliceable == null)
        {
            Debug.Log("Slice failed: no MeshSliceableIngredient found.");
            return;
        }

        Vector3 slicePosition = transform.position;

        Vector3 sliceNormal = sliceDirectionReference != null
            ? sliceDirectionReference.up
            : transform.up;

        Debug.Log("Trying to slice: " + sliceable.name);

        sliceable.Slice(slicePosition, sliceNormal);

        lastSliceTime = Time.time;
    }
}