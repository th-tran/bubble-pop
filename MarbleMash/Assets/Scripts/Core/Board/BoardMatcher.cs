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

    public List<Bubble> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        // Keep a running list of Bubbles
        List<Bubble> matches = new List<Bubble>();

        Bubble startBubble = null;

        // Get a starting Bubble at an (x,y) position in the array of Bubbles
        if (m_board.boardQuery.IsWithinBounds(startX, startY))
        {
            startBubble = m_board.allBubbles[startX, startY];
        }

        if (startBubble != null)
        {
            matches.Add(startBubble);
        }
        else
        {
            return new List<Bubble>();
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

            // Find the adjacent Bubble
            Bubble nextBubble = m_board.allBubbles[nextX, nextY];

            if (nextBubble == null)
            {
                break;
            }
            else
            {
                // If it matches then add it our running list of Bubbles
                if (nextBubble.matchValue == startBubble.matchValue && !matches.Contains(nextBubble) && nextBubble.matchValue != MatchValue.None)
                {
                    matches.Add(nextBubble);
                }
                else
                {
                    break;
                }
            }
        }

        // If list length is greater than given minimum (usually 3), then return the list.
        // Otherwise, return an empty list.
        return (matches.Count >= minLength) ? matches : new List<Bubble>();

    }

    public List<Bubble> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<Bubble> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<Bubble> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        List<Bubble> combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : new List<Bubble>();
    }

    public List<Bubble> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<Bubble> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<Bubble> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        List<Bubble> combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLength) ? combinedMatches : new List<Bubble>();

    }

    public List<Bubble> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<Bubble> horizontalMatches = FindHorizontalMatches(x, y, minLength);
        List<Bubble> verticalMatches = FindVerticalMatches(x, y, minLength);

        List<Bubble> combinedMatches = horizontalMatches.Union(verticalMatches).ToList();

        return combinedMatches;
    }

    public List<Bubble> FindMatchesAt(List<Bubble> bubbles, int minLength = 3)
    {
        List<Bubble> matches = new List<Bubble>();

        foreach (Bubble bubble in bubbles)
        {
            matches = matches.Union(FindMatchesAt(bubble.xIndex, bubble.yIndex, minLength)).ToList();
        }

        return matches;
    }

    public List<Bubble> FindAllMatches()
    {
        List<Bubble> combinedMatches = new List<Bubble>();

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                List<Bubble> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }

        return combinedMatches;
    }

    public List<Bubble> FindAllMatchValue(MatchValue matchValue)
    {
        List<Bubble> foundBubbles = new List<Bubble>();

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                if (m_board.allBubbles[i,j] != null)
                {
                    if (m_board.allBubbles[i,j].matchValue == matchValue)
                    {
                        foundBubbles.Add(m_board.allBubbles[i,j]);
                    }
                }
            }
        }

        return foundBubbles;
    }
}
