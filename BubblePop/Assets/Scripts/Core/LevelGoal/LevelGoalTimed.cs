using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal
{
    public Timer timer;
    int m_maxTime;

    // Start is called before the first frame update
    void Start()
    {
        if (timer != null)
        {
            timer.Init(timeLeft);
        }

        m_maxTime = timeLeft;
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        while (timeLeft >= 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;

            if (timer != null)
            {
                timer.UpdateTimer(timeLeft);
            }
        }
    }

    public override bool IsWinner()
    {
        if (ScoreManager.Instance != null)
        {
            return (ScoreManager.Instance.CurrentScore >= scoreGoals[0]);
        }

        return false;
    }

    public override bool IsGameOver()
    {
        return (timeLeft < 0 || scoreStars >= scoreGoals.Length);
    }

    public void AddTime(int timeValue)
    {
        timeLeft += timeValue;
        timeLeft = Mathf.Clamp(timeLeft, 0, m_maxTime);

        if (timer != null)
        {
            timer.UpdateTimer(timeLeft);
        }
    }
}
