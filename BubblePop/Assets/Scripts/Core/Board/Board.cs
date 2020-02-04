using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A generic GameObject that can be positioned at coordinate (x,y,z) when the game begins
[System.Serializable]
public class StartingObject
{
    public GameObject prefab;
    public int x;
    public int y;
    public int z;
}

[RequireComponent(typeof(BoardBomber))]
[RequireComponent(typeof(BoardClearer))]
[RequireComponent(typeof(BoardCollapser))]
[RequireComponent(typeof(BoardDeadlock))]
[RequireComponent(typeof(BoardFiller))]
[RequireComponent(typeof(BoardHighlighter))]
[RequireComponent(typeof(BoardInput))]
[RequireComponent(typeof(BoardMatcher))]
[RequireComponent(typeof(BoardQuery))]
[RequireComponent(typeof(BoardSetup))]
[RequireComponent(typeof(BoardShuffler))]
[RequireComponent(typeof(BoardTiles))]
public class Board : MonoBehaviour
{
    // Dimensions of Board
    public int width;
    public int height;

    // Margin outside Board for calculating camera field of view
    public int borderSize;

    // Prefab representing a single Tile
    public GameObject tileNormalPrefab;
    // Prefab representing an empty, unoccupied Tile
    public GameObject tileObstaclePrefab;
    // Array of Bubble Prefabs
    public GameObject[] bubblePrefabs;

    // Prefabs representing Bombs
    public GameObject[] adjacentBombPrefabs;
    public GameObject[] columnBombPrefabs;
    public GameObject[] rowBombPrefabs;
    public GameObject colorBombPrefab;

    GameObject m_clickedTileBomb;
    GameObject m_targetTileBomb;

    public int maxBlockers = 3;
    public int blockerCount = 0;
    [Range(0,1)]
    public float chanceForBlocker = 0.1f;
    public GameObject[] blockerPrefabs;

    // The time required to swap Bubbles between the target and clicked Tile
    float m_swapTime = 0.5f;
    public float SwapTime
    {
        get
        {
            return m_swapTime;
        }
    }
    // The base delay between events
    float m_delay = 0.2f;

    // Array of all the Board's Tiles
    public Tile[,] allTiles;
    // Array of all of the Board's Bubbles
    public Bubble[,] allBubbles;

    // Tile first clicked by mouse
    public Tile clickedTile;
    // Adjacent Tile dragged into by mouse
    public Tile targetTile;

    // Whether user input is currently allowed
    public bool playerInputEnabled = true;

    // Manually positioned Tiles, placed before the Board is filled
    public StartingObject[] startingTiles;
    // Manually positioned Bubbles, placed before the Board is filled
    public StartingObject[] startingBubbles;

    // Y Offset used to make the bubbles "fall" into place to fill the Board
    public int fillYOffset = 10;
    // Time used to fill the Board
    public float fillMoveTime = 0.5f;
    public float collapseMoveTime = 0.1f;

    public bool isRefilling = false;

    public int scoreMultiplier = 0;

    // References to Board components
    public BoardBomber boardBomber;
    public BoardClearer boardClearer;
    public BoardCollapser boardCollapser;
    public BoardDeadlock boardDeadlock;
    public BoardFiller boardFiller;
    public BoardHighlighter boardHighlighter;
    public BoardInput boardInput;
    public BoardMatcher boardMatcher;
    public BoardQuery boardQuery;
    public BoardSetup boardSetup;
    public BoardShuffler boardShuffler;
    public BoardTiles boardTiles;

    void Awake()
    {
        boardBomber = GetComponent<BoardBomber>();
        boardClearer = GetComponent<BoardClearer>();
        boardCollapser = GetComponent<BoardCollapser>();
        boardDeadlock = GetComponent<BoardDeadlock>();
        boardFiller = GetComponent<BoardFiller>();
        boardHighlighter = GetComponent<BoardHighlighter>();
        boardInput = GetComponent<BoardInput>();
        boardMatcher = GetComponent<BoardMatcher>();
        boardQuery = GetComponent<BoardQuery>();
        boardSetup = GetComponent<BoardSetup>();
        boardShuffler = GetComponent<BoardShuffler>();
        boardTiles = GetComponent<BoardTiles>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize array of Tiles
        allTiles = new Tile[width,height];
        // initialize array of Bubbles
        allBubbles = new Bubble[width,height];
    }

