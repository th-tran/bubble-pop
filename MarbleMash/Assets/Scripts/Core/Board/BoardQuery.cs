using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardQuery : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public GameObject GetRandomObject(GameObject[] objectArray)
    {
        if (m_board == null)
        {
            return null;
        }

        int randomIndex = Random.Range(0, objectArray.Length);
        if (objectArray[randomIndex] == null)
        {
            Debug.LogWarning("ERROR: BOARD.GetRandomObject at index " + randomIndex + "does not contain a valid GameObject!");
        }

        return objectArray[randomIndex];
    }

    public GameObject GetRandomMarble()
    {
        if (m_board == null)
        {
            return null;
        }

        return GetRandomObject(m_board.marblePrefabs);
    }

    public GameObject GetRandomBlocker()
    {
        if (m_board == null)
        {
            return null;
        }

        return GetRandomObject(m_board.blockerPrefabs);
    }

    public List<int> GetColumns(List<Marble> marbles)
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

    public List<Marble> GetRowMarbles(int row)
    {
        List<Marble> marbles = new List<Marble>();

        for (int i = 0; i < m_board.width; i++)
        {
            if (m_board.allMarbles[i, row] != null)
            {
                marbles.Add(m_board.allMarbles[i, row]);
            }
        }

        return marbles;
    }

    public List<Marble> GetColumnMarbles(int column)
    {
        List<Marble> marbles = new List<Marble>();

        for (int i = 0; i < m_board.height; i++)
        {
            if (m_board.allMarbles[column, i] != null)
            {
                marbles.Add(m_board.allMarbles[column, i]);
            }
        }

        return marbles;
    }

    public List<Marble> GetAdjacentMarbles(int x, int y, int offset = 1)
    {
        List<Marble> marbles = new List<Marble>();

        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if (IsWithinBounds(i,j))
                {
                    marbles.Add(m_board.allMarbles[i, j]);
                }
            }
        }

        return marbles;
    }

    public List<Marble> GetBombedMarbles(List<Marble> marbles)
    {
        List<Marble> allMarblesToClear = new List<Marble>();

        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                List<Marble> marblesToClear = new List<Marble>();

                Bomb bomb = marble.GetComponent<Bomb>();

                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            marblesToClear = GetColumnMarbles(bomb.xIndex);
                            break;
                        case BombType.Row:
                            marblesToClear = GetRowMarbles(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            marblesToClear = GetAdjacentMarbles(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:
                            // TODO: Destroy all marbles of a random color
                            break;
                        default:
                            break;
                    }
                }

                allMarblesToClear = allMarblesToClear.Union(marblesToClear).ToList();
                allMarblesToClear = RemoveBlockers(allMarblesToClear);
            }
        }

        return allMarblesToClear;
    }

    public bool IsColorBomb(Marble marble)
    {
        Bomb bomb = marble.GetComponent<Bomb>();

        if (bomb != null)
        {
            return (bomb.bombType == BombType.Color);
        }

        return false;
    }

    public bool IsCornerMatch(List<Marble> marbles)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                if (xStart == -1 || yStart == -1)
                {
                    xStart = marble.xIndex;
                    yStart = marble.yIndex;
                    continue;
                }

                if (marble.xIndex != xStart && marble.yIndex == yStart)
                {
                    horizontal = true;
                }

                if (marble.xIndex == xStart && marble.yIndex != yStart)
                {
                    vertical = true;
                }
            }
        }

        return (horizontal && vertical);
    }

    public bool IsWithinBounds(int x, int y)
    {
        if (m_board == null)
        {
            return false;
        }

        return (x >= 0 && x < m_board.width && y >= 0 && y < m_board.height);
    }

    public bool IsNextTo(Tile start, Tile end)
    {
        return (Mathf.Abs(start.xIndex - end.xIndex) + Mathf.Abs(start.yIndex - end.yIndex) == 1);
    }

    public bool IsCollapsed(List<Marble> marbles)
    {
        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                if (marble.transform.position.y - (float)marble.yIndex > 0.001f)
                {
                    return false;
                }

                if (marble.transform.position.x - (float)marble.xIndex > 0.001f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        // Find matches to the left
        List<Marble> leftMatches = m_board.boardMatcher.FindMatches(x, y, new Vector2(-1, 0), minLength);
        // Find matches downward
        List<Marble> downwardMatches = m_board.boardMatcher.FindMatches(x, y, new Vector2(0, -1), minLength);

        // Return whether matches were found
        return (leftMatches.Count > 0 || downwardMatches.Count > 0);

    }

    public List<Blocker> FindBlockersAt(int row, bool clearedAtBottomOnly = false)
    {
        List<Blocker> foundBlockers = new List<Blocker>();

        for (int i = 0; i < m_board.width; i++)
        {
            if (m_board.allMarbles[i,row] != null)
            {
                Blocker blockerComponent = m_board.allMarbles[i,row].GetComponent<Blocker>();

                if (blockerComponent != null)
                {
                    if (!clearedAtBottomOnly || (clearedAtBottomOnly && blockerComponent.clearedAtBottom))
                    {
                        foundBlockers.Add(blockerComponent);
                    }
                }
            }
        }

        return foundBlockers;
    }

    public List<Blocker> FindAllBlockers()
    {
        List<Blocker> foundBlockers = new List<Blocker>();

        for (int i = 0; i < m_board.height; i++)
        {
            List<Blocker> blockerRow = FindBlockersAt(i);
            foundBlockers = foundBlockers.Union(blockerRow).ToList();
        }

        return foundBlockers;
    }

    public bool CanAddBlocker()
    {
        return (Random.Range(0f, 1f) <= m_board.chanceForBlocker
                && m_board.blockerPrefabs.Length > 0
                && m_board.blockerCount < m_board.maxBlockers);
    }

    public List<Marble> RemoveBlockers(List<Marble> bombedMarbles)
    {
        List<Blocker> blockers = FindAllBlockers();
        List<Marble> marblesToRemove = new List<Marble>();

        foreach (Blocker blocker in blockers)
        {
            if (blocker != null)
            {
                if (!blocker.clearedByBomb)
                {
                    marblesToRemove.Add(blocker);
                }
            }
        }

        return bombedMarbles.Except(marblesToRemove).ToList();
    }

    public MatchValue FindMatchValue(List<Marble> marbles)
    {
        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                return marble.matchValue;
            }
        }

        return MatchValue.None;
    }

    public GameObject FindByMatchValue(GameObject[] prefabs, MatchValue matchValue)
    {
        if (matchValue == MatchValue.None)
        {
            return null;
        }

        foreach (GameObject gameObject in prefabs)
        {
            Marble marble = gameObject.GetComponent<Marble>();

            if (marble != null)
            {
                if (marble.matchValue == matchValue)
                {
                    return gameObject;
                }
            }
        }

        return null;
    }
}
