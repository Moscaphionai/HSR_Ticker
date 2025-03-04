using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isMoving = false;

    public GameObject goalPrefab;
    public Trail trail;
    public List<Block> goals;
    public Block currentBlock;
    public float timePerBlock = 0.1f;

    private int _goalIndex = 0;

    private Queue<Block> _open = new();
    private HashSet<Block> _close = new();
    private GameObject _goal;

    private void Start()
    {
        SetPlayerPos(currentBlock.transform.position);
        TryPlaceGoal();
    }

    private void TryPlaceGoal()
    {
        if (_goalIndex < goals.Count)
        {
            Transform trans = goals[_goalIndex].transform;
            _goal = Instantiate(goalPrefab, trans, false);
        }
    }


    public void TryMove()
    {
        _open.Clear();
        _close.Clear();

        if (_goalIndex >= goals.Count)
        {
            
            return;
        }
        Block to = goals[_goalIndex];
        _open.Enqueue(to);

        bool canMove = false;
        while (_open.TryDequeue(out Block cur))
        {
            _close.Add(cur);
            if (cur == currentBlock)
            {
                canMove = true;
            }

            foreach (var n in cur.neighbours)
            {
                if (!_close.Contains(n))
                {
                    _open.Enqueue(n);
                    n.comeTo = cur;
                }
            }
        }

        if (canMove)
        {
            _goalIndex++;
            StartCoroutine(Move(to));
        }
    }

    private IEnumerator Move(Block goal)
    {
        isMoving = true;
        BlockManager.Instance.DisableInteract();
        
        yield return StartCoroutine(trail.Move(currentBlock, goal));
        
        while (currentBlock != goal)
        {
            float time = 0;
            while (time < timePerBlock)
            {
                float p = time / timePerBlock;
                Vector3 pos = Vector3.Lerp(currentBlock.transform.position, currentBlock.comeTo.transform.position,
                    p);
                SetPlayerPos(pos);
                time += Time.deltaTime;
                yield return null;
            }

            currentBlock = currentBlock.comeTo;
        }

        GameObject.Destroy(_goal);
        TryPlaceGoal();
        isMoving = false;
        BlockManager.Instance.EnableInteract();
        
        TryMove();
    }

    private void SetPlayerPos(Vector3 pos)
    {
        transform.position = pos + new Vector3(0, 1.5f, 0);
    }
}