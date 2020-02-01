using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchValue
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Teal,
    Purple,
    Wild,
    None
}

[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public Color color;

    Board m_board;

    bool m_isMoving = false;

    public InterpolationType interpolation = InterpolationType.SmootherStep;

    public enum InterpolationType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    };

    public MatchValue matchValue;

    public int scoreValue = 20;

    public AudioClip clearSound;

    void Awake()
    {
        color = GetComponent<SpriteRenderer>().color;
    }

    public void Init(Board board)
    {
        m_board = board;
    }

    public void SetCoordinates(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Move(int destX, int destY, float timeToMove = 0.1f)
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
        }
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove = 0.1f)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination = false;
        float elapsedTime = 0f;

        m_isMoving = true;

        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true;
                if (m_board != null)
                {
                    m_board.boardFiller.PlaceBubble(this, (int) destination.x, (int) destination.y);
                }
                break;
            }

            // Track total running time and
            // use it to calculate lerp value
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

            // Set movement curve
            switch (interpolation)
            {
                case InterpolationType.Linear:
                    break;
                case InterpolationType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.SmoothStep:
                    t = t*t*(3 - 2*t);
                    break;
                case InterpolationType.SmootherStep:
                    t = t*t*t*(t*(t*6 - 15) + 10);
                    break;
            }
            // Move the bubble
            transform.position = Vector3.Lerp(startPosition, destination, t);

            // Wait until next frame
            yield return null;
        }

        m_isMoving = false;
    }

    public void ChangeColor(Bubble bubbleToMatch)
    {
        SpriteRenderer rendererToChange = GetComponent<SpriteRenderer>();

        Color colorToMatch = Color.clear;

        if (bubbleToMatch != null)
        {
            SpriteRenderer rendererToMatch = bubbleToMatch.GetComponent<SpriteRenderer>();

            if (rendererToMatch != null && rendererToChange != null)
            {
                rendererToChange.color = rendererToMatch.color;
            }

            matchValue = bubbleToMatch.matchValue;
        }
    }
}
