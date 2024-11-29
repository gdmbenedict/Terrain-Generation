using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private MeshGenerator meshGenerator;
    [SerializeField][Range(0,1)] private float distanceBuffer;
    [SerializeField][Range(0,1)] private float heightBuffer;

    // Update is called once per frame
    void Update()
    {
        
    }
}
