using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public GameObject tilePrefab;
    public GameObject[] marblePrefabs;

    Tile[,] m_allTiles;
    Marble[,] m_allMarbles;

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width,height];
        m_allMarbles = new Marble[width,height];
        SetupTiles();
        SetupCamera();
        FillRandom();
    }

    void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;

                tile.name = "Tile (" + i + "," + j + ")";

                m_allTiles[i,j] = tile.GetComponent<Tile>();

                tile.transform.parent = transform;
                m_allTiles[i,j].Init(i,j,this);
            }
        }
    }

    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float) (width-1)/2f, (float) (height-1)/2f, -10f);

        float aspectRatio = Screen.width / (float) Screen.height;

        float verticalSize = height/2f + (float) borderSize;

        float horizontalSize = ((float) width/2f + (float) borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    GameObject GetRandomMarble()
    {
        int randomIndex = Random.Range(0, marblePrefabs.Length);

        if (marblePrefabs[randomIndex] == null)
        {
            Debug.LogWarning("BOARD: " + randomIndex + " does not contain a valid Marble prefab!");
        }

        return marblePrefabs[randomIndex];
    }

    void PlaceMarble(Marble marble, int x, int y)
    {
        if (marble == null)
        {
            Debug.LogWarning("BOARD: Invalid Marble!");
            return;
        }

        marble.transform.position = new Vector3(x, y, 0);
        marble.transform.rotation = Quaternion.identity;
        marble.SetCoordinates(x,y);
    }

    void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomMarble = Instantiate(GetRandomMarble(), Vector3.zero, Quaternion.identity) as GameObject;

                if (randomMarble != null)
                {
                    PlaceMarble(randomMarble.GetComponent<Marble>(), i, j);
                }
            }
        }
    }
}
