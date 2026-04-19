using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveKnob : MonoBehaviour
{
    public Burner burner;

    public float snapSpeed = 250f;
    public bool isOn = false;
    private bool isRotating = false;
    private float targetAngle = 0f;

    void Update() {
        if (isRotating) {
            Quaternion targetRot = Quaternion.Euler(0, targetAngle, 0);
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                targetRot,
                snapSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.localRotation, targetRot) < 0.1f) {
                transform.localRotation = targetRot;
                isRotating = false;
            }
        }
    }

    public void OnRelease() {
        isOn = !isOn;
        targetAngle = isOn ? -90f : -180f;
        isRotating = true;
        if (burner != null) {
            burner.SetHeatState(isOn);
        }
    }
}
