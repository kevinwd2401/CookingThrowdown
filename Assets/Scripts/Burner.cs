using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burner : MonoBehaviour
{
    public bool isHot = false;
    public Material hotMat;
    public Material norMat;
    public MeshRenderer burnerRenderer;

    public void SetHeatState(bool isOn) {
        isHot = isOn;
        Debug.Log(gameObject.name + " is hot!");
        burnerRenderer.material = isHot ? hotMat : norMat;
    }
}
