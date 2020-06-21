using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardManager : MonoBehaviour
{
    Board board;

    GameObject[,] gemGOGrid;
    
    [SerializeField] private GameObject gemPrefab;

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
                gemGOGrid[i, j] = gem;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
