using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCThrowing : MonoBehaviour
{
    [SerializeField] Transform spawnPt;
    public Material[] tempMats;
    public AudioClip[] audioList;
    MeshRenderer mr;
    AudioSource audioSource;
    bool canThrow;
    void Start()
    {
        StartCoroutine(ThrowCor());
        mr = GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
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

    private IEnumerator ThrowCor()
    {
        yield return new WaitForSeconds(5f);

        while (true)
        {
            if (canThrow)
            {
                mr.material = tempMats[1];

                audioSource.clip = audioList[0];
                audioSource.Play();
                yield return new WaitForSeconds(1f);
                Vector3 r = Random.insideUnitSphere;
                r.y = 0f;

                Vector3 target = GameManager.Instance.playerTrans.position + new Vector3(r.x, 3.1f, r.z);

                ThrowAtPlayer(target, 11f);

                yield return new WaitForSeconds(0.2f);
                mr.material = tempMats[0];
            }

            yield return new WaitForSeconds(14f + 6f * Random.value);
        }
    }

    private void ThrowAtPlayer(Vector3 target, float speed = 11f)
    {
        if (GameManager.Instance.throwingList.Length == 0) return;

        GameObject prefab = GameManager.Instance.throwingList[
            Random.Range(0, GameManager.Instance.throwingList.Length)
        ];

        GameObject proj = Instantiate(prefab, spawnPt.position, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) return;

        proj.GetComponent<Throwable>().InitializeThrown();

        Vector3 dir = (target - spawnPt.position).normalized;
        dir.y += 0.1f; 
        dir.Normalize();

        rb.velocity = dir * speed;
    }

    public void SetThrow(bool throwing) {
        canThrow = throwing;
    }
}

