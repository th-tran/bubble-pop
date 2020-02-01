using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardDeadlock : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    List<Bubble> GetRowOrColumnList(int x, int y, int listLength = 3, bool checkRow = true)
    {
        if (m_board == null)
        {
            return null;
        }

        List<Bubble> bubblesList = new List<Bubble>();

        for (int i = 0; i < listLength; i++)
        {
            if (checkRow)
            {
                if (m_board.boardQuery.IsWithinBounds(x + i, y) && m_board.allBubbles[x + i, y] != null)
                {
                    bubblesList.Add(m_board.allBubbles[x+i, y]);
                }
            }
            else
            {
                if (m_board.boardQuery.IsWithinBounds(x, y + i) && m_board.allBubbles[x, y + i] != null)
                {
                    bubblesList.Add(m_board.allBubbles[x, y+i]);
                }
            }
        }

        return bubblesList;
    }

    List<Bubble> GetMinimumMatches(List<Bubble> bubbles, int minForMatch = 2)
    {
        List<Bubble> matches = new List<Bubble>();

        var groups = bubbles.GroupBy(bubble => bubble.matchValue);

        foreach (var group in groups)
        {
            if (group.Count() >= minForMatch && group.Key != MatchValue.None)
            {
                matches = group.ToList();
            }
        }

        return matches;
    }

    List<Bubble> GetNeighbors(int x, int y)
    {
        if (m_board == null)
        {
            return null;
        }

        List<Bubble> neighbors = new List<Bubble>();

        Vector2[] searchDirections = new Vector2[4]
        {
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-0f, 1f),
            new Vector2(0f, -1f)
        };

        foreach (Vector2 dir in searchDirections)
        {
            if (m_board.boardQuery.IsWithinBounds(x + (int)dir.x, y + (int)dir.y))
            {
                Bubble neighbor = m_board.allBubbles[x + (int)dir.x, y + (int)dir.y];

                if (neighbor != null)
                {
                    if (!neighbors.Contains(neighbor))
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }
        }

        return neighbors;
    }

    bool HasMoveAt(int x, int y, int listLength = 3, bool checkRow = true)
    {
        List<Bubble> bubbles = GetRowOrColumnList(x, y, listLength, checkRow);
        List<Bubble> matches = GetMinimumMatches(bubbles, listLength - 1);

        Bubble unmatchedBubble = null;

        if (bubbles != null && matches != null)
        {
            if (bubbles.Count == listLength && matches.Count == listLength - 1)
            {
                unmatchedBubble = bubbles.Except(matches).FirstOrDefault();
            }

            if (unmatchedBubble != null)
            {
                List<Bubble> neighbors = GetNeighbors(unmatchedBubble.xIndex, unmatchedBubble.yIndex);
                neighbors = neighbors.Except(matches).ToList()
                                     .FindAll(bubble => bubble.matchValue == matches[0].matchValue);

                matches = matches.Union(neighbors).ToList();
            }
        }

        if (matches.Count >= listLength)
        {
            return true;
        }

        return false;
    }

    public bool IsDeadlocked(int listLength = 3)
    {
        if (m_board == null)
        {
            return false;
        }

        bool isDeadlocked = true;

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                if (HasMoveAt(i, j, listLength, true) || HasMoveAt(i, j, listLength, false))
                {
                    isDeadlocked = false;
                }
            }
        }

        return isDeadlocked;
    }
}
