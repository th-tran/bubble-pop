﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalCollected : LevelGoal
{
    public CollectionGoal[] collectionGoals;
    public CollectionGoalPanel[] uiPanels;

    public void UpdateGoals(Bubble bubbleToCheck)
    {
        if (bubbleToCheck != null)
        {
            foreach (CollectionGoal goal in collectionGoals)
            {
                if (goal != null)
                {
                    goal.CollectBubble(bubbleToCheck);
                }
            }
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        foreach (CollectionGoalPanel panel in uiPanels)
        {
            if (panel != null)
            {
                panel.UpdatePanel();
            }
        }
    }

    bool AreGoalsComplete(CollectionGoal[] goals)
    {
        if (goals != null)
        {
            if (goals.Length == 0)
            {
                return false;
            }

            foreach (CollectionGoal goal in goals)
            {
                if (goal != null && goal.numberToCollect > 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public override bool IsGameOver()
    {
        if (AreGoalsComplete(collectionGoals))
        {
            int maxScore = scoreGoals[scoreGoals.Length - 1];

            if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= maxScore)
            {
                return true;
            }
        }

        return (movesLeft <= 0);
    }

    public override bool IsWinner()
    {
        if (ScoreManager.Instance != null)
        {
            return ScoreManager.Instance.CurrentScore >= scoreGoals[0] && AreGoalsComplete(collectionGoals);
        }

        return false;
    }
}
