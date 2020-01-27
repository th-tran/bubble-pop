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

    public List<Bubble> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        // Running list of Bubbles to move
        List<Bubble> movingBubbles = new List<Bubble>();

        // loop from the bottom of the column
        for (int i = 0; i < m_board.height - 1; i++)
        {
            // If the current space is empty and not occupied by an Obstacle Tile...
            if (m_board.allBubbles[column, i] == null && m_board.allTiles[column, i].tileType != TileType.Obstacle)
            {
                // ...loop from the space above it to the top of the column, to search for the next Bubble
                for (int j = i + 1; j < m_board.height; j++)
                {
                    // If we find a Bubble...
                    if (m_board.allBubbles[column, j] != null)
                    {
                        // ...move the Bubble downward to fill in the space and update the Bubble array
                        m_board.allBubbles[column, j].Move(column, i, collapseTime * (j - i));
                        m_board.allBubbles[column, i] = m_board.allBubbles[column, j];
                        m_board.allBubbles[column, i].SetCoordinates(column, i);

                        // ...add Bubble to the list of moving Bubbles
                        if (!movingBubbles.Contains(m_board.allBubbles[column, i]))
                        {
                            movingBubbles.Add(m_board.allBubbles[column, i]);
                        }

                        m_board.allBubbles[column, j] = null;

                        // ...break out of the loop and stop searching
                        break;
                    }
                }
            }
        }

        return movingBubbles;
    }

    public List<Bubble> CollapseColumn(List<Bubble> bubbles, float collapseTime = 0.1f)
    {
        List<Bubble> movingBubbles = new List<Bubble>();

        List<int> columnsToCollapse = m_board.boardQuery.GetColumns(bubbles);

        foreach (int column in columnsToCollapse)
        {
            movingBubbles = movingBubbles.Union(CollapseColumn(column, collapseTime)).ToList();
        }

        return movingBubbles;
    }

    public List<Bubble> CollapseColumn(List<int> columnsToCollapse, float collapseTime = 0.1f)
    {
        List<Bubble> movingBubbles = new List<Bubble>();
        foreach (int column in columnsToCollapse)
        {
            movingBubbles = movingBubbles.Union(CollapseColumn(column, collapseTime)).ToList();
        }

        return movingBubbles;
    }
}
