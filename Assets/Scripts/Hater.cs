using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hater : MonoBehaviour
{
    public bool hostile = true;
    public int spawnIndex;
    public NPCManager manager;
    private MoveState state;
    private Vector3 destination;

    //[SerializeField] bool stunned;

    [SerializeField] private int hitsUntilLeave = 1;
    [SerializeField] private NPCThrowing thrower;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StateUpdateCor());
    }

    // Update is called once per frame
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
        transform.position += Time.deltaTime * 4 * transform.forward;
    }

    public void Stun() {
        if (hitsUntilLeave <= 0) return;

        hitsUntilLeave--;
        if (hitsUntilLeave == 0) {
            Leave();
        }
    }

    void OnTriggerEnter(Collider c) {
        if (c.transform.parent != null && c.transform.parent.gameObject.TryGetComponent<Throwable>(out Throwable t) && !t.hurtsPlayer) {
            //stun NPC
            Debug.Log("npc hit");
            Stun();
            t.RemoveIngredient();
        }
    }
    private void Leave() {
        thrower.SetThrow(false);
        state = MoveState.MoveExit;
        destination = manager.entrancePoints[spawnIndex].position;
    }
    private IEnumerator StateUpdateCor() {
        state = MoveState.MoveEnter;
        destination = manager.entrancePoints[spawnIndex].position;
        while (true) {
            yield return new WaitForSeconds(0.5f);
            if (state == MoveState.Stay) continue;
            if (Vector3.Distance(transform.position, destination) < 1.5f) {
                if (state == MoveState.MoveEnter) {
                    state = MoveState.MoveStage;
                    Vector2 r = 4 * Random.insideUnitCircle;
                    destination = manager.crowdPoint.position + new Vector3(r.x, 0, r.y);
                }
                else if (state == MoveState.MoveStage) {
                    state = MoveState.Stay;
                    thrower.SetThrow(true);
                }
                else if (state == MoveState.MoveExit) {
                    state = MoveState.MoveSpawn;
                    destination = manager.spawnPoints[spawnIndex].position;
                }
                else if (state == MoveState.MoveSpawn) {
                    Destroy(gameObject);
                }
            }
        }
    }
}

public enum MoveState 
{
    MoveEnter,
    MoveStage,
    Stay,
    MoveExit,
    MoveSpawn
}
