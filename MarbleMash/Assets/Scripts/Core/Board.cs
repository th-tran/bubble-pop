using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public GameObject tilePrefab;
    public GameObject[] marblePrefabs;

    public float swapTime = 0.3f;

    Tile[,] m_allTiles;
    Marble[,] m_allMarbles;

    Tile m_clickedTile;
    Tile m_targetTile;

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width,height];
        m_allMarbles = new Marble[width,height];
        SetupTiles();
        SetupCamera();
        FillRandom();
        HighlightMatches();
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

    public void PlaceMarble(Marble marble, int x, int y)
    {
        if (marble == null)
        {
            Debug.LogWarning("BOARD: Invalid Marble!");
            return;
        }

        marble.transform.position = new Vector3(x, y, 0);
        marble.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x,y))
        {
            m_allMarbles[x,y] = marble;
        }
        marble.transform.parent = transform;
        marble.SetCoordinates(x,y);
    }

    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
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
                    randomMarble.GetComponent<Marble>().Init(this);
                    PlaceMarble(randomMarble.GetComponent<Marble>(), i, j);
                }
            }
        }
    }

    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsAdjacent(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        m_clickedTile = null;
        m_targetTile = null;
    }

    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        Marble clickedMarble = m_allMarbles[clickedTile.xIndex, clickedTile.yIndex];
        Marble targetMarble = m_allMarbles[targetTile.xIndex, targetTile.yIndex];

        if (clickedMarble != null && targetMarble != null)
        {
            clickedMarble.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            targetMarble.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

            yield return new WaitForSeconds(swapTime);

            List<Marble> clickedMarbleMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            List<Marble> targetMarbleMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

            if (clickedMarbleMatches.Count == 0 && targetMarbleMatches.Count == 0)
            {
                clickedMarble.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                targetMarble.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            }

            yield return new WaitForSeconds(swapTime);

            HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            HighlightMatchesAt(targetTile.xIndex, targetTile.yIndex);
        }
    }

    bool IsAdjacent(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1  && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }

    List<Marble> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<Marble> matches = new List<Marble>();
        Marble startMarble = null;

        if (IsWithinBounds(startX, startY))
        {
            startMarble = m_allMarbles[startX, startY];
        }

        if (startMarble != null)
        {
            matches.Add(startMarble);
        }
        else
        {
            return new List<Marble>();
        }

        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int) Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int) Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            Marble nextMarble = m_allMarbles[nextX, nextY];

            if (nextMarble.matchValue == startMarble.matchValue && !matches.Contains(nextMarble))
            {
                matches.Add(nextMarble);
            }
            else
            {
                break;
            }
        }

        return (matches.Count >= minLength) ? matches : new List<Marble>();
    }

    List<Marble> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<Marble> upwardMatches = FindMatches(startX, startY, new Vector2(0,1), 2);
        List<Marble> downwardMatches = FindMatches(startX, startY, new Vector2(0,-1), 2);

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : new List<Marble>();
    }

    List<Marble> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<Marble> rightMatches = FindMatches(startX, startY, new Vector2(1,0), 2);
        List<Marble> leftMatches = FindMatches(startX, startY, new Vector2(-1,0), 2);

        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : new List<Marble>();
    }

    List<Marble> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<Marble> horizontalMatches = FindHorizontalMatches(x, y, minLength);
        List<Marble> verticalMatches = FindVerticalMatches(x, y, minLength);

        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }

    void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    void HighlightTileOn(int x, int y, Color color)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }


    void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);

        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (Marble marble in combinedMatches)
            {
                HighlightTileOn(marble.xIndex, marble.yIndex, marble.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }
}
