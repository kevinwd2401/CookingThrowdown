using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CuttingBoardPhysics : MonoBehaviour {
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private Coroutine settleCoroutine;

    public float velocityThreshold = 0.01f;
    public float settleTimeRequired = 0.5f;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // automatically listen for grab and release
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args) {
        // stop the settling check if we grab it mid-air
        if (settleCoroutine != null) StopCoroutine(settleCoroutine);

        // set to not kinematic
        rb.isKinematic = false;
    }

    void OnRelease(SelectExitEventArgs args) {
        // let gravity take over immediately when released
        rb.isKinematic = false;

        // start watching for it to stop moving
        settleCoroutine = StartCoroutine(CheckForSettle());
    }

    private IEnumerator CheckForSettle() {
        float timeStationary = 0f;

        while (true) {
            bool isMovingSlowly = rb.velocity.sqrMagnitude < velocityThreshold &&
                                  rb.angularVelocity.sqrMagnitude < velocityThreshold;

            if (isMovingSlowly) {
                // add to our timer
                timeStationary += Time.deltaTime;

                if (timeStationary >= settleTimeRequired) {
                    // set it to kinematic
                    rb.isKinematic = true;

                    // zero out forces
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    Debug.Log($"{gameObject.name} has settled and is now locked.");
                    yield break; // end the coroutine
                }
            } else {
                // it bounced or got bumped, reset the timer
                timeStationary = 0f;
            }

            yield return null;
        }
    }
}