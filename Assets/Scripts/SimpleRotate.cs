using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        float rotation = rotationSpeed * Time.deltaTime;
        transform.Rotate(new Vector3(0, rotation, 0));
    }
}