    public void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        // If player input is enabled...
        if (playerInputEnabled && !GameManager.Instance.IsGameOver)
        {
            // ...set the corresponding Bubbles to the clicked Tile and target Tile
            Bubble clickedBubble = allBubbles[clickedTile.xIndex, clickedTile.yIndex];
            Bubble targetBubble = allBubbles[targetTile.xIndex, targetTile.yIndex];

            if (clickedBubble != null && targetBubble != null)
            {
                // Move the clicked Bubble to the target Bubble and vice versa
                clickedBubble.Move(targetTile.xIndex, targetTile.yIndex, m_swapTime);
                targetBubble.Move(clickedTile.xIndex, clickedTile.yIndex, m_swapTime);

                // Wait for the swap time
                yield return new WaitForSeconds(m_swapTime);

                // Find all matches for each Bubble after the swap
                List<Bubble> clickedBubbleMatches = boardMatcher.FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<Bubble> targetBubbleMatches = boardMatcher.FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

                // Check if color bomb was triggered, and if so get the list of corresponding bubbles
                List<Bubble> colorMatches = new List<Bubble>();
                if (boardQuery.IsColorBomb(clickedBubble) && !boardQuery.IsColorBomb(targetBubble))
                {
                    clickedBubble.matchValue = targetBubble.matchValue;
                    colorMatches = boardMatcher.FindAllMatchValue(clickedBubble.matchValue);
                }
                else if (!boardQuery.IsColorBomb(clickedBubble) && boardQuery.IsColorBomb(targetBubble))
                {
                    targetBubble.matchValue = clickedBubble.matchValue;
                    colorMatches = boardMatcher.FindAllMatchValue(targetBubble.matchValue);
                }
                else if (boardQuery.IsColorBomb(clickedBubble) && boardQuery.IsColorBomb(targetBubble))
                {
                    foreach(Bubble bubble in allBubbles)
                    {
                        if (!colorMatches.Contains(bubble))
                        {
                            colorMatches.Add(bubble);
                        }
                    }
                }

                // If no matches are found, then swap the Bubbles back
                if (clickedBubbleMatches.Count == 0 && targetBubbleMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedBubble.Move(clickedTile.xIndex, clickedTile.yIndex, m_swapTime);
                    targetBubble.Move(targetTile.xIndex, targetTile.yIndex, m_swapTime);

                    yield return new WaitForSeconds(m_swapTime);
                }
                else
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.DecrementMoves();
                    }
                    // Clear matches and refill the Board
                    Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);
                    // Drop bomb in-place
                    m_clickedTileBomb = boardBomber.DropBomb(clickedTile.xIndex, clickedTile.yIndex, swipeDirection, clickedBubbleMatches);
                    m_targetTileBomb = boardBomber.DropBomb(targetTile.xIndex, targetTile.yIndex, swipeDirection, targetBubbleMatches);

                    // Change bomb color to match
                    if (m_clickedTileBomb != null && targetBubble != null)
                    {
                        Bomb clickedBomb = m_clickedTileBomb.GetComponent<Bomb>();
                        if (!boardQuery.IsColorBomb(clickedBomb))
                        {
                            clickedBomb.ChangeColor(targetBubble);
                        }
                    }

                    if (m_targetTileBomb != null && clickedBubble != null)
                    {
                        Bomb targetBomb = m_targetTileBomb.GetComponent<Bomb>();
                        if (!boardQuery.IsColorBomb(targetBomb))
                        {
                            targetBomb.ChangeColor(clickedBubble);
                        }
                    }

                    // Add short pause if bomb was generated
                    if (m_clickedTileBomb != null || m_targetTileBomb != null)
                    {
                        yield return new WaitForSeconds(m_delay * 0.5f);
                    }

