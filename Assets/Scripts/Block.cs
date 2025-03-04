using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Block : MonoBehaviour
{
    public float moveHorizonMax;
    public float moveHorizonMin;
    public float moveVertMax;
    public float moveVertMin;
    public float moveYMax;
    public float moveYMin;

    public Vector2Int osXY;
    public float vsDepth;
    public BlockRemainPatch remainPatch;
    
    [Header("寻路")]
    public List<Block> neighbours = new();
    public Block comeTo;

    private MeshRenderer _meshRenderer;

    public MeshRenderer meshRenderer => _meshRenderer ??= GetComponent<MeshRenderer>();

    #region 事件

    [Header("事件")] [SerializeField] private UnityEvent<Block> onMouseEnter = new();
    [SerializeField] private UnityEvent<Block> onMouseExit = new();
    [SerializeField] private UnityEvent<Block> onMouseDown = new();
    [SerializeField] private UnityEvent<Block> onMouseUp = new();
    [SerializeField] private UnityEvent<Block> onMouseDrag = new();

    public event UnityAction<Block> MouseEnterEvent
    {
        add => onMouseEnter.AddListener(value);
        remove => onMouseEnter.RemoveListener(value);
    }

    public event UnityAction<Block> MouseExitEvent
    {
        add => onMouseExit.AddListener(value);
        remove => onMouseExit.RemoveListener(value);
    }

    public event UnityAction<Block> MouseDownEvent
    {
        add => onMouseDown.AddListener(value);
        remove => onMouseDown.RemoveListener(value);
    }

    public event UnityAction<Block> MouseUpEvent
    {
        add => onMouseUp.AddListener(value);
        remove => onMouseUp.RemoveListener(value);
    }

    public event UnityAction<Block> MouseDragEvent
    {
        add => onMouseDrag.AddListener(value);
        remove => onMouseDrag.RemoveListener(value);
    }

    private void OnMouseEnter() => onMouseEnter.Invoke(this);
    private void OnMouseExit() => onMouseExit.Invoke(this);
    private void OnMouseDown() => onMouseDown.Invoke(this);
    private void OnMouseDrag() => onMouseDrag.Invoke(this);
    private void OnMouseUp() => onMouseUp.Invoke(this);

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        if ((remainPatch & BlockRemainPatch.UpLeft) != 0)
        {
            Gizmos.color=Color.magenta;
            Gizmos.DrawWireCube(new Vector3(osXY.x, osXY.y,1), new Vector3(1,1,1));
        }
        else if ((remainPatch & BlockRemainPatch.LeftUp) != 0)
        {
            Gizmos.color=Color.red;
            Gizmos.DrawWireCube(new Vector3(osXY.x, osXY.y,1), new Vector3(1,1,1));
        }else if ((remainPatch & BlockRemainPatch.RightUp) != 0)
        {
            Gizmos.color=Color.green;
            Gizmos.DrawWireCube(new Vector3(osXY.x, osXY.y,1), new Vector3(1,1,1));
        }
        Gizmos.color=Color.cyan;
        foreach (var v in neighbours)
        {
            Gizmos.DrawLine(this.transform.position, v.transform.position);
        }
    }
}