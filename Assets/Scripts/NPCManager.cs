using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NPCManager : MonoBehaviour
{
    public GameObject npcPrefab;
    GameManager gm;
    private int counter;
    public Transform[] spawnPoints;
    public Transform[] entrancePoints;
    public Transform crowdPoint;
    private Hater chosenOne;

    // Start is called before the first frame update
    void Start()
    {
        gm = GetComponent<GameManager>();
        counter = 8;
        StartCoroutine(SpawnNPCs());
    }

    private IEnumerator SpawnNPCs() {
        yield return new WaitForSecondsRealtime(2);

        while (!gm.gameEnded) {
            if (GameManager.Instance.npcTransforms.Count < 20) {
                counter = Mathf.Max(0, counter - 1);
                int index = Random.Range(0, 2);
                GameObject npc = Instantiate(npcPrefab, spawnPoints[index].position, Quaternion.identity);
                npc.GetComponent<Hater>().spawnIndex = index;
                npc.GetComponent<Hater>().manager = this;
                GameManager.Instance.npcTransforms.Add(npc.transform);
            }
            yield return new WaitForSecondsRealtime(Random.value + (counter/2) + 8);

            if (GameManager.Instance.npcTransforms.Count > 3 && Random.value > 0.4f + counter * 0.05f) {
                var elements = GameManager.Instance.npcTransforms.ToArray();
                chosenOne = elements[Random.Range(0, elements.Length)].gameObject.GetComponent<Hater>();
                chosenOne.SetRage(true);
            }

            yield return new WaitForSecondsRealtime(3 * (GameManager.Instance.Reputation / 100f));
        }
    }
}
