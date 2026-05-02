using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnBound : MonoBehaviour
{
    void OnTriggerExit(Collider other) {
        // check if the object that left the kitchen has the tool respawn script
        if (other.TryGetComponent<ToolRespawn>(out ToolRespawn tool)) {
            tool.Respawn();
        }
    }
}
