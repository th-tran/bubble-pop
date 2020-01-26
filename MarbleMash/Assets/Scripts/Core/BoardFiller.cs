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

    public Marble FillRandomMarbleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (m_board == null)
        {
            return null;
        }

        if (m_board.boardQuery.IsWithinBounds(x, y))
        {
            GameObject randomMarble = Instantiate(m_board.boardQuery.GetRandomMarble(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeMarble(randomMarble, x, y, falseYOffset, moveTime);
            return randomMarble.GetComponent<Marble>();
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
                if (m_board.allMarbles[i, j] == null && m_board.allTiles[i, j].tileType != TileType.Obstacle)
                {
                    // ...fill in a Marble
                    FillRandomMarbleAt(i, j, falseYOffset, moveTime);
                    iterations = 0;

                    // If we form a match while filling in the Marble...
                    while (m_board.boardQuery.HasMatchOnFill(i, j))
                    {
                        // ...remove the Marble and try again
                        m_board.boardClearer.ClearMarbleAt(i, j);
                        FillRandomMarbleAt(i, j, falseYOffset, moveTime);

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

    public void MakeMarble(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (m_board == null)
        {
            return;
        }

        // Only run the logic on valid GameObject and if we are within the boundaries of the Board
        if (prefab != null && m_board.boardQuery.IsWithinBounds(x, y))
        {
            Marble marble = prefab.GetComponent<Marble>();
            marble.Init(m_board);
            PlaceMarble(marble, x, y);

            // Allow the Marble to be placed higher than the Board, so it can be moved into place
            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                marble.Move(x, y, moveTime);
            }

            // Parent the Marble to the Board
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

    public void PlaceMarble(Marble marble, int x, int y)
    {
        if (m_board == null)
        {
            return;
        }

        if (marble == null)
        {
            Debug.LogWarning("BOARD: Invalid Marble!");
            return;
        }

        marble.transform.position = new Vector3(x, y, 0);
        marble.transform.rotation = Quaternion.identity;

        if (m_board.boardQuery.IsWithinBounds(x, y))
        {
            m_board.allMarbles[x, y] = marble;
        }

        marble.SetCoordinates(x, y);
    }

    public IEnumerator RefillRoutine()
    {
        m_board.boardFiller.FillBoard(m_board.fillYOffset, m_board.fillMoveTime);

        yield return new WaitForSeconds(m_board.fillMoveTime);
    }
}
