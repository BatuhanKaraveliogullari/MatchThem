using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(LevelGoal))]
public class GameManager : Singleton<GameManager>
{
    Board m_board;
    LevelGoal m_levelGoal;

    public LevelGoal LevelGoal { get { return m_levelGoal; } }

    LevelGoalCollected m_levelGoalCollected;

    bool m_isReadyToBegin = false;
    bool m_isGameOver = false;

    public bool IsGameOver { get => m_isGameOver; set => m_isGameOver = value; }
    public bool IsWinner { get => m_isWinner; set => m_isWinner = value; }

    bool m_isWinner = false;
    bool m_isReadyToReload = false;

    public override void Awake()
    {
        base.Awake();

        m_levelGoal = GetComponent<LevelGoal>();

        m_levelGoalCollected = GetComponent<LevelGoalCollected>();

        m_board = GameObject.FindObjectOfType<Board>().GetComponent<Board>();
    }

    void Start()
    {
        if (UIManager.Insatance != null)
        {
            if (UIManager.Insatance.scoreMeter != null)
            {
                UIManager.Insatance.scoreMeter.SetupStars(m_levelGoal);
            }

            if (UIManager.Insatance.levelNameText != null)
            {
                Scene scene = SceneManager.GetActiveScene();

                UIManager.Insatance.levelNameText.text = scene.name;
            }

            if (m_levelGoalCollected != null)
            {
                UIManager.Insatance.EnableColletionGoalLayout(true);

                UIManager.Insatance.SetupCollectionGoalLayout(m_levelGoalCollected.collectionGoals);
            }

            else
            {
                UIManager.Insatance.EnableColletionGoalLayout(false);
            }

            bool useTimer = (m_levelGoal.levelCounter == LevelCounter.Timer);

            UIManager.Insatance.EnableTimer(useTimer);

            UIManager.Insatance.EnableMovesCounter(!useTimer);
        }

        m_levelGoal.movesLeft++;

        UpdateMoves();

        StartCoroutine(ExecuteGameLoop());
    }

    public void UpdateMoves()
    {
        if (m_levelGoal.levelCounter == LevelCounter.Moves)
        {
            m_levelGoal.movesLeft--;

            if (UIManager.Insatance != null && UIManager.Insatance.movesLeftText != null)
            {
                UIManager.Insatance.movesLeftText.text = m_levelGoal.movesLeft.ToString();
            }
        }
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("WaitForBoardRoutine", 0.5f);
        yield return StartCoroutine("EndGameRoutine");
    }

    public void BeginGame()
    {
        m_isReadyToBegin = true;
    }

    IEnumerator StartGameRoutine()
    {
        if (UIManager.Insatance != null && UIManager.Insatance.messageWindow != null)
        {
            UIManager.Insatance.messageWindow.GetComponent<RectXformMover>().MoveOn();

            int maxGoal = m_levelGoal.scoreGoals.Length - 1;

            UIManager.Insatance.messageWindow.ShowScoreMessage(m_levelGoal.scoreGoals[maxGoal]);

            if(m_levelGoal.levelCounter == LevelCounter.Timer)
            {
                UIManager.Insatance.messageWindow.ShowTimedGoal(m_levelGoal.timeLeft);
            }

            else
            {
                UIManager.Insatance.messageWindow.ShowMovesGoal(m_levelGoal.movesLeft);
            }

            if(m_levelGoalCollected != null)
            {
                UIManager.Insatance.messageWindow.ShowCollectionGoal(true);

                GameObject goalLayout = UIManager.Insatance.messageWindow.collectionGoalLayout;

                if(goalLayout != null)
                {
                    UIManager.Insatance.SetupCollectionGoalLayout(m_levelGoalCollected.collectionGoals, goalLayout, 80);
                }
            }
        }

        while (!m_isReadyToBegin)
        {
            yield return null;
        }

        if (UIManager.Insatance != null && UIManager.Insatance.screenFader != null)
        {
            UIManager.Insatance.screenFader.FadeOff();
        }

        yield return new WaitForSeconds(0.5f);

        if (m_board != null)
        {
            m_board.SetupBoard();
        }
    }

    IEnumerator PlayGameRoutine()
    {
        if (m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            m_levelGoal.StartCountdown();
        }
        while (!IsGameOver)
        {
            m_isGameOver = m_levelGoal.IsGameOver();

            IsWinner = m_levelGoal.IsWinner();

            yield return null;
        }
    }

