using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoal : Singleton<LevelGoal>
{
    public int scoreStars = 0;
    public int[] scoreGoals = new int[3] { 1000, 2000, 3000 };
    public int movesLeft = 10;

    public override void Awake()
    {
        base.Awake();

        Init();
    }

    public void Init()
    {
        scoreStars = 0;
    }

    public int UpdateScore(int score)
    {
        for (int i = 0; i < scoreGoals.Length; i++)
        {
            if (score < scoreGoals[i])
            {
                return i;
            }
        }

        return scoreGoals.Length;
    }

    public void UpdateScoreStars(int score)
    {
        scoreStars = UpdateScore(score);
    }
}
