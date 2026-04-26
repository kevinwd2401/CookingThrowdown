using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCThrowing : MonoBehaviour
{
    [SerializeField] Transform spawnPt;
    public Material[] tempMats;
    public AudioClip[] audioList;
    public ParticleSystem ps;
    public Hater hater;
    MeshRenderer mr;
    AudioSource audioSource;
    bool canThrow, rage;
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
                if (rage) {
                    mr.material = tempMats[1];
                    if (ps != null && !ps.isPlaying) {
                        ps.Play();
                    }
                } else {
                    mr.material = tempMats[2];
                }

                // play audio
                int clip = Random.Range(0, audioList.Length);
                audioSource.clip = audioList[clip];
                audioSource.volume = 1.0f;

                if (clip == 1) {
                    audioSource.volume = 0.70f;
                }

                audioSource.Play();

                GameObject proj = SpawnThrowable();


                yield return new WaitForSeconds(1f);

                Vector3 target = GameManager.Instance.playerTrans.position + new Vector3(2f * Random.value - 1f, 1.0f, 0.1f * Random.value);

                if (proj != null)
                    ThrowAtPlayer(proj, target);

                yield return new WaitForSeconds(0.2f);

                if (!rage) 
                    mr.material = tempMats[0];
            }

            yield return new WaitForSeconds(rage ? (2f) : (16f + 16f * Random.value));
        }
    }

    public void SetRage(bool active) {
        rage = active;
        if (!active && ps != null) {
            ps.Stop();
        }
    }

    private GameObject SpawnThrowable() {
        if (GameManager.Instance.throwingList.Length == 0) return null;

        GameObject prefab = GameManager.Instance.throwingList[
            Random.Range(0, GameManager.Instance.throwingList.Length)
        ];

        GameObject proj = Instantiate(prefab, spawnPt.position, Quaternion.identity);
        Vector3 randomDir = Quaternion.AngleAxis(
            Random.Range(-20, 20),
            Random.onUnitSphere
        ) * Vector3.up;
        proj.transform.forward = randomDir;

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) return null;
        rb.useGravity = false;

        proj.GetComponent<Throwable>().InitializeThrown();

        return proj;
    }

    private void ThrowAtPlayer(GameObject proj, Vector3 target, float angleDeg = 55f)
    {
        Vector3 start = spawnPt.position;

        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        float x = toTargetXZ.magnitude;   // horizontal distance
        float y = toTarget.y;             // vertical difference
        float g = Mathf.Abs(Physics.gravity.y);

        float angle = angleDeg * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float denom = 2f * cos * cos * (x * Mathf.Tan(angle) - y);

        // Cannot hit target with this angle
        if (denom <= 0f) {
            Debug.Log("Cannot throw at this angle");
            return;
        }

        float v = Mathf.Sqrt((g * x * x) / denom);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        rb.useGravity = true;

        proj.GetComponent<Throwable>().InitializeThrown();

        Vector3 dir = toTargetXZ.normalized;

        Vector3 velocity =
            dir * (v * cos) +
            Vector3.up * (v * sin);

        rb.velocity = velocity;
    }

    public void SetThrow(bool throwing) {
        canThrow = throwing;
        if (!throwing) {
            mr.material = tempMats[0];
            SetRage(false);
        }
    }

    void OnTriggerEnter(Collider c)
    {
        Throwable t = c.GetComponentInParent<Throwable>();

        if (t != null && !t.hurtsPlayer)
        {
            Debug.Log("npc hit");
            hater.Stun();
            t.RemoveIngredient();
        }
    }
}

