using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardShuffler : MonoBehaviour
{
    Board m_board;

    void Awake()
    {
        m_board = GetComponent<Board>();
    }

    public List<Bubble> RemoveNormalBubbles()
    {
        List<Bubble> normalBubbles = new List<Bubble>();

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                if (m_board.allBubbles[i, j] != null)
                {
                    Bomb bomb = m_board.allBubbles[i, j].GetComponent<Bomb>();
                    Blocker blocker = m_board.allBubbles[i, j].GetComponent<Blocker>();

                    if (bomb == null && blocker == null)
                    {
                        normalBubbles.Add(m_board.allBubbles[i, j]);
                        m_board.allBubbles[i, j] = null;
                    }
                }
            }
        }

        return normalBubbles;
    }

    public void ShuffleList(List<Bubble> bubblesToShuffle)
    {
        int maxCount = bubblesToShuffle.Count;

        for (int i = 0; i < maxCount - 1; i++)
        {
            int r = Random.Range(i, maxCount);

            if (r == i)
            {
                continue;
            }

            Bubble temp = bubblesToShuffle[r];
            bubblesToShuffle[r] = bubblesToShuffle[i];
            bubblesToShuffle[i] = temp;
        }
    }

    public void MoveBubbles(float swapTime = 0.5f)
    {
        if (m_board)

        for (int i = 0; i < m_board.width; i++)
        {
            for (int j = 0; j < m_board.height; j++)
            {
                if (m_board.allBubbles[i, j] != null)
                {
                    m_board.allBubbles[i, j].Move(i, j, swapTime);
                }
            }
        }
    }

    public IEnumerator ShuffleBoardRoutine()
    {
        List<Bubble> allBubbles = new List<Bubble>();
        foreach (Bubble bubble in m_board.allBubbles)
        {
            allBubbles.Add(bubble);
        }

        while (!m_board.boardQuery.IsCollapsed(allBubbles))
        {
            yield return null;
        }

        List<Bubble> normalBubbles = RemoveNormalBubbles();

        ShuffleList(normalBubbles);

        m_board.boardFiller.FillBoard(normalBubbles);

        MoveBubbles(m_board.SwapTime);
        yield return new WaitForSeconds(m_board.SwapTime);

        List<Bubble> matches = m_board.boardMatcher.FindAllMatches();
        yield return StartCoroutine(m_board.ClearAndRefillBoardRoutine(matches));
    }
}