    IEnumerator WaitForBoardRoutine(float delay = 0f)
    {
        if (m_levelGoal.levelCounter == LevelCounter.Timer && UIManager.Insatance.timer != null)
        {
            if (UIManager.Insatance.timer != null)
            {
                UIManager.Insatance.timer.FadeOff();

                UIManager.Insatance.timer.paused = true;
            }
        }

        if (m_board != null)
        {
            yield return new WaitForSeconds(m_board.swapTime);

            while (m_board.isRefilling)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(delay);
    }

    IEnumerator EndGameRoutine()
    {
        m_isReadyToReload = false;

        if (IsWinner)
        {
            if(m_levelGoal != null)
            {
                if (m_levelGoal.levelCounter == LevelCounter.Moves)
                {
                    if (m_levelGoal.movesLeft != 0)
                    {
                        ScoreManager.Insatance.AddScore(m_levelGoal.movesLeft * 60);

                        yield return new WaitForSeconds(1f);
                    }
                }
                else
                {
                    if (m_levelGoal.timeLeft != 0)
                    {
                        ScoreManager.Insatance.AddScore(m_levelGoal.timeLeft * 10);

                        yield return new WaitForSeconds(1f);
                    }
                }
            }

            ShowWinScreen();
        }

        else
        {
            ShowLoseScreen();
        }

        yield return new WaitForSeconds(1f);

        if (UIManager.Insatance != null && UIManager.Insatance.screenFader != null)
        {
            UIManager.Insatance.screenFader.FadeOn();
        }

        while (!m_isReadyToReload)
        {
            yield return null;

            if (m_isReadyToReload)
            {
                break;
            }
        }

        if(MenuManager.Insatance != null)
        {
            MenuManager.Insatance.StartGame();
        }
    }

    void ShowWinScreen()
    {
        if (UIManager.Insatance != null && UIManager.Insatance.messageWindow != null)
        {
            UIManager.Insatance.messageWindow.GetComponent<RectXformMover>().MoveOn();

            UIManager.Insatance.messageWindow.ShowWinMessage();

            UIManager.Insatance.messageWindow.ShowCollectionGoal(false);

            if(ScoreManager.Insatance != null)
            {
                string scoreStr = "you scored\n" + ScoreManager.Insatance.CurrentScore.ToString() + " points!";

                UIManager.Insatance.messageWindow.ShowGoalCaption(scoreStr, 0, 70);
            }

            if(UIManager.Insatance.messageWindow.goalCompleteIcon != null)
            {
                UIManager.Insatance.messageWindow.ShowGoalImage(UIManager.Insatance.messageWindow.goalCompleteIcon);
            }
        }

        if (SoundManager.Insatance != null)
        {
            SoundManager.Insatance.PlayWinSound();
        }
    }

    void ShowLoseScreen()
    {
        if (UIManager.Insatance != null && UIManager.Insatance.messageWindow != null)
        {
            UIManager.Insatance.messageWindow.GetComponent<RectXformMover>().MoveOn();

            UIManager.Insatance.messageWindow.ShowLoseMessage();

            UIManager.Insatance.messageWindow.ShowCollectionGoal(false);

            string caption = "";

            if(m_levelGoal.levelCounter == LevelCounter.Timer)
            {
                caption = "Out of time!";
            }

            else
            {
                caption = "Out of moves!";
            }

            UIManager.Insatance.messageWindow.ShowGoalCaption(caption, 0, 70);

            if(UIManager.Insatance.messageWindow.goalFailedIcon != null)
            {
                UIManager.Insatance.messageWindow.ShowGoalImage(UIManager.Insatance.messageWindow.goalFailedIcon);
            }
        }

        if (SoundManager.Insatance != null)
        {
            SoundManager.Insatance.PlayLoseSound();
        }
    }

    public void ReloadScene()
    {
        m_isReadyToReload = true;
    }

    public void ScorePoints(GamePiece piece, int multiplier = 1, int bonus = 0)
    {
        if (piece != null)
        {
            if (ScoreManager.Insatance != null)
            {
                ScoreManager.Insatance.AddScore((piece.scoreValue * multiplier) + bonus);

                m_levelGoal.UpdateScoreStars(ScoreManager.Insatance.CurrentScore);

                if (UIManager.Insatance != null && UIManager.Insatance.scoreMeter != null)
                {
                    UIManager.Insatance.scoreMeter.UpdateScoreMeter(ScoreManager.Insatance.CurrentScore, m_levelGoal.scoreStars);
                }
            }

            if (SoundManager.Insatance != null && piece.clearSound != null)
            {
                SoundManager.Insatance.PlayClipAtPoint(piece.clearSound, Vector3.zero, SoundManager.Insatance.fxVolume);
            }
        }
    }

    public void AddTime(int timeValue)
    {
        if (m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            m_levelGoal.AddTime(timeValue);
        }
    }

    public void UpdateCollectionGoals(GamePiece pieceToCheck)
    {
        if(m_levelGoalCollected != null)
        {
            m_levelGoalCollected.UpdateGoals(pieceToCheck);
        }
    }
}
