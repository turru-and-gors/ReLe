using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TrainingRoad : MonoBehaviour
{
    public int width = 18;
    public int length = 40;

    public Transform goStraightSprite;
    public Transform steerLeftSprite;
    public Transform steerRightSprite;

    private int[,] road;

    // =====> PUBLIC METHODS
    public int At(ulong row, ulong col)
    {
        return road[row, col];
    }
    

    // =====> MONO BEHAVIOR METHODS
    private void Awake()
    {
        road = new int[length, width];
        PopulateRoad();
        DrawRoad();
    }

    // =====> CUSTOM PRIVATE METHODS    

    // ===> INITIALIZATION
    public void PopulateRoad()
    {
        for(int i=0; i<length; i++)
        {
            for(int j=0; j<width; j++)
            {
                float value = Random.Range(-1f, 1f);
                if (value < -0.3f) road[i, j] = -1;
                else if (value > 0.3f) road[i, j] = 1;
                else road[i, j] = 0;
            }
        }
    }

    public void DrawRoad()
    {
        DestroyChildren();

        float Y = transform.position.y;
        for (int i = 0; i < length; i++)
        {            
            float X = transform.position.x - width / 2f;
            for (int j = 0; j < width; j++)
            {
                Transform tile;
                if (road[i, j] < 0)
                    tile = Instantiate(steerLeftSprite);
                else if (road[i, j] > 0)
                    tile = Instantiate(steerRightSprite);
                else
                    tile = Instantiate(goStraightSprite);
                tile.name = "tile_" + i.ToString() + "_" + j.ToString();
                tile.parent = transform;
                tile.position = new Vector2(X, Y);
                X++;
            }
            Y++;
        }
    }

    private void DestroyChildren()
    {
        foreach(Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
