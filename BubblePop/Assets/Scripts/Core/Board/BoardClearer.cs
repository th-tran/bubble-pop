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
                ClearBubbleAt(i, j);
                ParticleManager.Instance.ClearBubbleFXAt(i, j);
            }
        }
    }

    public void ClearBubbleAt(int x, int y)
    {
        Bubble bubbleToClear = m_board.allBubbles[x, y];

        if (bubbleToClear != null)
        {
            m_board.allBubbles[x, y] = null;
            Destroy(bubbleToClear.gameObject);
        }

        //HighlightTileOff(x,y);
    }

    public void ClearBubbleAt(List<Bubble> bubbles, List<Bubble> bombedBubbles)
    {
        foreach (Bubble bubble in bubbles)
        {
            if (bubble != null)
            {
                ClearBubbleAt(bubble.xIndex, bubble.yIndex);

                int bonus = 0;
                if (bubbles.Count >= 4)
                {
                    bonus = 20;
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ScorePoints(bubble, m_board.scoreMultiplier, bonus);
                }

                if (ParticleManager.Instance != null)
                {
                    if (bombedBubbles.Contains(bubble))
                    {
                        ParticleManager.Instance.BombFXAt(bubble.xIndex, bubble.yIndex);
                    }
                    else
                    {
                        ParticleManager.Instance.ClearBubbleFXAt(bubble.xIndex, bubble.yIndex);
                    }
                }
            }
        }
    }
}
