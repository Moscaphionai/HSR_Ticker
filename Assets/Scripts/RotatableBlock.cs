using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableBlock : MonoBehaviour
{
    public Block[] blocks = new Block[7];
    public RotatableBlock InMirror;
    public Block Pivot;
    public Vector3 Axis = Vector3.left;
    [Header("参数")] [Range(0, 1)] public float highLightMix = 0.2f;
    public float rotateTime = 0.05f;
    public bool isRotating = false;

    private int _rotateAngle = 90;
    private readonly int _highlightMixPropId = Shader.PropertyToID("_HighLightMix");

    private void Start()
    {
        foreach (var block in blocks)
        {
            block.MouseDownEvent += _ => StartCoroutine(TryRotate());
            block.MouseEnterEvent += _ => SetHighlightMix(highLightMix);
            block.MouseExitEvent += _ => SetHighlightMix(0);
        }
    }

    private void SetHighlightMix(float value)
    {
        foreach (var block in blocks)
        {
            block.meshRenderer.material.SetFloat(_highlightMixPropId, value);
        }
    }

    private IEnumerator TryRotate()
    {
        if (!BlockManager.Instance.CanInteract)
        {
            yield break;
        }

        if (!CanRotate())
        {
            // 换一个方向尝试
            _rotateAngle = -_rotateAngle;

            if (!CanRotate())
            {
                BlockManager.Instance.DisableInteract();
                yield break;
            }
        }

        BlockManager.Instance.DisableInteract();
        yield return StartCoroutine(DoRotateAnimation(_rotateAngle));
        BlockManager.Instance.EnableInteract();
        BlockManager.Instance.RebuildBlockMap();
    }

    private IEnumerator DoRotateAnimation(int angle)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> positionsInMirror = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();
        List<Quaternion> rotationsInMirror = new List<Quaternion>();

        // 记录初始位置和旋转
        for (int i = 0; i < blocks.Length; i++)
        {
            positions.Add(blocks[i].transform.position);
            rotations.Add(blocks[i].transform.localRotation);
        }

        for (int i = 0; i < InMirror.blocks.Length; i++)
        {
            positionsInMirror.Add(InMirror.blocks[i].transform.position);
            rotationsInMirror.Add(InMirror.blocks[i].transform.localRotation);
        }

        float time = 0;
        while (time < rotateTime)
        {
            float progress = time / rotateTime;

            // 旋转镜子外的方块
            for (int i = 0; i < blocks.Length; i++)
            {
                Transform blockTransform = blocks[i].transform;
                blockTransform.position = GetPositionAfterRotation(positions[i], angle, progress,
                    Pivot.transform.position, out Quaternion rot);
                blockTransform.localRotation = rot * rotations[i];
            }

            // 旋转镜子内的方块
            for (int i = 0; i < InMirror.blocks.Length; i++)
            {
                Transform blockTransform = InMirror.blocks[i].transform;
                // 使用负角度使旋转方向相反
                blockTransform.position = GetPositionAfterRotation(positionsInMirror[i], -angle, progress,
                    InMirror.Pivot.transform.position, out Quaternion rot);
                // 由于localScale.z为-1，需要调整局部旋转方向
                blockTransform.localRotation = rotationsInMirror[i] * Quaternion.Inverse(rot);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // 最后重新设置一次，减少误差
        for (int i = 0; i < blocks.Length; i++)
        {
            Transform blockTransform = blocks[i].transform;
            blockTransform.position = GetPositionAfterRotation(positions[i], angle, 1, Pivot.transform.position, out Quaternion rot);
            blockTransform.localRotation = rot * rotations[i];
        }

        for (int i = 0; i < InMirror.blocks.Length; i++)
        {
            Transform blockTransform = InMirror.blocks[i].transform;
            blockTransform.position = GetPositionAfterRotation(positionsInMirror[i], -angle, 1, InMirror.Pivot.transform.position, out Quaternion rot);
            blockTransform.localRotation = rotationsInMirror[i] * Quaternion.Inverse(rot);
        }
    }

    private Vector3 GetPositionAfterRotation(Vector3 position, int angle, float slerpT, Vector3 pivot,
        out Quaternion rot)
    {
        rot = Quaternion.AngleAxis(angle, Axis);

        if (slerpT < 1)
        {
            rot = Quaternion.Slerp(Quaternion.identity, rot, slerpT);
        }

        return pivot + rot * (position - pivot);
    }

    private bool CanRotate()
    {
        HashSet<Vector3Int> cubes = new HashSet<Vector3Int>();
        cubes.UnionWith(BlockManager.Instance.allBlockInWorld);

        // 移除自己下面所有方块
        for (int i = 0; i < blocks.Length; i++)
        {
            cubes.Remove(blocks[i].transform.position.RoundToVector3Int());
        }

        // 检查旋转后周围是否有方块。有的话就没法旋转
        Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        for (int i = 0; i < blocks.Length; i++)
        {
            Vector3Int pos =
                GetPositionAfterRotation(blocks[i].transform.position, _rotateAngle, 1, Pivot.transform.position, out _)
                    .RoundToVector3Int();
            min = Vector3Int.Min(pos, min);
            max = Vector3Int.Max(pos, max);
        }

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                for (int z = min.z; z <= max.z; z++)
                {
                    if (cubes.Contains(new Vector3Int(x, y, z)))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}