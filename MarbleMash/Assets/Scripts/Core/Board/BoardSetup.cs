using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardSetup : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public void SetupBoard()
    {
        if (m_board == null)
        {
            return;
        }

        // Set up pre-placed Tiles
        SetupTiles();

        // Set up pre-placed Marbles
        SetupMarbles();

        // Place Camera to frame the Board with a set border
        SetupCamera();

        // Fill empty Tiles of the Board with Marbles
        m_board.boardFiller.FillBoard(m_board.fillYOffset, m_board.fillMoveTime);
    }

    public void SetupTiles()
    {
        if (m_board == null)
        {
            return;
        }

        foreach (StartingObject sTile in m_board.startingTiles)
        {
            if (sTile != null)
            {
                m_board.boardFiller.MakeTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
            }

        }

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                if (m_board.allTiles[i, j] == null)
                {
                    m_board.boardFiller.MakeTile(m_board.tileNormalPrefab, i, j);
                }
            }
        }
    }

    public void SetupMarbles()
    {
        if (m_board == null)
        {
            return;
        }

        foreach (StartingObject sMarble in m_board.startingMarbles)
        {
            if (sMarble != null)
            {
                GameObject marble = Instantiate(sMarble.prefab, new Vector3(sMarble.x, sMarble.y, 0), Quaternion.identity) as GameObject;
                m_board.boardFiller.MakeMarble(marble, sMarble.x, sMarble.y, m_board.fillYOffset, m_board.fillMoveTime);
            }
        }
    }


    public void SetupCamera()
    {
        if (m_board == null)
        {
            return;
        }

        Camera.main.transform.position = new Vector3((float)(m_board.width - 1) / 2f, (float)(m_board.height - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)m_board.height / 2f + (float)m_board.borderSize;
        float horizontalSize = ((float)m_board.width / 2f + (float)m_board.borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

    }
}
