using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCThrowing : MonoBehaviour
{
    [SerializeField] Transform spawnPt;
    public Material[] tempMats;
    MeshRenderer mr;
    bool canThrow;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ThrowCor());
        mr = GetComponent<MeshRenderer>();
        tempMats[0] = mr.material;
    }
    void Update() {

        if (canThrow)
        {
            Vector3 targetDirection;
            Vector3 toPlayer = GameManager.Instance.playerTrans.position - transform.position;
            targetDirection = new Vector3(toPlayer.x, 0f, toPlayer.z);

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 5f
                );
            }
        }
        else
        {
            transform.rotation = transform.parent.rotation;
        }
    }

    private IEnumerator ThrowCor() {
        yield return new WaitForSeconds(5);
        while (true) {
            if (canThrow) {
                mr.material = tempMats[1];
                yield return new WaitForSeconds(1);
                Vector3 r = Random.insideUnitSphere;
                r.y = 0;
                ThrowAtPlayer(GameManager.Instance.playerTrans.position + r + 0.1f * Vector3.up);
                yield return new WaitForSeconds(0.2f);
                mr.material = tempMats[0];
            }
            yield return new WaitForSeconds(8 + 5 *  Random.value);
        }

    }

    private void ThrowAtPlayer(Vector3 target, float speed = 12)
    {

        Vector3 start = spawnPt.position;

        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        float x = toTargetXZ.magnitude;   // horizontal distance
        float y = toTarget.y;             // vertical difference
        float g = Mathf.Abs(Physics.gravity.y);
        float v = speed;

        float v2 = v * v;
        float discriminant = v2 * v2 - g * (g * x * x + 2f * y * v2);

        // Target cannot be hit with this speed
        if (discriminant < 0f) {
            Debug.Log("Cannot throw"); 
            return;
        }

        if (GameManager.Instance.throwingList.Length == 0) return;

        GameObject prefab = GameManager.Instance.throwingList[Random.Range(0, GameManager.Instance.throwingList.Length)];
        GameObject proj = Instantiate(prefab, spawnPt.position, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) return;
        proj.GetComponent<Throwable>().InitializeThrown();

        float sqrt = Mathf.Sqrt(discriminant);

        float tanTheta = (Random.value > 0.25f) // use high arc?
            ? (v2 + sqrt) / (g * x)
            : (v2 - sqrt) / (g * x);

        float angle = Mathf.Atan(tanTheta);

        Vector3 dir = toTargetXZ.normalized;

        Vector3 velocity =
            dir * (v * Mathf.Cos(angle)) +
            Vector3.up * (v * Mathf.Sin(angle));

        rb.velocity = velocity;
    }

    public void SetThrow(bool throwing) {
        canThrow = throwing;
    }
}
