using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardBomber : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public GameObject DropBomb(int x, int y, Vector2 swapDirection, List<Bubble> bubbles)
    {
        GameObject bomb = null;
        MatchValue matchValue = MatchValue.None;

        if (bubbles != null)
        {
            matchValue = m_board.boardQuery.FindMatchValue(bubbles);
        }

        // Only drop a bomb if player matched 4 or more
        if (bubbles.Count >= 4 && matchValue != MatchValue.None)
        {
            if (m_board.boardQuery.IsCornerMatch(bubbles))
            {
                // Adjacent bomb
                GameObject adjacentBomb = m_board.boardQuery.FindByMatchValue(m_board.adjacentBombPrefabs, matchValue);
                if (adjacentBomb != null)
                {
                    bomb = m_board.boardFiller.MakeBomb(adjacentBomb, x, y);
                }
            }
            else if (bubbles.Count >= 5)
            {
                // Color bomb
                if (m_board.colorBombPrefab != null)
                {
                    bomb = m_board.boardFiller.MakeBomb(m_board.colorBombPrefab, x, y);
                }
            }
            else if (swapDirection.x != 0)
            {
                // Row bomb
                GameObject rowBomb = m_board.boardQuery.FindByMatchValue(m_board.rowBombPrefabs, matchValue);
                if (rowBomb != null)
                {
                    bomb = m_board.boardFiller.MakeBomb(rowBomb, x, y);
                }
            }
            else
            {
                // Column bomb
                GameObject columnBomb = m_board.boardQuery.FindByMatchValue(m_board.columnBombPrefabs, matchValue);
                if (columnBomb != null)
                {
                    bomb = m_board.boardFiller.MakeBomb(columnBomb, x, y);
                }
            }
        }

        return bomb;
    }

    public void ActivateBomb(GameObject bomb)
    {
        int x = (int) bomb.transform.position.x;
        int y = (int) bomb.transform.position.y;

        if (m_board.boardQuery.IsWithinBounds(x,y))
        {
            m_board.allBubbles[x,y] = bomb.GetComponent<Bomb>();
        }
    }
}
