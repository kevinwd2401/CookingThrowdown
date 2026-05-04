using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hater : MonoBehaviour
{
    public bool hostile = true;
    public int spawnIndex;
    public NPCManager manager;
    public Animator anim;
    private MoveState state;
    private Vector3 destination;

    [SerializeField] private int hitsUntilLeave = 1;
    [SerializeField] private NPCThrowing thrower;

    void Start()
    {
        StartCoroutine(StateUpdateCor());
    }

    void Update()
    {
        if (state == MoveState.Stay) return;

        Vector3 targetDirection = destination - transform.position;
        targetDirection.y = 0;

        if (targetDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }

        transform.position += Time.deltaTime * 3.2f * transform.forward;
    }

    public void Stun(int damage = 1)
    {
        if (hitsUntilLeave <= 0) return;

        hitsUntilLeave--;

        if (hitsUntilLeave == 0)
        {
            Leave(damage);
        }
    }
    public void Clap() {
        thrower.SetThrow(false);
        state = MoveState.Clapping;
        anim.SetTrigger("Clap");
    }

    public void SetRage(bool active) {
        thrower.SetRage(active);
    }

    private void Leave(int damage)
    {
        if (state == MoveState.Clapping) return;
        thrower.SetThrow(false);
        if (damage == 1 & Random.value > 0.7f) {
            StartCoroutine(DeathCor());
        } else {
            anim.SetBool("Walking", true);
            state = MoveState.MoveExit;
            destination = manager.entrancePoints[spawnIndex].position;
        }  
    }

    private IEnumerator DeathCor() {
        anim.SetTrigger("Die");
        yield return new WaitForSeconds(2);
        RemoveNPC();
    }

    private IEnumerator StateUpdateCor()
    {
        state = MoveState.MoveEnter;
        destination = manager.entrancePoints[spawnIndex].position;

        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (state == MoveState.Clapping) yield break;

            if (state == MoveState.Stay) continue;

            if (Vector3.Distance(transform.position, destination) < 1.5f)
            {
                if (state == MoveState.MoveEnter)
                {
                    state = MoveState.MoveStage;
                    Vector2 r = 4 * Random.insideUnitCircle;
                    destination = manager.crowdPoint.position + new Vector3(r.x, 0, 0.33f * r.y);
                }
                else if (state == MoveState.MoveStage)
                {
                    if (GameManager.Instance.gameEnded) {
                        state = MoveState.MoveExit;
                        destination = manager.entrancePoints[spawnIndex].position;
                    } else {
                        state = MoveState.Stay;
                        anim.SetBool("Walking", false);
                        thrower.SetThrow(true);
                    }
                }
                else if (state == MoveState.MoveExit)
                {
                    anim.SetBool("Walking", true);
                    state = MoveState.MoveSpawn;
                    destination = manager.spawnPoints[spawnIndex].position;
                }
                else if (state == MoveState.MoveSpawn)
                {
                    RemoveNPC();
                }
            }
        }
    }

    private void RemoveNPC() {
        GameManager.Instance.npcTransforms.Remove(transform);
        Destroy(gameObject);
    }
}

public enum MoveState
{
    MoveEnter,
    MoveStage,
    Stay,
    MoveExit,
    MoveSpawn,
    Clapping
}