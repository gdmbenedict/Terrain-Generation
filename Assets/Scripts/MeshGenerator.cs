using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [Header("Mesh Generation Variables")]
    [SerializeField] private int xSize;
    [SerializeField] private int zSize;

    private Mesh mesh;

    private Vector3[] vertices;
    private int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        CreateMesh();
    }

    //Function that hadnles the entire process of creating a mesh
    private void CreateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    //function that handles creating the shape of the mesh
    private void CreateShape()
    {
        //declaring array size
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        //looping through points to create vertices
        int i = 0;
        for (int z=0; z<= zSize; z++)
        {
            for (int x=0; x<= xSize; x++)
            {
                vertices[i] = new Vector3(x, 0, z);
                i++;
            }
        }

        //declaring triangles array
        triangles = new int[xSize * zSize * 6];

        //creating indexes for the triangles array
        int vert = 0;
        int tris = 0;

        //looping through creating quads from triangles
        for (int z=0; z< zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                //*note: triangle vertices need to be put in a clockwise direction to generate triangle facing the correct direction

                triangles[tris + 0] = vert + 0; //bottom left point
                triangles[tris + 1] = vert + xSize + 1; //top right point
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + zSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }

            vert++; //used to stop generator from making mesh connection between rows
        }         
    }

    //Function that updates 
    private void UpdateMesh()
    {
        //clear previous data
        mesh.Clear();

        //update points
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //calculate normals for mesh
        mesh.RecalculateNormals();
    }

}
