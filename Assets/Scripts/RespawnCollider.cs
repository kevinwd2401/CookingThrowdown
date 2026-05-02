using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<ToolRespawn>(out ToolRespawn tool)) {
            tool.Respawn();
        }
    }
}
