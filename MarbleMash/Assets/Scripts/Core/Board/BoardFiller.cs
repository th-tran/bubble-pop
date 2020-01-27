using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardFiller : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public Bubble FillRandomBubbleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (m_board == null)
        {
            return null;
        }

        if (m_board.boardQuery.IsWithinBounds(x, y))
        {
            GameObject randomBubble = Instantiate(m_board.boardQuery.GetRandomBubble(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeBubble(randomBubble, x, y, falseYOffset, moveTime);
            return randomBubble.GetComponent<Bubble>();
        }

        return null;
    }

    public Blocker FillRandomBlockerAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (m_board == null)
        {
            return null;
        }

        if (m_board.boardQuery.IsWithinBounds(x, y))
        {
            GameObject randomBlocker = Instantiate(m_board.boardQuery.GetRandomBlocker(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeBubble(randomBlocker, x, y, falseYOffset, moveTime);
            return randomBlocker.GetComponent<Blocker>();
        }

        return null;
    }

    public void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxInterations = 100;
        int iterations = 0;

        // Loop through all spaces of the board
        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                // If the space is unoccupied and does not contain an Obstacle tile...
                if (m_board.allBubbles[i, j] == null && m_board.allTiles[i, j].tileType != TileType.Obstacle)
                {
                    if (j == m_board.height - 1 && m_board.boardQuery.CanAddBlocker())
                    {
                        FillRandomBlockerAt(i, j, falseYOffset, moveTime);
                        m_board.blockerCount++;
                    }
                    else
                    {
                        // ...fill in a Bubble
                        FillRandomBubbleAt(i, j, falseYOffset, moveTime);
                        iterations = 0;

                        // If we form a match while filling in the Bubble...
                        while (m_board.boardQuery.HasMatch(i, j))
                        {
                            // ...remove the Bubble and try again
                            m_board.boardClearer.ClearBubbleAt(i, j);
                            FillRandomBubbleAt(i, j, falseYOffset, moveTime);

                            // ...check to prevent infinite loop
                            iterations++;
                            if (iterations >= maxInterations)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (m_board == null)
        {
            return;
        }

        // Only run the logic on valid GameObject and if we are within the boundaries of the Board
        if (prefab != null && m_board.boardQuery.IsWithinBounds(x, y))
        {
            // Create a Tile at position (x,y,z) with no rotations.
            // Rename the Tile and parent it to the Board,
            // then initialize the Tile into the m_allTiles array
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + x + "," + y + ")";
            m_board.allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;
            m_board.allTiles[x, y].Init(x, y, m_board);
        }
    }

    public void MakeBubble(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (m_board == null)
        {
            return;
        }

        // Only run the logic on valid GameObject and if we are within the boundaries of the Board
        if (prefab != null && m_board.boardQuery.IsWithinBounds(x, y))
        {
            Bubble bubble = prefab.GetComponent<Bubble>();
            bubble.Init(m_board);
            PlaceBubble(bubble, x, y);

            // Allow the Bubble to be placed higher than the Board, so it can be moved into place
            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                bubble.Move(x, y, moveTime);
            }

            // Parent the Bubble to the Board
            prefab.transform.parent = transform;
        }
    }

    public GameObject MakeBomb(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null && m_board.boardQuery.IsWithinBounds(x,y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            bomb.GetComponent<Bomb>().Init(m_board);
            bomb.GetComponent<Bomb>().SetCoordinates(x,y);
            bomb.transform.parent = transform;
            return bomb;
        }

        return null;
    }

    public void PlaceBubble(Bubble bubble, int x, int y)
    {
        if (m_board == null)
        {
            return;
        }

        if (bubble == null)
        {
            Debug.LogWarning("BOARD: Invalid Bubble!");
            return;
        }

        bubble.transform.position = new Vector3(x, y, 0);
        bubble.transform.rotation = Quaternion.identity;

        if (m_board.boardQuery.IsWithinBounds(x, y))
        {
            m_board.allBubbles[x, y] = bubble;
        }

        bubble.SetCoordinates(x, y);
    }

    public IEnumerator RefillRoutine()
    {
        m_board.boardFiller.FillBoard(m_board.fillYOffset, m_board.fillMoveTime);

        yield return new WaitForSeconds(m_board.fillMoveTime);
    }
}
