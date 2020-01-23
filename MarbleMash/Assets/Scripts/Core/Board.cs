using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public GameObject tileNormalPrefab;
    public GameObject tileObstaclePrefab;
    public GameObject[] marblePrefabs;

    public float swapTime = 0.3f;

    Tile[,] m_allTiles;
    Marble[,] m_allMarbles;

    Tile m_clickedTile;
    Tile m_targetTile;

    bool m_playerInputEnabled = true;

    public StartingTile[] startingTiles;

    [System.Serializable]
    public class StartingTile
    {
        public GameObject tilePrefab;
        public int x;
        public int y;
        public int z;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width,height];
        m_allMarbles = new Marble[width,height];
        SetupTiles();
        SetupCamera();
        FillBoard(10, 0.5f);
    }

    void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null)
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + x + "," + y + ")";
            m_allTiles[x,y] = tile.GetComponent<Tile>();
            m_allTiles[x,y].Init(x,y,this);
            tile.transform.parent = transform;
        }
    }

    void SetupTiles()
    {
        foreach (StartingTile sTile in startingTiles)
        {
            if (sTile != null)
            {
                MakeTile(sTile.tilePrefab, sTile.x, sTile.y, sTile.z);
            }
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allTiles[i,j] == null)
                {
                    MakeTile(tileNormalPrefab, i, j);
                }
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

    Marble FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        GameObject randomMarble = Instantiate(GetRandomMarble(), Vector3.zero, Quaternion.identity) as GameObject;

        if (randomMarble != null)
        {
            randomMarble.GetComponent<Marble>().Init(this);
            PlaceMarble(randomMarble.GetComponent<Marble>(), x, y);

            if (falseYOffset != 0)
            {
                randomMarble.transform.position = new Vector3(x, y + falseYOffset, 0);
                randomMarble.GetComponent<Marble>().Move(x, y, moveTime);
            }

            return randomMarble.GetComponent<Marble>();
        }
        return null;
    }

    void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int iterations = 0;
        int maxIterations = 100;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allMarbles[i,j] == null && m_allTiles[i,j].tileType != TileType.Obstacle)
                {
                    Marble marble = FillRandomAt(i, j, falseYOffset, moveTime);

                    while (HasMatchOnFill(i, j))
                    {
                        ClearMarbleAt(i, j);
                        marble = FillRandomAt(i, j, falseYOffset, moveTime);

                        iterations++;
                        if (iterations >= maxIterations)
                        {
                            Debug.LogWarning("BOARD: Broke out of infinite while loop.");
                            break;
                        }
                    }
                }
            }
        }
    }

    bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<Marble> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<Marble> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
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
        if (m_playerInputEnabled)
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

                    yield return new WaitForSeconds(swapTime);
                }
                else
                {
                    ClearAndRefillBoard(clickedMarbleMatches.Union(targetMarbleMatches).ToList());
                }
            }
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

            if (nextMarble != null && nextMarble.matchValue == startMarble.matchValue && !matches.Contains(nextMarble))
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

    List<Marble> FindMatchesAt(List<Marble> marbles, int minLength = 3)
    {
        List<Marble> matches = new List<Marble>();

        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                matches = matches.Union(FindMatchesAt(marble.xIndex, marble.yIndex, minLength)).ToList();
            }
        }

        return matches;
    }

    List<Marble> FindAllMatches()
    {
        List<Marble> combinedMatches = new List<Marble>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<Marble> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }

        return combinedMatches;
    }

    void HighlightTileOff(int x, int y)
    {
        if (m_allTiles[x,y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    void HighlightTileOn(int x, int y, Color color)
    {
        if (m_allTiles[x,y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = color;
        }
    }


    void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);

        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (Marble marble in combinedMatches)
            {
                if (marble != null)
                {
                    HighlightTileOn(marble.xIndex, marble.yIndex, marble.color);
                }
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

    void HighlightMarbles(List<Marble> marbles)
    {
        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                HighlightTileOn(marble.xIndex, marble.yIndex, marble.color);
            }
        }
    }

    void ClearMarbleAt(int x, int y)
    {
        Marble marbleToClear = m_allMarbles[x,y];

        if (marbleToClear != null)
        {
            m_allMarbles[x,y] = null;
            Destroy(marbleToClear.gameObject);
        }

        //HighlightTileOff(x, y);
    }

    void ClearMarbleAt(List<Marble> marbles)
    {
        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                ClearMarbleAt(marble.xIndex, marble.yIndex);
                ParticleManager.Instance.ClearPieceFXAt(marble.xIndex, marble.yIndex);
            }
        }
    }

    void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = m_allTiles[x,y];
        if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            ParticleManager.Instance.BreakTileFXAt(tileToBreak.breakableValue, tileToBreak.xIndex, tileToBreak.yIndex);
            tileToBreak.BreakTile();
        }
    }

    void BreakTileAt(List<Marble> marbles)
    {
        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                BreakTileAt(marble.xIndex, marble.yIndex);
            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearMarbleAt(i, j);
            }
        }
    }

    List<Marble> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<Marble> movingMarbles = new List<Marble>();

        for (int i = 0; i < height - 1; i++)
        {
            if (m_allMarbles[column,i] == null && m_allTiles[column,i].tileType != TileType.Obstacle)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allMarbles[column, j] != null)
                    {
                        m_allMarbles[column, j].Move(column, i, collapseTime * (j - i));

                        m_allMarbles[column, i] = m_allMarbles[column, j];
                        m_allMarbles[column, i].SetCoordinates(column, i);

                        if (!movingMarbles.Contains(m_allMarbles[column, i]))
                        {
                            movingMarbles.Add(m_allMarbles[column, i]);
                        }

                        m_allMarbles[column, j] = null;

                        break;
                    }
                }
            }
        }

        return movingMarbles;
    }

    List<Marble> CollapseColumn(List<Marble> marbles)
    {
        List<Marble> movingMarbles = new List<Marble>();
        List<int> columnsToCollapse = GetColumns(marbles);

        foreach (int column in columnsToCollapse)
        {
            movingMarbles = movingMarbles.Union(CollapseColumn(column)).ToList();
        }

        return movingMarbles;
    }

    List<int> GetColumns(List<Marble> marbles)
    {
        List<int> columns = new List<int>();

        foreach (Marble marble in marbles)
        {
            if (!columns.Contains(marble.xIndex))
            {
                columns.Add(marble.xIndex);
            }
        }

        return columns;
    }

    void ClearAndRefillBoard(List<Marble> marbles)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(marbles));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<Marble> marbles)
    {
        m_playerInputEnabled = false;

        List<Marble> matches = marbles;
        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();
        }
        while (matches.Count != 0);

        m_playerInputEnabled = true;
    }

    IEnumerator ClearAndCollapseRoutine(List<Marble> marbles)
    {
        List<Marble> movingMarbles = new List<Marble>();
        List<Marble> matches = new List<Marble>();

        //HighlightMarbles(marbles);

        bool isFinished = false;
        while (!isFinished)
        {
            ClearMarbleAt(marbles);
            BreakTileAt(marbles);

            yield return new WaitForSeconds(0.25f);

            movingMarbles = CollapseColumn(marbles);

            while (!IsCollapsed(movingMarbles))
            {
                yield return null;
            }

            matches = FindMatchesAt(movingMarbles);

            if (matches.Count == 0)
            {
                isFinished = true;
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(10, 0.5f);
        yield return new WaitForSeconds(0.5f);
    }

    bool IsCollapsed(List<Marble> marbles)
    {
        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                if (marble.transform.position.y - (float) marble.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
