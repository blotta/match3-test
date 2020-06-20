using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public GameObject gemPrefab;

    private GameObject[,] gemGrid;
    [SerializeField] private GemManager gemManager;

    // Start is called before the first frame update
    void Start()
    {
        gemGrid = new GameObject[width, height];
        PopulateGemGrid();
    }

    void PopulateGemGrid()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject gem = Instantiate(gemPrefab, new Vector2(i, j), Quaternion.identity, transform);
                gem.name = $"({i}, {j})";
                SOGem newGem = GemManager.Instance.GetRandomGem();
                gem.GetComponent<Gem>().gemType = newGem.GemType;
                gem.GetComponent<SpriteRenderer>().sprite = newGem.Sprite;
                gemGrid[i, j] = gem;
            }
        }

        CheckMatches();
    }

    public List<List<Vector2Int>> CheckMatches()
    {
        List<List<Vector2Int>> results = new List<List<Vector2Int>>();
        List<Vector2Int> currMatch = new List<Vector2Int>();
        List<Vector2Int> gemsAlreadyInAMatch = new List<Vector2Int>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (gemsAlreadyInAMatch.Contains(new Vector2Int(i, j)))
                {
                    continue;
                }

                // Add current gem node being evaluated to the current match
                currMatch.Add(new Vector2Int(i, j));

                // Check neighbors recursively
                List<Vector2Int> operationExclude = new List<Vector2Int>();
                operationExclude.Add(new Vector2Int(i, j));
                List<Vector2Int> connectedSameTypeNeighbors = SearchSameGemTypeNeighbors(new Vector2Int(i, j), ref operationExclude);
                currMatch.AddRange(connectedSameTypeNeighbors);

                if (currMatch.Count >= 3)
                {
                    print($"Found Match: {currMatch.Count} . Type: {GetGemTypeAt(i, j)}");
                    gemsAlreadyInAMatch.AddRange(currMatch);
                    results.Add(currMatch);
                }

                // Reset currMatch
                currMatch.Clear();
            }
        }


        return results;
    }

    // Returns a list of the neighbor GameObject references of the gems matching
    // the same type of the given gem (excluding the given gem)
    public List<Vector2Int> SearchSameGemTypeNeighbors(Vector2Int gemPos, ref List<Vector2Int> exclude)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        // up
        if (gemPos.y != height - 1 && GetGemTypeAt(gemPos + Vector2Int.up) == GetGemTypeAt(gemPos))
        {
            ret.Add(gemPos + Vector2Int.up);
        }

        // down
        if (gemPos.y != 0 && GetGemTypeAt(gemPos) == GetGemTypeAt(gemPos + Vector2Int.down))
        {
            ret.Add(gemPos + Vector2Int.down);
        }

        // left
        if (gemPos.x != 0 && GetGemTypeAt(gemPos) == GetGemTypeAt(gemPos + Vector2Int.left))
        {
            ret.Add(gemPos + Vector2Int.left);
        }

        // right
        if (gemPos.x != width - 1 && GetGemTypeAt(gemPos) == GetGemTypeAt(gemPos + Vector2Int.right))
        {
            ret.Add(gemPos + Vector2Int.right);
        }

        // Remove exclude matches
        foreach (Vector2Int excludeGemPos in exclude)
        {
            if (ret.Contains(excludeGemPos))
            {
                ret.Remove(excludeGemPos);
            }
        }

        // Update exclude
        exclude.AddRange(ret);

        Vector2Int[] e;

        // Check if neighbors have neighbors of same gem type
        if (ret.Count > 0)
        {

            List<Vector2Int> retFromNeighbor = new List<Vector2Int>();
            foreach (Vector2Int neighbor in ret)
            {
                retFromNeighbor.AddRange(SearchSameGemTypeNeighbors(neighbor, ref exclude));
            }
            ret.AddRange(retFromNeighbor);
        }

        return ret;
    }

    public Gem.GemType GetGemTypeAt(Vector2Int gemPos)
    {
        return GetGemTypeAt(gemPos.x, gemPos.y);
    }
    public Gem.GemType GetGemTypeAt(int i, int j)
    {
        return gemGrid[i, j].GetComponent<Gem>().gemType;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
