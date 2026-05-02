using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolRespawn : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        // If the tool falls below -5 meters (the floor is usually at 0)
        if (transform.position.y < 1.75f) {
            Respawn();
        }
    }

    public void Respawn()
    {
        // reset physics 
        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // move back to starting pos/rot
        transform.position = startPosition;
        transform.rotation = startRotation;

        Debug.Log(gameObject.name + " has respawned!");
    }
}
