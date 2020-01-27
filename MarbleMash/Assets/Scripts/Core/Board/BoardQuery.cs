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

    public GameObject GetRandomBubble()
    {
        if (m_board == null)
        {
            return null;
        }

        return GetRandomObject(m_board.bubblePrefabs);
    }

    public GameObject GetRandomBlocker()
    {
        if (m_board == null)
        {
            return null;
        }

        return GetRandomObject(m_board.blockerPrefabs);
    }

    public List<int> GetColumns(List<Bubble> bubbles)
    {
        List<int> columns = new List<int>();

        foreach (Bubble bubble in bubbles)
        {
            if (!columns.Contains(bubble.xIndex))
            {
                columns.Add(bubble.xIndex);
            }
        }

        return columns;
    }

    public List<Bubble> GetRowBubbles(int row)
    {
        List<Bubble> bubbles = new List<Bubble>();

        for (int i = 0; i < m_board.width; i++)
        {
            if (m_board.allBubbles[i, row] != null)
            {
                bubbles.Add(m_board.allBubbles[i, row]);
            }
        }

        return bubbles;
    }

    public List<Bubble> GetColumnBubbles(int column)
    {
        List<Bubble> bubbles = new List<Bubble>();

        for (int i = 0; i < m_board.height; i++)
        {
            if (m_board.allBubbles[column, i] != null)
            {
                bubbles.Add(m_board.allBubbles[column, i]);
            }
        }

        return bubbles;
    }

    public List<Bubble> GetAdjacentBubbles(int x, int y, int offset = 1)
    {
        List<Bubble> bubbles = new List<Bubble>();

        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if (IsWithinBounds(i,j))
                {
                    bubbles.Add(m_board.allBubbles[i, j]);
                }
            }
        }

        return bubbles;
    }

    public List<Bubble> GetBombedBubbles(List<Bubble> bubbles)
    {
        List<Bubble> allBubblesToClear = new List<Bubble>();

        foreach (Bubble bubble in bubbles)
        {
            if (bubble != null)
            {
                List<Bubble> bubblesToClear = new List<Bubble>();

                Bomb bomb = bubble.GetComponent<Bomb>();

                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            bubblesToClear = GetColumnBubbles(bomb.xIndex);
                            break;
                        case BombType.Row:
                            bubblesToClear = GetRowBubbles(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            bubblesToClear = GetAdjacentBubbles(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:
                            // TODO: Destroy all bubbles of a random color
                            break;
                        default:
                            break;
                    }
                }

                allBubblesToClear = allBubblesToClear.Union(bubblesToClear).ToList();
                allBubblesToClear = RemoveBlockers(allBubblesToClear);
            }
        }

        return allBubblesToClear;
    }

    public bool IsColorBomb(Bubble bubble)
    {
        Bomb bomb = bubble.GetComponent<Bomb>();

        if (bomb != null)
        {
            return (bomb.bombType == BombType.Color);
        }

        return false;
    }

    public bool IsCornerMatch(List<Bubble> bubbles)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (Bubble bubble in bubbles)
        {
            if (bubble != null)
            {
                if (xStart == -1 || yStart == -1)
                {
                    xStart = bubble.xIndex;
                    yStart = bubble.yIndex;
                    continue;
                }

                if (bubble.xIndex != xStart && bubble.yIndex == yStart)
                {
                    horizontal = true;
                }

                if (bubble.xIndex == xStart && bubble.yIndex != yStart)
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

    public bool IsCollapsed(List<Bubble> bubbles)
    {
        foreach (Bubble bubble in bubbles)
        {
            if (bubble != null)
            {
                if (bubble.transform.position.y - (float)bubble.yIndex > 0.001f)
                {
                    return false;
                }

                if (bubble.transform.position.x - (float)bubble.xIndex > 0.001f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool HasMatch(int x, int y, int minLength = 3)
    {
        // Find matches at given (x,y)
        List<Bubble> matches = m_board.boardMatcher.FindMatchesAt(x, y, minLength);

        // Return whether matches were found
        return (matches.Count > 0);
    }

    public List<Blocker> FindBlockersAt(int row, bool clearedAtBottomOnly = false)
    {
        List<Blocker> foundBlockers = new List<Blocker>();

        for (int i = 0; i < m_board.width; i++)
        {
            if (m_board.allBubbles[i,row] != null)
            {
                Blocker blockerComponent = m_board.allBubbles[i,row].GetComponent<Blocker>();

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

    public List<Bubble> RemoveBlockers(List<Bubble> bombedBubbles)
    {
        List<Blocker> blockers = FindAllBlockers();
        List<Bubble> bubblesToRemove = new List<Bubble>();

        foreach (Blocker blocker in blockers)
        {
            if (blocker != null)
            {
                if (!blocker.clearedByBomb)
                {
                    bubblesToRemove.Add(blocker);
                }
            }
        }

        return bombedBubbles.Except(bubblesToRemove).ToList();
    }

    public MatchValue FindMatchValue(List<Bubble> bubbles)
    {
        foreach (Bubble bubble in bubbles)
        {
            if (bubble != null)
            {
                return bubble.matchValue;
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
            Bubble bubble = gameObject.GetComponent<Bubble>();

            if (bubble != null)
            {
                if (bubble.matchValue == matchValue)
                {
                    return gameObject;
                }
            }
        }

        return null;
    }
}
