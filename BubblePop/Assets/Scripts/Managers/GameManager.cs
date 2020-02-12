using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelGoal))]
public class GameManager : Singleton<GameManager>
{
    Board m_board;

    bool m_isReadyToBegin = false;
    bool m_isReadyToReload = false;
    bool m_isGameOver = false;
    public bool IsGameOver
    {
        get
        {
            return m_isGameOver;
        }
    }
    bool m_isWinner = false;

    LevelGoal m_levelGoal;
    public LevelGoal LevelGoal
    {
        get
        {
            return m_levelGoal;
        }
    }
    LevelGoalCollected m_levelGoalCollected;

    public override void Awake()
    {
        base.Awake();

        m_levelGoal = GetComponent<LevelGoal>();
        m_levelGoalCollected = GetComponent<LevelGoalCollected>();

        m_board = GameObject.FindObjectOfType<Board>().GetComponent<Board>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.scoreMeter != null)
            {
                UIManager.Instance.scoreMeter.SetupStars(m_levelGoal);
            }

            if (UIManager.Instance.levelNameText != null)
            {
                Scene scene = SceneManager.GetActiveScene();
                UIManager.Instance.levelNameText.text = scene.name;
            }

            if (m_levelGoalCollected != null)
            {
                UIManager.Instance.EnableCollectionGoalLayout(true);
                UIManager.Instance.SetupCollectionGoalLayout(m_levelGoalCollected.collectionGoals);
            }
            else
            {
                UIManager.Instance.EnableCollectionGoalLayout(false);
            }

            bool useTimer = (m_levelGoal.levelCounter == LevelCounter.Timer);
            UIManager.Instance.EnableTimer(useTimer);
            UIManager.Instance.EnableMovesCounter(!useTimer);
        }

        UpdateMoves();

        StartCoroutine(ExecuteGameLoop());
    }

    public void DecrementMoves()
    {
        m_levelGoal.movesLeft--;
        UpdateMoves();
    }

    public void UpdateMoves()
    {
        if (m_levelGoal.levelCounter == LevelCounter.Moves)
        {
            if (UIManager.Instance != null && UIManager.Instance.movesLeftText != null)
            {
                UIManager.Instance.movesLeftText.text = m_levelGoal.movesLeft.ToString();
            }
        }
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine(StartGameRoutine());
        yield return StartCoroutine(PlayGameRoutine());
        yield return StartCoroutine(EndGameRoutine());
    }

    public void BeginGame()
    {
        m_isReadyToBegin = true;
    }

    IEnumerator StartGameRoutine()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.messageWindow != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
                int maxGoal = m_levelGoal.scoreGoals.Length - 1;
                UIManager.Instance.messageWindow.ShowScoreMessage(m_levelGoal.scoreGoals[maxGoal]);

                if (m_levelGoal.levelCounter == LevelCounter.Timer)
                {
                    UIManager.Instance.messageWindow.ShowTimedGoal(m_levelGoal.timeLeft);
                }
                else
                {
                    UIManager.Instance.messageWindow.ShowMovesGoal(m_levelGoal.movesLeft);
                }

                if (m_levelGoalCollected != null)
                {
                    UIManager.Instance.messageWindow.ShowCollectionGoal();
                    GameObject goalLayout = UIManager.Instance.messageWindow.collectionGoalLayout;
                    if (goalLayout != null)
                    {
                        UIManager.Instance.SetupCollectionGoalLayout(m_levelGoalCollected.collectionGoals, goalLayout, 80);
                    }
                }
            }
        }

        // Keep waiting until player is ready
        while (!m_isReadyToBegin)
        {
            yield return null;
        }

        if (UIManager.Instance != null && UIManager.Instance.screenFader != null)
        {
            UIManager.Instance.screenFader.FadeOff();
        }

        yield return new WaitForSeconds(0.5f);
        if (m_board != null)
        {
            m_board.boardSetup.SetupBoard();
        }
    }

    IEnumerator PlayGameRoutine()
    {
        if (m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            m_levelGoal.StartCountdown();
        }

        while (!m_isGameOver)
        {
            m_isGameOver = m_levelGoal.IsGameOver();

            yield return null;
        }

        if (m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            m_levelGoal.StopCountdown();
        }

        yield return StartCoroutine(WaitForBoardRoutine(0.5f));

        m_isWinner = m_levelGoal.IsWinner();
    }

    IEnumerator WaitForBoardRoutine(float delay = 0f)
    {
        if (m_levelGoal.levelCounter == LevelCounter.Timer)
        {
            if (UIManager.Instance != null && UIManager.Instance.timer != null)
            {
                UIManager.Instance.timer.paused = true;
                UIManager.Instance.timer.FadeOff();
            }
        }

        if (m_board != null)
        {
            while (m_board.isBusy)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(delay);
    }

    IEnumerator EndGameRoutine()
    {
        m_isReadyToReload = false;

        if (m_isWinner)
        {
            if (UIManager.Instance != null && UIManager.Instance.messageWindow != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
                UIManager.Instance.messageWindow.ShowWinMessage();
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWinSound();
            }
        }
        else
        {
            if (UIManager.Instance != null && UIManager.Instance.messageWindow != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
                UIManager.Instance.messageWindow.ShowLoseMessage();
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLoseSound(0.5f);
            }
        }

        yield return new WaitForSeconds(1f);
        if (UIManager.Instance != null && UIManager.Instance.screenFader != null)
        {
            UIManager.Instance.screenFader.FadeOn();
        }

        while (!m_isReadyToReload)
        {
            yield return null;
        }

        // TODO: Replace with proper retry option or return to menu
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReloadScene()
    {
        m_isReadyToReload = true;
    }

    public void ScorePoints(Bubble bubble, int multiplier = 1, int bonus = 0)
    {
        if (bubble != null)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(bubble.scoreValue * multiplier + bonus);
                m_levelGoal.UpdateScoreStars(ScoreManager.Instance.CurrentScore);

                if (UIManager.Instance != null && UIManager.Instance.scoreMeter != null)
                {
                    UIManager.Instance.scoreMeter.UpdateScoreMeter(ScoreManager.Instance.CurrentScore, m_levelGoal.scoreStars);
                }

                if (bubble.clearSound != null)
                {
                    SoundManager.Instance.PlayClipAtPoint(bubble.clearSound, Vector3.zero, SoundManager.Instance.fxVolume, randomizePitch: true);
                }
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

    public void UpdateCollectionGoals(Bubble bubbleToCheck)
    {
        if (m_levelGoalCollected != null)
        {
            m_levelGoalCollected.UpdateGoals(bubbleToCheck);
        }
    }
}
