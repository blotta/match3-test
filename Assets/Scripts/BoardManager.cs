using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardManager : MonoBehaviour
{
    Board board;

    public GameObject[,] gemGOGrid;

    private Rect worldBoardBound;
    
    [SerializeField] private GameObject gemPrefab;

    float distanceBetweenGems = 1f;

    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
    }

    private void OnDestroy()
    {
        SwipeDetector.OnSwipe -= SwipeDetector_OnSwipe;
    }

    void Start()
    {
        board = GetComponent<Board>();

        gemGOGrid = new GameObject[board.width, board.height];

        ResetGemGOGrid();
    }

    void ResetGemGOGrid()
    {
        var gemGrid = board.GetGGridCopy();

        for (int i = 0; i < gemGrid.GetLength(0); i++)
        {
            for (int j = 0; j < gemGrid.GetLength(1); j++)
            {
                if (gemGOGrid[i, j] != null)
                {
                    Destroy(gemGOGrid[i, j]);
                }
                GameObject gem = Instantiate(gemPrefab, new Vector2(i, j), Quaternion.identity, transform);
                gem.name = $"({i}, {j})";
                gem.GetComponent<Gem>().gemType = gemGrid[i, j];
                gem.GetComponent<SpriteRenderer>().sprite = GemManager.Instance.GetGemOfType(gemGrid[i, j]).Sprite;
                gem.GetComponent<Gem>().originalPosition = gem.transform.position;
                gem.GetComponent<Gem>().gridPos = new Vector2Int(i, j);
                gemGOGrid[i, j] = gem;

            }
        }

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

    GameObject[,] UpdateGemGOGrid()
    {
        GameObject[,] newGrid = new GameObject[gemGOGrid.GetLength(0), gemGOGrid.GetLength(1)];
        for (int i = 0; i < newGrid.GetLength(0); i++)
        {
            for (int j = 0; j < newGrid.GetLength(1); j++)
            {
                newGrid[i, j] = null;
            }
        }

        foreach (var gem in gemGOGrid)
        {
            if (gem != null)
            {
                Gem gemScript = gem.GetComponent<Gem>();
                Vector2Int gridPos = gemScript.gridPos;
                newGrid[gridPos.x, gridPos.y] = gem;
            }
        }
        return newGrid;
    }

    GameObject draggingGem = null;
    GameObject[] draggingRowOrCol;
    SwipeDirection lastSwipeDirection;
    void SwipeDetector_OnSwipe(SwipeData data)
    {
        if (data.Phase == SwipePhase.Swipping)
        {
            // print($"Swipping");
            if (draggingGem == null)
            {
                // Define gem being dragged
                for (var i = 0; i < gemGOGrid.GetLength(0); i++)
                {
                    for (var j = 0; j < gemGOGrid.GetLength(1); j++)
                    {
                        var gem = gemGOGrid[i, j];
                        var gemCollider = gem.GetComponent<BoxCollider2D>();

                        Vector3 worldStartTouchPos = Camera.main.ScreenToWorldPoint(data.StartPosition);
                        worldStartTouchPos.z = gem.transform.position.z;

                        if (gemCollider.bounds.Contains(worldStartTouchPos))
                        {
                            draggingGem = gem;
                        }
                    }

                }

                // Now we know the gem and the drag direction.
                // draggingRowOrCol = new GameObject[gemGOGrid.GetLength(0)];
                if (data.Direction == SwipeDirection.Down || data.Direction == SwipeDirection.Up)
                {
                    // Col
                    draggingRowOrCol = GetGemColumn(draggingGem);
                }
                else
                {
                    // Row
                    draggingRowOrCol = GetGemRow(draggingGem);
                }
            }
            else
            {
                var worldDeltaDrag = Camera.main.ScreenToWorldPoint(data.EndPosition) - Camera.main.ScreenToWorldPoint(data.StartPosition);
                worldDeltaDrag.z = draggingGem.transform.position.z;

                // Move col or row
                if (data.Direction == SwipeDirection.Down || data.Direction == SwipeDirection.Up)
                {
                    if (lastSwipeDirection != SwipeDirection.Down || lastSwipeDirection != SwipeDirection.Up)
                    {
                        foreach (var gem in draggingRowOrCol)
                        {
                            gem.transform.position = gem.GetComponent<Gem>().originalPosition;
                        }
                        draggingRowOrCol = GetGemColumn(draggingGem);
                    }

                    foreach (var gem in draggingRowOrCol)
                    {
                        var originalPos = gem.GetComponent<Gem>().originalPosition;
                        var relPos = ((originalPos.y - worldBoardBound.y + worldDeltaDrag.y) % worldBoardBound.height);
                        while (relPos < 0)
                        {
                            relPos += worldBoardBound.height;
                        }
                        float newYPos = worldBoardBound.y + relPos;

                        // Research modulus range to make it wrap for positive and negative numbers
                        // without the use of condition
                        gem.transform.position = new Vector3(originalPos.x, newYPos, originalPos.z);
                    }
                }
                else
                {
                    if (lastSwipeDirection != SwipeDirection.Right || lastSwipeDirection != SwipeDirection.Left)
                    {
                        foreach (var gem in draggingRowOrCol)
                        {
                            gem.transform.position = gem.GetComponent<Gem>().originalPosition;
                        }
                        draggingRowOrCol = GetGemRow(draggingGem);
                    }

                    foreach (var gem in draggingRowOrCol)
                    {
                        var originalPos = gem.GetComponent<Gem>().originalPosition;
                        var relPos = ((originalPos.x - worldBoardBound.x + worldDeltaDrag.x) % worldBoardBound.width);
                        while (relPos < 0)
                        {
                            relPos += worldBoardBound.width;
                        }
                        float newXPos = worldBoardBound.x + relPos;
                        gem.transform.position = new Vector3(newXPos, originalPos.y, originalPos.z);
                    }
                }
            }

        }
        else // SwipePhase == Ended
        {
            // TODO: Leave the rest to the Update method, so we can use tweens, game states and coroutines
            // so the player knows what's going on (e.g multiple matches from the generated gems)

            // print($"Swipping Ended");
            // Make move
            // draggingGem may already be null if dragged in a wrong start position

            // Snap to correct position
            // Create a Gem.GemType[,] grid from this position
            // Use the Board.CheckMatches function on this grid
            // if there are NO matches
            // * Do nothing and return gems to original position (tween?)
            // if there are matches
            // * trigger a loop that runs while there are no more matches
            // * * update gem original positions to current one, and gridPos of each gem
            // * * update "official" GemType grid (Board.gGrid)
            // * * "delete" matching gems and add to a score
            // * * Make remaining gems fall to the bottom (tween?) and update their data and the grid's
            // * * Generate new gems where there are missing ones, update their data and the grid's
            // * * Create Gem.GemType[,] grid from current grid
            // * * Use Board.CheckMatches on the new grid

            // snap
            foreach(var gem in draggingRowOrCol)
            {
                gem.transform.position = GetGridSnappedPos(gem.transform.position, distanceBetweenGems, worldBoardBound);
            }

            // Create GemType[,]
            var currentGemTypeArray = GetGemTypeArray(gemGOGrid);

            // Check if there are matches on this position
            var matches = board.CheckMatches(currentGemTypeArray);

            // while (matches.Count > 0)
            if (matches.Count > 0)
            {
                print($"Found {matches.Count} match(es)");
                // Update gems GOs positions
                foreach (GameObject gem in gemGOGrid)
                {
                    // Error setting some originalPositions and gridPos'
                    Gem gemScript = gem.GetComponent<Gem>();
                    gemScript.originalPosition = GetGridSnappedPos(gem.transform.position, distanceBetweenGems, worldBoardBound);
                    gemScript.gridPos = GetGridPosFromWorld(gem.transform.position, distanceBetweenGems, worldBoardBound);
                }
                gemGOGrid = UpdateGemGOGrid();

                // Update "official" GemType grid
                board.UpdateGemTypeGrid(currentGemTypeArray);

                // Delete matching gems and add to a score
                foreach (var match in matches)
                {
                    foreach (var gemGridPos in match)
                    {
                        Destroy(gemGOGrid[gemGridPos.x, gemGridPos.y]);
                        gemGOGrid[gemGridPos.x, gemGridPos.y] = null;
                    }
                }

                // Move remaining gems down and update all data
                for (int i = 0; i < gemGOGrid.GetLength(0); i++)
                {
                    List<GameObject> colGems = new List<GameObject>();
                    List<Vector2Int> emptySpots = new List<Vector2Int>();
                    for (int j = 0; j < gemGOGrid.GetLength(1); j++)
                    {
                        if (gemGOGrid[i, j] != null)
                        {
                            colGems.Add(gemGOGrid[i, j]);
                        }
                        else
                        {
                            emptySpots.Add(new Vector2Int(i, j));
                        }
                    }

                    for (int j = 0; j < colGems.Count; j++)
                    {
                        Vector3 newWorldPos = GetWorldPosFromGrid(new Vector2Int(i, j), distanceBetweenGems, worldBoardBound);
                        GameObject gem = colGems[j];
                        gem.GetComponent<Gem>().originalPosition = GetGridSnappedPos(newWorldPos, distanceBetweenGems, worldBoardBound);
                        gem.GetComponent<Gem>().gridPos = new Vector2Int(i, j);
                        gem.transform.position = gem.GetComponent<Gem>().originalPosition;
                    }


                    // Generate new Gems for the available spots (on top)
                    for (int j = colGems.Count; j < gemGOGrid.GetLength(1); j++)
                    {
                        SOGem newGem = GemManager.Instance.GetRandomGem();
                        GameObject gem = Instantiate(
                            gemPrefab,
                            GetWorldPosFromGrid(new Vector2Int(i, j), distanceBetweenGems, worldBoardBound),
                            Quaternion.identity,
                            transform);
                        gem.GetComponent<Gem>().gemType = newGem.GemType;
                        gem.GetComponent<SpriteRenderer>().sprite = newGem.Sprite;
                        gem.GetComponent<Gem>().originalPosition = gem.transform.position;
                        gem.GetComponent<Gem>().gridPos = new Vector2Int(i, j);
                        // hack. UpdateGemGoGrid will set to the correct grid position
                        Vector2Int emptySpot = emptySpots[0];
                        emptySpots.Remove(emptySpot);
                        gemGOGrid[emptySpot.x, emptySpot.y] = gem;
                        gem.name = $"({gem.GetComponent<Gem>().gemType})";
                    }
                }

                // Update data
                gemGOGrid = UpdateGemGOGrid();
                currentGemTypeArray = GetGemTypeArray(gemGOGrid);
                board.UpdateGemTypeGrid(currentGemTypeArray);

                // Last thing to check in loop
                matches = board.CheckMatches(currentGemTypeArray);
                if (matches.Count > 0)
                {
                    print($"Still more matches");
                }
            }

            // Reset Gems to designated positions
            ResetGemsToOriginalPos();

            draggingGem = null;
            draggingRowOrCol = null;
        }

        lastSwipeDirection = data.Direction;
    }

    GameObject[] GetGemColumn(GameObject gem)
    {
        GameObject[] goCol = new GameObject[gemGOGrid.GetLength(1)];
        for (int j = 0; j < gemGOGrid.GetLength(1); j++)
        {
            goCol[j] = gemGOGrid[gem.GetComponent<Gem>().gridPos.x, j];
        }
        return goCol;
    }

    GameObject[] GetGemRow(GameObject gem)
    {
        GameObject[] goRow = new GameObject[gemGOGrid.GetLength(0)];
        for (int i = 0; i < gemGOGrid.GetLength(0); i++)
        {
            goRow[i] = gemGOGrid[i, gem.GetComponent<Gem>().gridPos.y];
        }
        return goRow;
    }

    Vector3 GetGridSnappedPos(Vector3 gemPos, float distBetween, Rect bounds)
    {
        float extent = distBetween / 2f;

        float gridX = bounds.x +
            distBetween * (float)Math.Floor((gemPos.x - bounds.x) / distBetween) +
            extent;
        float gridY = bounds.y +
            distBetween * (float)Math.Floor((gemPos.y - bounds.y) / distBetween) +
            extent;

        return new Vector3(gridX, gridY, gemPos.z);
    }

    Vector2Int GetGridPosFromWorld(Vector3 gemPos, float distBetween, Rect bounds)
    {
        Vector3 gsp = GetGridSnappedPos(gemPos, distBetween, bounds);
        return new Vector2Int((int)gsp.x, (int)gsp.y);
    }

    Vector3 GetWorldPosFromGrid(Vector2Int gridPos, float distBetween, Rect bounds)
    {
        float x = bounds.x + gridPos.x * distanceBetweenGems + distanceBetweenGems / 2f;
        float y = bounds.y + gridPos.y * distanceBetweenGems + distanceBetweenGems / 2f;
        return new Vector3(x, y);
    }

    void ResetGemsToOriginalPos()
    {
        foreach (var gem in gemGOGrid)
        {
            gem.transform.position = gem.GetComponent<Gem>().originalPosition;
        }
    }

    Gem.GemType[,] GetGemTypeArray(GameObject[,] goGrid)
    {
        int w = goGrid.GetLength(0);
        int h = goGrid.GetLength(1);

        Gem.GemType[,] gtGrid = new Gem.GemType[w, h];

        foreach (GameObject gem in goGrid)
        {
            Vector2Int gridPos = GetGridPosFromWorld(gem.transform.position, distanceBetweenGems, worldBoardBound);
            gtGrid[gridPos.x, gridPos.y] = gem.GetComponent<Gem>().gemType;
        }

        return gtGrid;
    }
}
