using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MirrorMove : MonoBehaviour
{
    [Header("参数")] public MIrrorPlane plane;
    public float HighLightMix = 0.2f;
    public float speed = 0.05f;
    private MeshRenderer _meshRenderer;
    private Vector2? _mousePos;

    private Vector3 _1of4pos => new(plane.width , plane.height , plane.transform.position.z);

    public Vector3 LeftDownPoint => plane.transform.position + new Vector3(-_1of4pos.x, -_1of4pos.y, 0);
    public Vector3 LeftUpPoint => plane.transform.position + new Vector3(-_1of4pos.x, 0, 0);
    public Vector3 RightUpPoint => plane.transform.position + new Vector3(0, 0, 0);
    public Vector3 RightDownPoint => plane.transform.position + new Vector3(0, -_1of4pos.y, 0);

    

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnMouseEnter()
    {
        _meshRenderer.material.SetFloat("_HighLightMix", HighLightMix);
    }

    private void OnMouseExit()
    {
        _meshRenderer.material.SetFloat("_HighLightMix", 0);
    }

    private void OnMouseDown()
    {
        if (!BlockManager.Instance.CanInteract)
        {
            return;
        }

        _mousePos = Input.mousePosition;
        BlockManager.Instance.DisableInteract();
    }

    private void OnMouseDrag()
    {
        if (_mousePos == null)
        {
            return;
        }

        //注意别用Mathf
        float move = speed * Math.Sign(Input.mousePosition.x - _mousePos.Value.x);
        Vector3 pos = plane.transform.localPosition;
        pos.x = Mathf.Clamp(pos.x + move, plane.horizonMin, plane.horizonMax);
        plane.transform.localPosition = pos;

        _mousePos = Input.mousePosition;
    }

    private void OnMouseUp()
    {
        Vector3 pos = plane.transform.localPosition;
        pos.x = Math.Clamp(Mathf.RoundToInt(pos.x-0.5f)+0.5f, plane.horizonMin, plane.horizonMax);
        plane.transform.localPosition = pos;
        BlockManager.Instance.EnableInteract();
        BlockManager.Instance.RebuildBlockMap();
    }
}