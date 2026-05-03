using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceInitializer : MonoBehaviour
{
    MeshRenderer mr;
    [SerializeField] Material[] faceMats;
    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        mr.material = faceMats[Random.Range(0, faceMats.Length)];
    }
}
