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

    public GameObject DropBomb(int x, int y, Vector2 swapDirection, List<Marble> marbles)
    {
        GameObject bomb = null;

        // Only drop a bomb if player matched 4 or more
        if (marbles.Count >= 4)
        {
            if (m_board.boardQuery.IsCornerMatch(marbles))
            {
                // Adjacent bomb
                if (m_board.adjacentBombPrefab != null)
                {
                    bomb = m_board.boardFiller.MakeBomb(m_board.adjacentBombPrefab, x, y);
                }
            }
            else if (marbles.Count >= 5)
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
                if (m_board.rowBombPrefab != null)
                {
                    bomb = m_board.boardFiller.MakeBomb(m_board.rowBombPrefab, x, y);
                }
            }
            else
            {
                // Column bomb
                if (m_board.columnBombPrefab != null)
                {
                    bomb = m_board.boardFiller.MakeBomb(m_board.columnBombPrefab, x, y);
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
            m_board.allMarbles[x,y] = bomb.GetComponent<Bomb>();
        }
    }
}
