using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardCollapser : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public List<Marble> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        // Running list of Marbles to move
        List<Marble> movingMarbles = new List<Marble>();

        // loop from the bottom of the column
        for (int i = 0; i < m_board.height - 1; i++)
        {
            // If the current space is empty and not occupied by an Obstacle Tile...
            if (m_board.allMarbles[column, i] == null && m_board.allTiles[column, i].tileType != TileType.Obstacle)
            {
                // ...loop from the space above it to the top of the column, to search for the next Marble
                for (int j = i + 1; j < m_board.height; j++)
                {
                    // If we find a Marble...
                    if (m_board.allMarbles[column, j] != null)
                    {
                        // ...move the Marble downward to fill in the space and update the Marble array
                        m_board.allMarbles[column, j].Move(column, i, collapseTime * (j - i));
                        m_board.allMarbles[column, i] = m_board.allMarbles[column, j];
                        m_board.allMarbles[column, i].SetCoordinates(column, i);

                        // ...add Marble to the list of moving Marbles
                        if (!movingMarbles.Contains(m_board.allMarbles[column, i]))
                        {
                            movingMarbles.Add(m_board.allMarbles[column, i]);
                        }

                        m_board.allMarbles[column, j] = null;

                        // ...break out of the loop and stop searching
                        break;
                    }
                }
            }
        }

        return movingMarbles;
    }

    public List<Marble> CollapseColumn(List<Marble> marbles, float collapseTime = 0.1f)
    {
        List<Marble> movingMarbles = new List<Marble>();

        List<int> columnsToCollapse = m_board.boardQuery.GetColumns(marbles);

        foreach (int column in columnsToCollapse)
        {
            movingMarbles = movingMarbles.Union(CollapseColumn(column, collapseTime)).ToList();
        }

        return movingMarbles;
    }

    public List<Marble> CollapseColumn(List<int> columnsToCollapse, float collapseTime = 0.1f)
    {
        List<Marble> movingMarbles = new List<Marble>();
        foreach (int column in columnsToCollapse)
        {
            movingMarbles = movingMarbles.Union(CollapseColumn(column, collapseTime)).ToList();
        }

        return movingMarbles;
    }
}
