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
}
