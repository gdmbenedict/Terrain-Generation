using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [Header("Mesh Generation Variables")]
    [SerializeField][Range(1,250)] private int xSize;
    [SerializeField][Range(1,250)] private int zSize;
    [SerializeField][Range(1,50)] private float verticalScale;

    [Header("Perlin Varaibles")]
    [SerializeField] private string stringSeed;
    [SerializeField][Range(1,100)] private float perlinScale;
    [SerializeField][Range(0,100)] private float offSetRange;
    [SerializeField][Range(1,10)] private int octaves;
    [SerializeField][Range(0.0001f, 1)] private float persistance;
    [SerializeField][Range(1,10)] private float lacunarity;
    private Vector2[] offsets;

    [Header("Terrain Visuals")]
    [SerializeField] private Gradient gradient;
    private float maxHeight;
    private float minHeight;

    private Mesh mesh;

    private Vector3[] vertices; //all coordinates for the texture
    private int[] triangles; //all the tris the mesh is made of
    private Color[] colors;

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
        //setting initial seed
        if (!String.IsNullOrEmpty(stringSeed))
        {
            UnityEngine.Random.InitState(StringToSeed(stringSeed));
        }

        //randomly generate offsets
        offsets = new Vector2[octaves];
        for (int i=0; i<octaves; i++)
        {
            float xOffset = UnityEngine.Random.Range(-offSetRange, offSetRange);
            float yOffset = UnityEngine.Random.Range(-offSetRange, offSetRange);
            offsets[i] = new Vector2(xOffset, yOffset);
        }
        
        //declaring array size
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        //looping through points to create vertices
        float vertexHeight = 0;
        for (int i = 0, z=0; z<= zSize; z++)
        {
            for (int x=0; x<= xSize; x++)
            {
                vertexHeight = GenerateHeight(x,z) * verticalScale; //generating heigh according to perlin noise
                vertices[i] = new Vector3(x, vertexHeight, z); //assign vertices
                i++;
            }
        }

        //get max and min height;
        float persistanceFactor = 0;
        for (int i=0; i<octaves; i++)
        {
            persistanceFactor += MathF.Pow(persistance, i); 
        }
        minHeight = -persistanceFactor * verticalScale;
        maxHeight = persistanceFactor * verticalScale;

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

                triangles[tris + 0] = vert + 0; //bottom left 
                triangles[tris + 1] = vert + xSize + 1; //top left
                triangles[tris + 2] = vert + 1; //bottom right
                triangles[tris + 3] = vert + 1; //bottom right
                triangles[tris + 4] = vert + xSize + 1; //top left
                triangles[tris + 5] = vert + xSize + 2; //top right

                vert++;
                tris += 6;
            }

            vert++; //used to stop generator from making mesh connection between rows
        }

        //Setting Colors
        colors = new Color[vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y); //get where on the scale this height is using inverse lerp (smallest = 0, largest = 1)
                colors[i] = gradient.Evaluate(height);
                i++;
            }
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
        mesh.colors = colors;

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
        float frequency = 1f; //base frequency
        float amplitude = 1f; //base amplitude

        for (int i=0; i<octaves; i++)
        {
            //get position of perlin noise sample
            float xCoord = posX / xSize * frequency * perlinScale + offsets[i].x;
            float zCoord = posZ / zSize * frequency * perlinScale + offsets[i].y;

            float perlinValue = Mathf.PerlinNoise(xCoord, zCoord) * 2 - 1; //get value on a scale of -1 tp 1
            height += perlinValue * amplitude; //add height from octave to result

            frequency *= lacunarity; //increase the frequency of changes with each octave
            amplitude *= persistance; //decrease magnitude of changes with each octave
        } 

        return height;
    }
}
