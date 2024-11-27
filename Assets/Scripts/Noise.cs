using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity)
    {
        //declaring map to store noise results
        float[,] noisemap = new float[mapWidth, mapHeight];

        //create random number generator to give offset to each octave
        Random.InitState(seed);

        //set starting offsets for octaves
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i =0; i<octaves; i++)
        {
            octaveOffsets[i] = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        }

        //handle invalid scale input;
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        //declaration for max heigh recording
        float maxheight = 0;
        float minheight = 0;

        //loop through map dimensions assigning noise values
        for (int y=0; y< mapHeight; y++)
        {
            for (int x=0; x<mapWidth; x++)
            {
                float amplitude = 1; //base applitude of height changes
                float frequency = 1; //base frequency of height changes
                float noiseHeigth = 0; //totaled height change

                for (int i =0; i<octaves; i++)
                {
                    Debug.Log(octaveOffsets[i]);
                    float valueX = x / scale * frequency + octaveOffsets[i].x; //value modified by scale and frequency and offset to get perlin values (integer values stay constant)
                    float valueY = y / scale * frequency + octaveOffsets[i].y; //value modified by scale and frequency and offset to get perlin values (integer values stay constant)

                    float perlinValue = Mathf.PerlinNoise(valueX, valueY) * 2 -1; //feed values into perlin noise function (end multiplactions change range to -1 to 1)
                    noiseHeigth += perlinValue * amplitude; //apply applitude to get weight differntiation between values

                    frequency *= lacunarity; //have frequency increase with each octave
                    amplitude *= persistance; //have amplitude decrese with each octave
                }

                //update min and max height
                if (noiseHeigth < minheight)
                {
                    minheight = noiseHeigth;
                }
                else if (noiseHeigth > maxheight)
                {
                    maxheight = noiseHeigth;
                }

                //save to noise map
                noisemap[x, y] = noiseHeigth;
            }
        }

        //set divisor to largest absolute value
        float divisor = maxheight > Mathf.Abs(minheight) ? maxheight: Mathf.Abs(minheight);

        //normalize the noise map;
        for (int y=0; y<mapHeight; y++)
        {
            for (int x=0; x<mapWidth; x++)
            {
                noisemap[x, y] /= divisor;
            }
        }

        //return the generated noise map
        return noisemap;
    }
}
