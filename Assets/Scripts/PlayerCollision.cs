using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    // Start is called before the first frame update
    void OnTriggerEnter(Collider c) {
        if (c.gameObject.TryGetComponent<Throwable>(out Throwable t) && t.hurtsPlayer) {
            GameManager.Instance.Reputation -= 10;
            t.RemoveIngredient();
        }
    }
}
