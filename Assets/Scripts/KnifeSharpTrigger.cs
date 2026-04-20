using UnityEngine;

public class KnifeSharpTrigger : MonoBehaviour
{
    [SerializeField] private Transform sharpDirectionReference;

    private KnifeCutter knife;

    private void Awake()
    {
        knife = GetComponentInParent<KnifeCutter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (knife == null) return;
        if (!knife.CanCut()) return;

        CuttableIngredient cuttable = other.GetComponentInParent<CuttableIngredient>();
        if (cuttable == null) return;

        Vector3 cutDirection;

        if (sharpDirectionReference != null)
            cutDirection = sharpDirectionReference.forward;
        else
            cutDirection = transform.forward;

        cuttable.TryCut(knife.GetKnifeSpeed(), cutDirection);
    }
}