using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class StraightShotProjectile : MonoBehaviour
{
    private Rigidbody rb;
    private Coroutine flightRoutine;

    public void Shoot(Vector3 direction, float speed, float straightTime)
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("Shoot failed: no Rigidbody on " + gameObject.name);
            return;
        }

        direction = direction.normalized;

        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();

        rb.velocity = direction * speed;

        XRGrabInteractable grab = GetComponent<XRGrabInteractable>();

        Debug.Log(
            "STRAIGHT SHOT FIRED: " + gameObject.name +
            " | selected? " + (grab != null && grab.isSelected) +
            " | isKinematic = " + rb.isKinematic +
            " | velocity = " + rb.velocity
        );

        if (flightRoutine != null)
            StopCoroutine(flightRoutine);

        flightRoutine = StartCoroutine(EnableGravityLater(straightTime));
    }

    private IEnumerator EnableGravityLater(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null)
        {
            rb.useGravity = true;
            Debug.Log("Gravity restored after shot: " + gameObject.name);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rb != null)
            rb.useGravity = true;
    }
}