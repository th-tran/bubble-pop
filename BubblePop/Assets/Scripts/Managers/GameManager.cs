﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelGoal))]
public class GameManager : Singleton<GameManager>
{
    public ScreenFader screenFader;
    public Text levelNameText;
    public Text movesLeftText;

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

    public MessageWindow messageWindow;

    public Sprite loseIcon;
    public Sprite winIcon;
    public Sprite goalIcon;

    public ScoreMeter scoreMeter;
    LevelGoal m_levelGoal;
    LevelGoalTimed m_levelGoalTimed;
    public LevelGoalTimed LevelGoalTimed
    {
        get
        {
            return m_levelGoalTimed;
        }
    }
    LevelGoalCollected m_levelGoalCollected;

    public override void Awake()
    {
        base.Awake();

        m_levelGoal = GetComponent<LevelGoal>();
        m_levelGoalTimed = GetComponent<LevelGoalTimed>();
        m_levelGoalCollected = GetComponent<LevelGoalCollected>();

        m_board = GameObject.FindObjectOfType<Board>().GetComponent<Board>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (scoreMeter != null)
        {
            scoreMeter.SetupStars(m_levelGoal);
        }

        Scene scene = SceneManager.GetActiveScene();

        if (levelNameText != null)
        {
            levelNameText.text = scene.name;
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
        if (m_levelGoalTimed == null)
        {
            if (movesLeftText != null)
            {
                movesLeftText.text = m_levelGoal.movesLeft.ToString();
            }
        }
        else
        {
            if (movesLeftText != null)
            {
                movesLeftText.text = "\u221E";
                movesLeftText.fontSize = 70;
            }
        }
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine(StartGameRoutine());
        yield return StartCoroutine(PlayGameRoutine());
        yield return StartCoroutine(WaitForBoardRoutine(0.5f));
        yield return StartCoroutine(EndGameRoutine());
    }

    public void BeginGame()
    {
        m_isReadyToBegin = true;
    }

    IEnumerator StartGameRoutine()
    {
        if (messageWindow != null)
        {
            messageWindow.GetComponent<RectXformMover>().MoveOn();
            messageWindow.ShowMessage(goalIcon, "score goal\n" + m_levelGoal.scoreGoals[0].ToString(), "start");
        }

        // Keep waiting until player is ready
        while (!m_isReadyToBegin)
        {
            yield return null;
        }

        if (screenFader != null)
        {
            screenFader.FadeOff();
        }

        yield return new WaitForSeconds(0.5f);
        if (m_board != null)
        {
            m_board.boardSetup.SetupBoard();
        }
    }

    IEnumerator PlayGameRoutine()
    {
        if (m_levelGoalTimed != null)
        {
            m_levelGoalTimed.StartCountdown();
        }

        while (!m_isGameOver)
        {
            m_isGameOver = m_levelGoal.IsGameOver();

            m_isWinner = m_levelGoal.IsWinner();

            yield return null;
        }
    }

    IEnumerator WaitForBoardRoutine(float delay = 0f)
    {
        if (m_levelGoalTimed != null)
        {
            if (m_levelGoalTimed.timer != null)
            {
                m_levelGoalTimed.timer.FadeOff();
                m_levelGoalTimed.timer.paused = true;
            }
        }

        if (m_board != null)
        {
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

        if (m_isWinner)
        {
            if (messageWindow != null)
            {
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowMessage(winIcon, "YOU WIN!", "OK");
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWinSound();
            }
        }
        else
        {
            if (messageWindow != null)
            {
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowMessage(loseIcon, "YOU LOSE!", "OK");
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLoseSound(0.5f);
            }
        }

        yield return new WaitForSeconds(1f);
        if (screenFader != null)
        {
            screenFader.FadeOn();
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

                if (scoreMeter != null)
                {
                    scoreMeter.UpdateScoreMeter(ScoreManager.Instance.CurrentScore, m_levelGoal.scoreStars);
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
        if (m_levelGoalTimed != null)
        {
            m_levelGoalTimed.AddTime(timeValue);
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
