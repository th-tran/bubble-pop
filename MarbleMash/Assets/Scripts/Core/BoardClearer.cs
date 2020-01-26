using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardClearer : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public void ClearBoard()
    {
        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                ClearMarbleAt(i, j);
                ParticleManager.Instance.ClearMarbleFXAt(i, j);
            }
        }
    }

    public void ClearMarbleAt(int x, int y)
    {
        Marble marbleToClear = m_board.allMarbles[x, y];

        if (marbleToClear != null)
        {
            m_board.allMarbles[x, y] = null;
            Destroy(marbleToClear.gameObject);
        }

        //HighlightTileOff(x,y);
    }

    public void ClearMarbleAt(List<Marble> marbles, List<Marble> bombedMarbles)
    {
        foreach (Marble marble in marbles)
        {
            if (marble != null)
            {
                ClearMarbleAt(marble.xIndex, marble.yIndex);
                if (bombedMarbles.Contains(marble))
                {
                    ParticleManager.Instance.BombFXAt(marble.xIndex, marble.yIndex);
                }
                else
                {
                    ParticleManager.Instance.ClearMarbleFXAt(marble.xIndex, marble.yIndex);
                }
            }
        }
    }
}
