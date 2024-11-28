using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [Header("Mesh Generation Variables")]
    [SerializeField] private int xSize;
    [SerializeField] private int zSize;

    [Header("Perlin Varaibles")]
    [SerializeField] private string stringSeed;
    [SerializeField][Range(1,100)] private float perlinScale;
    [SerializeField][Range(0,100)] private float offSetRange;
    private Vector2 offset;

    private Mesh mesh;

    private Vector3[] vertices;
    private int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        CreateMesh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateMesh();
        }
    }

    //Function that hadnles the entire process of creating a mesh
    public void CreateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //create mesh and update game
        CreateShape();
        UpdateMesh();
    }

    //function that handles creating the shape of the mesh
    private void CreateShape()
    {
        //randomly generate offset
        float xOffset = UnityEngine.Random.Range(-offSetRange, offSetRange);
        float yOffset = UnityEngine.Random.Range(-offSetRange, offSetRange);
        offset = new Vector2(xOffset, yOffset);

        //declaring array size
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        //looping through points to create vertices
        int i = 0;
        float vertexHeight = 0;
        for (int z=0; z<= zSize; z++)
        {
            for (int x=0; x<= xSize; x++)
            {
                vertexHeight = GenerateHeight(x,z);
                vertices[i] = new Vector3(x, vertexHeight, z);
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

    private int StringToSeed(string input)
    {
        int intSeed = 0;
        for (int i =0; i<input.Length; i++)
        {
            intSeed += input[i] - 0;
        }

        return intSeed;
    }

    private float GenerateHeight(float posX, float posZ)
    {
        float height = 0; //height returned
        float xCoord = posX / xSize * perlinScale + offset.x;
        float zCoord = posZ / zSize * perlinScale + offset.y;

        height = Mathf.PerlinNoise(xCoord, zCoord);

        return height;
    }
}
