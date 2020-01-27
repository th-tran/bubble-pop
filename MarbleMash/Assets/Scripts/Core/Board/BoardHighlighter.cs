using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardHighlighter : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    void HighlightTileOff(int x, int y)
    {
        if (m_board == null)
        {
            return;
        }

        if (m_board.allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_board.allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    void HighlightTileOn(int x, int y, Color color)
    {
        if (m_board == null)
        {
            return;
        }

        if (m_board.allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_board.allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = color;
        }
    }

    void HighlightMatchesAt(int x, int y)
    {
        if (m_board == null)
        {
            return;
        }

        HighlightTileOff(x, y);
        List<Marble> combinedMatches = m_board.boardMatcher.FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (Marble marble in combinedMatches)
            {
                if (marble != null)
                {
                    HighlightTileOn(marble.xIndex, marble.yIndex, marble.GetComponent<SpriteRenderer>().color);
                }
            }
        }
    }

    void HighlightMatches()
    {
        if (m_board == null)
        {
            return;
        }

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    void HighlightMarbles(List<Marble> marbles)
    {
        if (m_board == null)
        {
            return;
        }

        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                HighlightTileOn(marble.xIndex, marble.yIndex, marble.GetComponent<SpriteRenderer>().color);
            }
        }
    }
}
