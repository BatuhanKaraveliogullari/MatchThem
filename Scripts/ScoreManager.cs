using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    int m_currentScore = 0;
    public int CurrentScore
    {
        get
        {
            return m_currentScore;
        }
    }

    int m_counterValue = 0;

    public Text scoreText;
    public float countTime = 1f;

    public ScoreMeter scoreMeter;

    LevelGoal m_levelGoal;

    void Start()
    {
        m_levelGoal = GetComponent<LevelGoal>();

        UpdateScoreText(m_currentScore);
    }

    public void UpdateScoreText(int scoreValue)
    {
        if(scoreText != null)
        {
            scoreText.text = scoreValue.ToString();
        }
    }

    public void AddScore(int value)
    {
        m_currentScore += value;

        StartCoroutine(CountScoreRoutine());
    }

    IEnumerator CountScoreRoutine()
    {
        int iterations = 0;

        while(m_counterValue < m_currentScore && iterations < 1000000)
        {
            UpdateScoreText(m_counterValue);

            iterations++;

            yield return null;

            m_counterValue = m_currentScore;

            UpdateScoreText(m_counterValue);
        }
    }
}
