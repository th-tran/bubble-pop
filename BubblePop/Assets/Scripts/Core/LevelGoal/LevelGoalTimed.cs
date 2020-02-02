using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal
{
    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;
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
        return (timeLeft <= 0);
    }
}
