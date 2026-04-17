using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public GameObject npcPrefab;
    GameManager gm;
    private int counter;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform[] entrancePoints;
    [SerializeField] Transform crowdPoint;

    // Start is called before the first frame update
    void Start()
    {
        gm = GetComponent<GameManager>();
        counter = 4;
        StartCoroutine(SpawnNPCs());
    }

    private IEnumerator SpawnNPCs() {
        yield return new WaitForSeconds(2);

        while (!gm.gameEnded) {
            counter = Mathf.Max(0, counter - 1);
            int index = Random.Range(0, 2);
            GameObject npc = Instantiate(npcPrefab, spawnPoints[index].position, Quaternion.identity);
            npc.GetComponent<Hater>().spawnIndex = index;
            GameManager.Instance.npcTransforms.Add(npc.transform);
            yield return new WaitForSeconds(Random.value + 3 * (GameManager.Instance.Reputation / 100f) + counter + 8);
        }
    }
}
