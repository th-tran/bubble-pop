using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal
{
    // Start is called before the first frame update
    public override void Start()
    {
        levelCounter = LevelCounter.Timer;
        base.Start();

        if (UIManager.Instance != null && UIManager.Instance.timer != null)
        {
            UIManager.Instance.timer.Init(timeLeft);
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
}