                    List<Bubble> bubblesToClear = clickedBubbleMatches.Union(targetBubbleMatches).ToList()
                                                                      .Union(colorMatches).ToList();
                    ClearAndRefillBoard(bubblesToClear);
                }
            }
        }
    }
    public void ClearAndRefillBoard(List<Bubble> bubbles)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(bubbles));
    }

    public IEnumerator ClearAndRefillBoardRoutine(List<Bubble> bubbles)
    {
        // Disable player input while the Board is collapsing/refilling
        playerInputEnabled = false;
        isRefilling = true;

        // Create a new List of Bubbles, using the initial list as a starting point
        List<Bubble> matches = bubbles;
        do
        {
            scoreMultiplier = 1;
            // Run the coroutine to clear the Board and collapse any columns to fill in the spaces
            yield return StartCoroutine(ClearAndProcessRoutine(matches));
            // Refill empty spaces from collapsing
            yield return StartCoroutine(boardFiller.RefillRoutine());
            // Find any subsequent matches and repeat the process
            matches = boardMatcher.FindAllMatches();
            if (matches.Count > 0)
            {
                Debug.Log(matches.Count);
            }
        }
        while (matches.Count != 0);

        if (boardDeadlock.IsDeadlocked())
        {
            yield return new WaitForSeconds(m_delay);
            yield return StartCoroutine(boardShuffler.ShuffleBoardRoutine());
        }

        // Re-enable player input
        playerInputEnabled = true;
        isRefilling = false;
    }

    IEnumerator ClearAndProcessRoutine(List<Bubble> bubblesToClear)
    {
        // List of Bubbles to move
        List<Bubble> movingBubbles = new List<Bubble>();
        // List of Bubbles that form matches
        List<Bubble> matches = new List<Bubble>();

        //HighlightBubbles(bubbles);

        bool isFinished = false;
        while (!isFinished)
        {
            // Trigger all bombs
            int oldCount;
            List<Bubble> bombedBubbles = new List<Bubble>();
            do
            {
                // Keep track of number of Bubbles affected before bomb triggers
                oldCount = bubblesToClear.Count;
                // Find Bubbles affected by bombs...
                bombedBubbles = boardQuery.GetBombedBubbles(bubblesToClear);
                // ...and add to list of Bubbles to clear
                bubblesToClear = bubblesToClear.Union(bombedBubbles).ToList();
            }
            while (oldCount < bubblesToClear.Count);

            // Add any heavy blockers that hit bottom of Board to list of bubbles to clear
            List<Blocker> bottomBlockers = boardQuery.FindBlockersAt(0, true);
            // Get list of blockers to be cleared
            List<Blocker> allBlockers = boardQuery.FindAllBlockers();
            List<Blocker> blockersToClear = allBlockers.Intersect(bubblesToClear).Cast<Blocker>().ToList();
            blockersToClear = blockersToClear.Union(bottomBlockers).ToList();
            blockerCount -= blockersToClear.Count;

            bubblesToClear = bubblesToClear.Union(blockersToClear).ToList();

            List<int> columnsToCollapse = boardQuery.GetColumns(bubblesToClear);

            // Clear the Bubbles
            boardClearer.ClearBubbleAt(bubblesToClear, bombedBubbles);
            // Break any Tiles under the cleared Bubbles
            boardTiles.BreakTileAt(bubblesToClear);

            // Activate any previously generated bombs
            if (m_clickedTileBomb != null)
            {
                boardBomber.ActivateBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }

            if (m_targetTileBomb != null)
            {
                boardBomber.ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }

            yield return new WaitForSeconds(m_delay);

            // Collapse any columns with empty spaces and keep track of what Bubbles moved as a result
            movingBubbles = boardCollapser.CollapseColumn(columnsToCollapse, collapseMoveTime);

            // Wait while these Bubbles fill in the gaps
            while (!boardQuery.IsCollapsed(movingBubbles))
            {
                yield return null;
            }

            // Find any matches that form from collapsing
            matches = boardMatcher.FindMatchesAt(movingBubbles);
            // Check if any blockers fell to bottom
            blockersToClear = boardQuery.FindBlockersAt(0, true);
            matches = matches.Union(blockersToClear).ToList();

            // If no matches are formed from the collapse, then finish
            if (matches.Count == 0)
            {
                isFinished = true;
            }
            // Otherwise, repeat this process again
            else
            {
                scoreMultiplier++;
                if (scoreMultiplier >= 3)
                {
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayBonusSound();
                    }
                }
                yield return StartCoroutine(ClearAndProcessRoutine(matches));
            }
        }
    }
}
