using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardInput : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public void ClickTile(Tile tile)
    {
        if (m_board == null)
        {
            return;
        }

        if (m_board.clickedTile == null)
        {
            m_board.clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (m_board == null)
        {
            return;
        }

        if (m_board.clickedTile != null && m_board.boardQuery.IsNextTo(tile, m_board.clickedTile))
        {
            m_board.targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (m_board == null)
        {
            return;
        }

        if (m_board.clickedTile != null && m_board.targetTile != null)
        {
            m_board.SwitchTiles(m_board.clickedTile, m_board.targetTile);
        }

        m_board.clickedTile = null;
        m_board.targetTile = null;
    }
}
