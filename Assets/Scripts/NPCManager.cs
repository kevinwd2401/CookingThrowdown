using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public GameObject npcPrefab;
    GameManager gm;
    private int counter;
    public Transform[] spawnPoints;
    public Transform[] entrancePoints;
    public Transform crowdPoint;

    // Start is called before the first frame update
    void Start()
    {
        gm = GetComponent<GameManager>();
        counter = 4;
        StartCoroutine(SpawnNPCs());
    }

    private IEnumerator SpawnNPCs() {
        yield return new WaitForSecondsRealtime(2);

        while (!gm.gameEnded) {
            counter = Mathf.Max(0, counter - 1);
            int index = Random.Range(0, 2);
            GameObject npc = Instantiate(npcPrefab, spawnPoints[index].position, Quaternion.identity);
            npc.GetComponent<Hater>().spawnIndex = index;
            npc.GetComponent<Hater>().manager = this;
            GameManager.Instance.npcTransforms.Add(npc.transform);
            yield return new WaitForSecondsRealtime(Random.value + 3 * (GameManager.Instance.Reputation / 100f) + counter + 8);
        }
    }
}
