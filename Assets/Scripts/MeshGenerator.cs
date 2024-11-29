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

    [Header("Falloff Variables")]
    [SerializeField] private bool applyFalloff;
    [SerializeField][Range(1,5)] private float falloffExponent;
    [SerializeField][Range(0, 10)] private float falloffMultiplier;
    private float[,] falloffMap;

    [Header("Terrain Visuals")]
    [SerializeField] private Gradient gradient;
    private float maxHeight;
    private float minHeight;

    [Header("Water")]
    [SerializeField] private GameObject waterVisual;
    [SerializeField][Range(-50, 0)] private float waterLevel;
    private Vector2 waterOffset;
    private GameObject waterInstance;

    private Mesh mesh;

    private Vector3[] vertices; //all coordinates for the texture
    private int[] triangles; //all the tris the mesh is made of
    private Color[] colors;

    // Start is called before the first frame update
    void Start()
    {
        GenerateTerrain();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateTerrain();
        }
    }

    public void GenerateTerrain()
    {
        CreateMesh();
        GenerateWater();
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

        //get max and min height;
        float persistanceFactor = 0;
        for (int i = 0; i < octaves; i++)
        {
            persistanceFactor += MathF.Pow(persistance, i);
            Debug.Log(persistanceFactor);
        }
        minHeight = -persistanceFactor;
        maxHeight = persistanceFactor;

        //generating falloff map for use
        falloffMap = GenerateFalloffMap();

        //looping through points to create vertices
        float vertexHeight = 0;
        for (int i = 0, z=0; z<= zSize; z++)
        {
            for (int x=0; x<= xSize; x++)
            {
                //vertex height generation
                vertexHeight = GenerateHeight(x,z) * verticalScale; //generating heigh according to perlin noise
                vertices[i] = new Vector3(x, vertexHeight, z); //assign vertices
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
                float height = Mathf.InverseLerp(minHeight * verticalScale, maxHeight * verticalScale, vertices[i].y); //get where on the scale this height is using inverse lerp (smallest = 0, largest = 1)
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

    //Function that converts a string to an int (for the purposes of creating a seed for generation
    private int StringToSeed(string input)
    {
        int intSeed = 0;
        for (int i =0; i<input.Length; i++)
        {
            intSeed += input[i] - 0;
        }

        return intSeed;
    }

    private float GenerateHeight(int posX, int posZ)
    {
        float height = 0; //height returned
        float frequency = 1f; //base frequency
        float amplitude = 1f; //base amplitude

        //bring down border
        if (posX == 0 || posZ == 0 || posX == xSize || posZ == zSize)
        {
            height = minHeight;
            //Debug.Log(minHeight);
            //Debug.Log(maxHeight);
        }
        //regular generation
        else
        {
            for (int i = 0; i < octaves; i++)
            {
                //get position of perlin noise sample
                float xCoord = (float)posX / xSize * frequency * perlinScale + offsets[i].x;
                float zCoord = (float)posZ / zSize * frequency * perlinScale + offsets[i].y;

                float perlinValue = Mathf.PerlinNoise(xCoord, zCoord) * 2 - 1; //get value on a scale of -1 tp 1
                height += perlinValue * amplitude; //add height from octave to result

                frequency *= lacunarity; //increase the frequency of changes with each octave
                amplitude *= persistance; //decrease magnitude of changes with each octave
            }
        }

        //applying falloff
        height -= falloffMap[posX, posZ];

        //applying bounds
        if (height < minHeight)
        {
            height = minHeight;
        }
        else if (height > maxHeight)
        {
            height = maxHeight;
        }

        return height;
    }

    private float[,] GenerateFalloffMap()
    {
        //declaring map
        float[,] falloffMap = new float[xSize + 1, zSize + 1];

        //loop through creation falloff
        for (int z=0; z<= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                //generate falloff for each side and determine which is closest
                float xFalloff = (float)x / xSize * 2 - 1;
                float zFalloff = (float)z / zSize * 2 - 1;
                float falloff = Mathf.Max(Mathf.Abs(zFalloff), Mathf.Abs(xFalloff));

                //Debug.Log(x + "," + z);
                //apply curve and store value
                falloffMap[x,z] = ApplyFalloffCurve(falloff);
            }
        }

        return falloffMap;
    }

    //function that applyes a falloff curve to the falloff generation
    private float ApplyFalloffCurve(float x)
    {
        return Mathf.Pow(x, falloffExponent) / (Mathf.Pow(x, falloffExponent) + Mathf.Pow(falloffMultiplier - x * falloffMultiplier, falloffExponent)); ;
    }

    //function that places and scales water
    private void GenerateWater()
    {
        if (waterInstance != null)
        {
            Destroy(waterInstance);
        }

        //find offset and instantiate object and parent to self
        waterOffset = new Vector2(xSize * 0.001f, zSize * 0.001f);
        waterInstance = Instantiate(waterVisual, gameObject.transform);

        //setting position
        Vector3 pos = new Vector3(waterOffset.x, minHeight * verticalScale, waterOffset.y);
        waterInstance.transform.position = pos;

        //scale wate visual
        float xScale = xSize - 2 * waterOffset.x;
        float yScale = waterLevel - minHeight * verticalScale;
        float zScale = zSize - 2 * waterOffset.y;
        waterInstance.transform.localScale = new Vector3(xScale, yScale, zScale);
    }


}
