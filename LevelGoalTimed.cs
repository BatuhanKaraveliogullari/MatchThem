﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal
{
    public override void Start()
    {
        levelCounter = LevelCounter.Timer;

        base.Start();
    }

    public override bool IsWinner()
    {
        if (ScoreManager.Insatance != null)
        {
            return (ScoreManager.Insatance.CurrentScore >= scoreGoals[0]);
        }

        return false;
    }

    public override bool IsGameOver()
    {
        int maxScore = scoreGoals[scoreGoals.Length - 1];

        if (ScoreManager.Insatance.CurrentScore >= maxScore)
        {
            return true;
        }

        return (timeLeft <= 0);
    }
}
