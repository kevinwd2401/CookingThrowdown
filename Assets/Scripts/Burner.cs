using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burner : MonoBehaviour
{
    private bool isHot = false;
    public Material hotMat;
    public Material norMat;
    public MeshRenderer burnerRenderer;

    public void SetHeatState(bool isOn) {
        isHot = isOn;
        burnerRenderer.material = isHot ? hotMat : norMat;
    }
}
