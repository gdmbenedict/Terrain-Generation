using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private MeshGenerator meshGenerator;
    [SerializeField] private float moveTime = 1;
    [SerializeField] private float vertMult = 3;
    [SerializeField][Range(0,1)] private float distanceBuffer;
    [SerializeField][Range(0,1)] private float heightBuffer;
    private float zPos;
    private float yPos;
    private Vector3 destination;
    private Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        zPos = -meshGenerator.GetSize() * (1 + distanceBuffer);
        yPos = meshGenerator.GetHeight() * (1 + heightBuffer) * vertMult;
        destination = new Vector3(0f, yPos, zPos);

        transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, moveTime);
        transform.LookAt(Vector3.zero);
    }
}
