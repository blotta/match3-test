using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteMatches : IState
{
    private GameObject[,] _gemGOGrid;
    private List<List<Vector2Int>> _matches;
    private List<GameObject> _gemsToDelete;
    private Action<List<List<Vector2Int>>> _doneCallback;
    public delegate IEnumerator GemEffectCoroutine(GameObject gem, Action callback);
    private IEnumerator _vfxCoroutine;

    public DeleteMatches(ref GameObject[,] gemGOGrid, List<List<Vector2Int>> matches, Action<List<List<Vector2Int>>> doneCallback)
    {
        _gemGOGrid = gemGOGrid;
        _matches = matches;
        _doneCallback = doneCallback;

        _gemsToDelete = new List<GameObject>();
        foreach (var group in matches)
        {
            foreach (var gemGridPos in group)
            {
                _gemsToDelete.Add(_gemGOGrid[gemGridPos.x, gemGridPos.y]);
            }
        }
    }

    IEnumerator FadeGem(GameObject gem, Action callback)
    {
        var sprite = gem.GetComponent<SpriteRenderer>();
        while (sprite.color.a > 0)
        {
            Color newColor = sprite.color;

            newColor.a -= 10f * Time.deltaTime;
            if (newColor.a <= 0.1f)
                newColor.a = 0f;

            sprite.color = newColor;

            yield return null;
        }

        SoundManager.Instance.PlaySound("select");
        BoardManager.Instance.SpawnDestroyParticleEffect(gem.transform.position);

        callback();
    }

    IEnumerator SerialEffectGems(List<GameObject> gems, GemEffectCoroutine e)
    {
        IEnumerator effectC;
        List<GameObject> fadedGems = new List<GameObject>();
        foreach (var gem in gems)
        {
            effectC = e(gem, () => { effectC = null; });
            BoardManager.Instance.StartCoroutine(effectC);

            while (effectC != null)
            {
                yield return null;
            }

            yield return null;
        }

        _vfxCoroutine = null;
    }

    public void Enter()
    {
        Vector3 newScale = new Vector3(1.2f, 1.2f, 1);
        foreach (var gem in _gemsToDelete)
            gem.transform.localScale = newScale;

        _vfxCoroutine = SerialEffectGems(_gemsToDelete, FadeGem);
        // Any MonoBehaviour instance
        BoardManager.Instance.StartCoroutine(_vfxCoroutine);
    }

    public void Execute()
    {
        if (_vfxCoroutine == null)
        {
            // Destroy Gems
            for (int i = 0; i < _gemsToDelete.Count; i++)
            {
                var gridPos = _gemsToDelete[i].GetComponent<Gem>().gridPos;
                _gemGOGrid[gridPos.x, gridPos.y] = null;

                MonoBehaviour.Destroy(_gemsToDelete[i]);
            }

            // Scoring
            float addToScore = 0f;
            foreach (var gemGroup in _matches)
            {
                // Every group is 1 point + 0.5 per extra gem in group
                addToScore += 1 + (gemGroup.Count - 3) * 0.5f;
            }

            // multiply addToScore by 1.1 for 2 matches, 1.2 for 3 matches, etc
            addToScore *= (1 + (_matches.Count - 1) / 10f);

            // Apply score
            // BoardManager.Instance.score += addToScore;
            BoardManager.Instance.SetScore(BoardManager.Instance.StageData.score + addToScore);

            _doneCallback(_matches);
        }
    }

    public void Exit()
    {
    }
}
