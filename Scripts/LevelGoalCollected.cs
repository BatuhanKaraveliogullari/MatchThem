using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalCollected : LevelGoal
{
    public CollectionGoal[] collectionGoals;

    public void UpdateGoals(GamePiece pieceToCheck)
    {
        if(pieceToCheck != null)
        {
            foreach (CollectionGoal goal in collectionGoals)
            {
                if(goal != null)
                {
                    goal.CollectPiece(pieceToCheck);
                }
            }
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if(UIManager.Insatance != null)
        {
            UIManager.Insatance.UpdateCollectionGoalLayout();
        }
    }

    bool AreGoalsComplete(CollectionGoal[] goals)
    {
        foreach (CollectionGoal g in goals)
        {
            if(g == null && goals == null)
            {
                return false;
            }

            if(goals.Length == 0)
            {
                return false;
            }

            if(g.numberToCollect != 0)
            {
                return false;
            }
        }

        return true;
    }

    public override bool IsGameOver()
    {
        if(AreGoalsComplete(collectionGoals))
        {
            int maxScore = scoreGoals[scoreGoals.Length - 1];

            if(ScoreManager.Insatance.CurrentScore >= maxScore)
            {
                return true;
            }
        }

        if(levelCounter == LevelCounter.Timer)
        {
            return (timeLeft <= 0);
        }

        else
        {
            return (movesLeft <= 0);
        }
    }

    public override bool IsWinner()
    {
        if(ScoreManager.Insatance != null)
        {
            return (ScoreManager.Insatance.CurrentScore >= scoreGoals[0] && AreGoalsComplete(collectionGoals));
        }

        return false;
    }
}

