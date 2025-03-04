using System;
using System.Collections.Generic;
using UnityEngine;

public class MovableBlock : MonoBehaviour
{
    public enum MoveDir
    {
        Horizontal = 1 << 0,
        Vertical = 1 << 1,
        Y = 1 << 2
    }

    public Block[] blocks = new Block[4];
    public MoveDir moveDir = 0;
    public float horizonMax;
    public float horizonMin;
    public float vertMax;
    public float vertMin;
    [Header("参数")] [Range(0, 1)] public float highLightMix = 0.2f;
    public float speed = 0.05f;
    public MovableBlock inMirror;


    private Vector2? _mousePos;
    private HashSet<Vector3Int> _worldBlocks = new HashSet<Vector3Int>();

    private void Start()
    {
        if (inMirror == null)
        {
            return;
        }

        foreach (var v in blocks)
        {
            v.MouseEnterEvent += _ => OnMouseEnterHandler();
            v.MouseExitEvent += _ => OnMouseExitHandler();
            v.MouseDownEvent += _ => OnMouseDownHandler();
            v.MouseDragEvent += _ => OnMouseDragHandler();
            v.MouseUpEvent += _ => OnMouseUpHandler();
        }
    }

    private void OnMouseEnterHandler()
    {
        foreach (var v in blocks)
        {
            v.meshRenderer.material.SetFloat("_HighLightMix", highLightMix);
        }
    }

    private void OnMouseExitHandler()
    {
        foreach (var v in blocks)
        {
            v.meshRenderer.material.SetFloat("_HighLightMix", 0);
        }
    }


    private void OnMouseDownHandler()
    {
        if (!BlockManager.Instance.CanInteract)
        {
            return;
        }

        _mousePos = Input.mousePosition;
        _worldBlocks.Clear();
        _worldBlocks.UnionWith(BlockManager.Instance.allBlockInWorld);

        foreach (var block in blocks)
        {
            _worldBlocks.Remove(block.transform.position.RoundToVector3Int());
        }

        BlockManager.Instance.DisableInteract();
    }

    private void OnMouseDragHandler()
    {
        if (!_mousePos.HasValue)
        {
            return;
        }

        float dir=0f;
        if (moveDir == MoveDir.Horizontal)
        {
            dir = Math.Sign(Input.mousePosition.x - _mousePos.Value.x);
        }
        else if (moveDir == MoveDir.Vertical)
        {
            dir = Math.Sign(Input.mousePosition.y - _mousePos.Value.y);
        }
        else if (moveDir == MoveDir.Y)
        {
            dir = Math.Sign(Input.mousePosition.y - _mousePos.Value.y);
        }
        if (CanMove(dir))
        {
            foreach (var block in blocks)
            {
                Vector3 pos = block.transform.localPosition;
                pos.x = Mathf.Clamp(pos.x + dir * speed, block.moveHorizonMin, block.moveHorizonMax);
                pos.z = Mathf.Clamp(pos.z + dir * speed, block.moveVertMin, block.moveVertMax);
                pos.y = Mathf.Clamp(pos.y + dir * speed, block.moveYMin, block.moveYMax);
                block.transform.localPosition = pos;
            }

            foreach (var block in inMirror.blocks)
            {
                Vector3 pos = block.transform.localPosition;
                pos.x = Mathf.Clamp(pos.x + dir * speed, block.moveHorizonMin, block.moveHorizonMax);
                pos.z = Mathf.Clamp(pos.z + dir * speed, block.moveVertMin, block.moveVertMax);
                pos.y = Mathf.Clamp(pos.y + dir * speed, block.moveYMin, block.moveYMax);
                block.transform.localPosition = pos;
            }
        }

        _mousePos = Input.mousePosition;
    }

    private void OnMouseUpHandler()
    {
        foreach (var block in blocks)
        {
            Vector3 pos = block.transform.localPosition;
            pos.x = Mathf.Clamp(Mathf.RoundToInt(pos.x), block.moveHorizonMin, block.moveHorizonMax);
            pos.z = Mathf.Clamp(Mathf.RoundToInt(pos.z), block.moveVertMin, block.moveVertMax);
            pos.y = Mathf.Clamp(Mathf.RoundToInt(pos.y), block.moveYMin, block.moveYMax);
            block.transform.localPosition = pos;
        }

        foreach (var block in inMirror.blocks)
        {
            Vector3 pos = block.transform.localPosition;
            pos.x = Mathf.Clamp(Mathf.RoundToInt(pos.x), block.moveHorizonMin, block.moveHorizonMax);
            pos.z = Mathf.Clamp(Mathf.RoundToInt(pos.z), block.moveVertMin, block.moveVertMax);
            pos.y = Mathf.Clamp(Mathf.RoundToInt(pos.y), block.moveYMin, block.moveYMax);
            block.transform.localPosition = pos;
        }

        BlockManager.Instance.EnableInteract();
        BlockManager.Instance.RebuildBlockMap();
    }

    private bool CanMove(float dir)
    {
        foreach (var block in blocks)
        {
            Vector3 pos = block.transform.localPosition;

            //dir 是1或-1，clamp下来有偏差，要乘上speed
            pos.x = Mathf.Clamp(pos.x + dir * speed, block.moveHorizonMin, block.moveHorizonMax);
            pos.z = Mathf.Clamp(pos.z + dir * speed, block.moveVertMin, block.moveVertMax);

            if (block.transform.parent != null)
            {
                pos = block.transform.parent.transform.TransformPoint(pos);
            }

            if (_worldBlocks.Contains(pos.FloorToVector3Int()))
            {
                return false;
            }

            if (_worldBlocks.Contains(pos.CeilToVector3Int()))
            {
                return false;
            }
        }

        return true;
    }
}