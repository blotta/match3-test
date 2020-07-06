using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    // TODO: change static methods, variables and usage to the singleton
    // usage where it makes sense
    private static BoardManager _instance;
    public static BoardManager Instance { get { return _instance; } }

    public int width;
    public int height;

    public GameObject[,] gemGOGrid;

    private Rect worldBoardBound;
    
    public GameObject gemPrefab;

    [SerializeField] static float DistanceBetweenGems = 1f;

    private StateMachine stateMachine = new StateMachine();

    private bool waitPlayerInput = false;

    public static event Action<float, float> OnScoreUpdated = delegate { };
    public static event Action<float> OnTimerUpdated = delegate { };

    public float baseTargetPoints;
    public float baseTargetPointsRoundMultiplier;
    public int round;

    [SerializeField] private float _countdownSeconds;
    public float countdownSeconds
    {
        get { return _countdownSeconds; }
        set
        {
            _countdownSeconds = value;
            OnTimerUpdated(_countdownSeconds);
        }
    }

    private float _targetScore;

    private float _score;
    public float score
    {
        get { return _score; }
        set
        {
            _score = value;
            Debug.Log($"Broadcasting score: {_score}");
            OnScoreUpdated(_score, _targetScore);
        }
    }

    private void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
        }

        gemGOGrid = new GameObject[width, height];
    }

    private void Start()
    {
        ResetGemGOGrid(); // Sets gemGOGrid and worldBoardBound

        waitPlayerInput = true;
        score = 0f;
        _targetScore = baseTargetPoints * round * baseTargetPointsRoundMultiplier;

        this.stateMachine.ChangeState(new PlayerTurn(gemGOGrid, worldBoardBound, OnInputEnded));
    }

    private void Update()
    {
        countdownSeconds -= Time.deltaTime;
        this.stateMachine.ExecuteStateUpdate();
    }

    private void OnInputEnded()
    {
        this.stateMachine.ChangeState(new ProcessTurn(ref gemGOGrid, OnProcessEnded));
    }

    private void OnProcessEnded(List<List<Vector2Int>> matches)
    {
        if (matches.Count > 0)
        {
            waitPlayerInput = false;
            gemGOGrid = BoardManager.UpdateGemGOGridFromWorldPos(gemGOGrid);
            this.stateMachine.ChangeState(new DeleteMatches(ref gemGOGrid, matches, OnDeleteMatchesDone));
        }
        else
        {
            // invalid move. Return gems to original grid position
            waitPlayerInput = true;
            this.stateMachine.ChangeState(new AnimGems(gemGOGrid, OnAnimGemsEnded));
        }
    }

    private void OnAnimGemsEnded()
    {
        if (waitPlayerInput)
        {
            this.stateMachine.ChangeState(new PlayerTurn(gemGOGrid, worldBoardBound, OnInputEnded));
            waitPlayerInput = false;
        }
        else
        {
            this.stateMachine.ChangeState(new ProcessTurn(ref gemGOGrid, OnProcessEnded));
        }
    }

    private void OnDeleteMatchesDone(List<List<Vector2Int>> matches)
    {
        this.stateMachine.ChangeState(new PushDownAndCreateGems(ref gemGOGrid, matches, OnPushDownAndCreateGemsDone));
    }

    private void OnPushDownAndCreateGemsDone()
    {
        this.stateMachine.ChangeState(new AnimGems(gemGOGrid, OnAnimGemsEnded));
    }

    public void ShuffleJustBecause()
    {
        var (newGrid, diffs) = BoardUtils.ShuffleDiff(BoardManager.GemArrayFromGridPos(gemGOGrid), true);
        GameObject[,] newGOGrid = new GameObject[newGrid.GetLength(0), newGrid.GetLength(1)];
        foreach (var diff in diffs)
        {
            var oldPos = diff.Key;
            var newPos = diff.Value;
            newGOGrid[newPos.x, newPos.y] = gemGOGrid[oldPos.x, oldPos.y];
            newGOGrid[newPos.x, newPos.y].GetComponent<Gem>().gridPos = newPos;
            // world position will be updated within the AnimGems state
        }
        gemGOGrid = newGOGrid;
        this.stateMachine.ChangeState(new AnimGems(gemGOGrid, OnAnimGemsEnded));
    }

    void ResetGemGOGrid()
    {
        // Destroy GOs in current grid
        if (gemGOGrid != null)
        {
            foreach (GameObject oldGem in gemGOGrid)
            {
                if (oldGem != null)
                {
                    Destroy(oldGem);
                }
            }
        }

        // Create new GOs
        Gem.GemType[,] gemGrid = BoardUtils.GetNewPlayableGrid(width, height);
        gemGOGrid = new GameObject[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // TODO: replace with GemFactory
                Vector3 worldPos = GridToWorldPos(new Vector2Int(i, j));
                GameObject gem = Instantiate(gemPrefab, worldPos, Quaternion.identity, transform);
                gem.GetComponent<Gem>().gemType = gemGrid[i, j];
                gem.GetComponent<SpriteRenderer>().sprite = GemManager.Instance.GetGemOfType(gemGrid[i, j]).Sprite;
                // gem.GetComponent<Gem>().originalPosition = gem.transform.position;
                gem.GetComponent<Gem>().gridPos = new Vector2Int(i, j);
                gem.name = $"-- {gemGrid[i, j]} --";
                gemGOGrid[i, j] = gem;
            }
        }

        // World Bounds
        var blGem = gemGOGrid[0, 0];
        var blGemBounds = blGem.GetComponent<SpriteRenderer>().sprite.bounds;
        var bottomLeftBound = new Vector2(blGem.transform.position.x - blGemBounds.extents.x, blGem.transform.position.y - blGemBounds.extents.y);

        var trGem = gemGOGrid[gemGOGrid.GetLength(0) - 1, gemGOGrid.GetLength(1) - 1];
        var trGemBounds = trGem.GetComponent<SpriteRenderer>().sprite.bounds;
        var topRightBound = new Vector2(trGem.transform.position.x + trGemBounds.extents.x, trGem.transform.position.y + trGemBounds.extents.y);

        Vector2 boundSize = topRightBound - bottomLeftBound;

        worldBoardBound = new Rect(bottomLeftBound, boundSize);

        // print($"TR Bounds: {trGemBounds}");
        // print($"Bottom Left Bounds: {bottomLeftBound} ; Top Right Bounds: {topRightBound}");
        // print($"World Bounds: {worldBoardBound}");
    }

    public static GameObject[,] UpdateGemGOGridFromWorldPos(GameObject[,] oldGrid)
    {
        GameObject[,] newGrid = new GameObject[oldGrid.GetLength(0), oldGrid.GetLength(1)];
        for (int i = 0; i < newGrid.GetLength(0); i++)
        {
            for (int j = 0; j < newGrid.GetLength(1); j++)
            {
                newGrid[i, j] = null;
            }
        }

        foreach (var gem in oldGrid)
        {
            if (gem != null)
            {
                Vector2Int gridPos = BoardManager.WorldToGridPos(gem.transform.position);
                gem.GetComponent<Gem>().gridPos = gridPos;
                newGrid[gridPos.x, gridPos.y] = gem;
            }
        }
        return newGrid;
    }

    public static GameObject[] GetGemColumnS(GameObject gem, GameObject[,] grid)
    {
        GameObject[] goCol = new GameObject[grid.GetLength(1)];
        int i = WorldToGridPos(gem.transform.position).x;
        for (int j = 0; j < grid.GetLength(1); j++)
        {
            goCol[j] = grid[i, j];
        }
        return goCol;
    }

    public static GameObject[] GetGemRowS(GameObject gem, GameObject[,] grid)
    {
        GameObject[] goRow = new GameObject[grid.GetLength(0)];
        int j = WorldToGridPos(gem.transform.position).y;
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            goRow[i] = grid[i, j];
        }
        return goRow;
    }

    public static Vector2Int WorldToGridPos(Vector3 worldPos)
    {
        int gridX = (int)Math.Round(worldPos.x / BoardManager.DistanceBetweenGems);
        int gridY = (int)Math.Round(worldPos.y / BoardManager.DistanceBetweenGems);
        return new Vector2Int(gridX, gridY);
    }

    public static Vector3 GridToWorldPos(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * BoardManager.DistanceBetweenGems, gridPos.y * BoardManager.DistanceBetweenGems, 0);
    }

    public static Vector3 SnappedGridPos(Vector3 worldPos)
    {
        return GridToWorldPos(WorldToGridPos(worldPos));
    }

    public static Gem.GemType[,] GemArrayFromWorldPos(GameObject[,] grid)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        Gem.GemType[,] gtGrid = new Gem.GemType[w, h];

        foreach (GameObject gem in grid)
        {
            Vector2Int gridPos = WorldToGridPos(gem.transform.position);
            gtGrid[gridPos.x, gridPos.y] = gem.GetComponent<Gem>().gemType;
        }

        return gtGrid;
    }

    public static Gem.GemType[,] GemArrayFromGridPos(GameObject[,] grid)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        Gem.GemType[,] gtGrid = new Gem.GemType[w, h];

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                gtGrid[i, j] = grid[i, j].GetComponent<Gem>().gemType;
            }
        }

        return gtGrid;
    }

    public GameObject GemFactory(Gem.GemType type, Vector2Int gridPos, Vector3 initialWorldPos, bool updateGrid = false)
    {
        SOGem newGem = GemManager.Instance.GetGemOfType(type);
        GameObject gem = Instantiate(
            BoardManager.Instance.gemPrefab,
            initialWorldPos,
            Quaternion.identity,
            BoardManager.Instance.transform);
        gem.GetComponent<Gem>().gemType = newGem.GemType;
        gem.GetComponent<SpriteRenderer>().sprite = newGem.Sprite;
        gem.GetComponent<Gem>().gridPos = gridPos;
        gem.name = $"-- {gem.GetComponent<Gem>().gemType} --";

        if (updateGrid)
            BoardManager.Instance.gemGOGrid[gridPos.x, gridPos.y] = gem;

        return gem;
    }
}
