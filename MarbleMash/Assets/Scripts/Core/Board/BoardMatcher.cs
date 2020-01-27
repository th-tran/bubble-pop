using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardMatcher : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public List<Marble> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        // Keep a running list of Marbles
        List<Marble> matches = new List<Marble>();

        Marble startMarble = null;

        // Get a starting Marble at an (x,y) position in the array of Marbles
        if (m_board.boardQuery.IsWithinBounds(startX, startY))
        {
            startMarble = m_board.allMarbles[startX, startY];
        }

        if (startMarble != null)
        {
            matches.Add(startMarble);
        }
        else
        {
            return new List<Marble>();
        }

        // Use the search direction to increment to the next space to look
        int nextX;
        int nextY;

        int maxValue = (m_board.width > m_board.height) ? m_board.width : m_board.height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!m_board.boardQuery.IsWithinBounds(nextX, nextY))
            {
                break;
            }

            // Find the adjacent Marble
            Marble nextMarble = m_board.allMarbles[nextX, nextY];

            if (nextMarble == null)
            {
                break;
            }
            else
            {
                // If it matches then add it our running list of Marbles
                if (nextMarble.matchValue == startMarble.matchValue && !matches.Contains(nextMarble) && nextMarble.matchValue != MatchValue.None)
                {
                    matches.Add(nextMarble);
                }
                else
                {
                    break;
                }
            }
        }

        // If list length is greater than given minimum (usually 3), then return the list.
        // Otherwise, return an empty list.
        return (matches.Count >= minLength) ? matches : new List<Marble>();

    }

    public List<Marble> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<Marble> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<Marble> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        List<Marble> combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : new List<Marble>();
    }

    public List<Marble> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<Marble> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<Marble> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        List<Marble> combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : new List<Marble>();

    }

    public List<Marble> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<Marble> horizontalMatches = FindHorizontalMatches(x, y, minLength);
        List<Marble> verticalMatches = FindVerticalMatches(x, y, minLength);

        List<Marble> combinedMatches = horizontalMatches.Union(verticalMatches).ToList();

        return combinedMatches;
    }

    public List<Marble> FindMatchesAt(List<Marble> marbles, int minLength = 3)
    {
        List<Marble> matches = new List<Marble>();

        foreach (Marble marble in marbles)
        {
            matches = matches.Union(FindMatchesAt(marble.xIndex, marble.yIndex, minLength)).ToList();
        }

        return matches;
    }

    public List<Marble> FindAllMatches()
    {
        List<Marble> combinedMatches = new List<Marble>();

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                List<Marble> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }

        return combinedMatches;
    }

    public List<Marble> FindAllMatchValue(MatchValue matchValue)
    {
        List<Marble> foundMarbles = new List<Marble>();

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                if (m_board.allMarbles[i,j] != null)
                {
                    if (m_board.allMarbles[i,j].matchValue == matchValue)
                    {
                        foundMarbles.Add(m_board.allMarbles[i,j]);
                    }
                }
            }
        }

        return foundMarbles;
    }
}
