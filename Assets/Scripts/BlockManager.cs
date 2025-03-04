using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance { get; private set; }
    public MirrorMove mirror;

    public GameObject map;
    public GameObject beforeMirror;
    public GameObject inMirror;
    public GameObject behindMirror;

    public UnityEvent OnRebuildMap=new ();

    public HashSet<Vector3Int> allBlockInWorld = new();

    [SerializeField] private bool canInteract = true;

    public void EnableInteract() => canInteract = true;
    public void DisableInteract() => canInteract = false;

    public bool CanInteract => canInteract;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RebuildBlockMap();
    }

    public void RebuildBlockMap()
    {
        allBlockInWorld.Clear();
        ops.Clear();
        forDraw.Clear();

        Block[] beforeBlocks = beforeMirror.GetComponentsInChildren<Block>();
        Block[] inBlocks = inMirror.GetComponentsInChildren<Block>();
        Block[] behindBlocks = behindMirror.GetComponentsInChildren<Block>();

        Matrix4x4 worldToViewMatrix = Camera.main.worldToCameraMatrix;
        //in view space
        Vector2 axisX = worldToViewMatrix.MultiplyVector(Vector3.right);
        Vector2 axisZ = worldToViewMatrix.MultiplyVector(Vector3.forward);
        Vector3 originWS = beforeBlocks[0].transform.position; //原点
        Vector2 originVS = worldToViewMatrix.MultiplyPoint(originWS);
        Debug.Log($"origin: {originWS.RoundToVector3Int()}",beforeBlocks[0]);

        //axisX is (1,0) in OS
        //axisY is (0,1) in OS
        //origin is (0,0) in oblique space
        /*  example:
         *  M是VS到OS的过度矩阵
         *  M * VS的基 = OS的基
         *  M * (1,0) = axisX = a in VS
         *  M * (0,1) = axisY = b in VS
         *  M = [a,b](M * OS = VS)
         *  对于 OS 下 (x,y)
         *  有
         *      M * [x,y]^T = [x1,y1]^T - [Ox,Oy]^T
         *  即
         *      M^-1 * ( [x1,y1]^T - [Ox,Oy]^T ) = [x,y]^T
         */

        // no need to bother NDC

        //M的行列式的值
        float det = axisX.x * axisZ.y - axisX.y * axisZ.x;
        Matrix4x4 MInv = new Matrix4x4(
            new Vector4(axisZ.y, -axisX.y ) / det,
            new Vector4(-axisZ.x, axisX.x ) / det,
            Vector4.zero,
            Vector4.zero);

        Dictionary<Vector2Int, BlockGroup> osMap = new();
        
        ProcessBlocks(beforeBlocks, worldToViewMatrix, MInv, originVS, osMap);
        ProcessBlocks(inBlocks, worldToViewMatrix, MInv, originVS, osMap);
        ProcessBlocks(behindBlocks, worldToViewMatrix, MInv, originVS, osMap);

        //通过Mirror剔除不能看到的block
        Vector2 A = WorldToObliqueInt(mirror.LeftUpPoint);
        Vector2 B = WorldToObliqueInt(mirror.LeftDownPoint);
        Vector2 C = WorldToObliqueInt(mirror.RightUpPoint);
        Vector2 D = WorldToObliqueInt(mirror.RightDownPoint);
        
        forDraw.Add(A);
        forDraw.Add(B);
        forDraw.Add(C);
        forDraw.Add(D);

        /*
         *      A    C
         *
         *
         *
         * B    D
         */
        CullBlockByMirror(A, B, C, D, beforeBlocks, CullMode.Before);
        CullBlockByMirror(A, B, C, D, inBlocks, CullMode.In);
        CullBlockByMirror(A, B, C, D, behindBlocks, CullMode.Behind);
        
        CullBlockByDepth(osMap);
        ConnectBlocks(osMap);
        
        OnRebuildMap.Invoke();
        return;

        Vector2 WorldToObliqueInt(Vector3 w)
        {
            Vector3 vp = worldToViewMatrix.MultiplyPoint(w);
            Vector2 op = MInv * ((Vector2)vp - originVS);
            return new Vector2(Mathf.RoundToInt(op.x), Mathf.RoundToInt(op.y));
        }
    }

    private void ProcessBlocks(Block[] blocks, Matrix4x4 worldToViewMatrix, Matrix4x4 MInv, Vector2 originVS, Dictionary<Vector2Int, BlockGroup> osMap)
    {
        foreach (var block in blocks)
        {
            Vector3 wp = block.transform.position;
            Vector3 vp = worldToViewMatrix.MultiplyPoint(wp);
            Vector2 op = MInv * ((Vector2)vp - originVS);

            int x = Mathf.RoundToInt(op.x);
            int y = Mathf.RoundToInt(op.y);
            Vector2Int key = new(x, y);

            ops.Add(key);
            allBlockInWorld.Add(block.transform.position.RoundToVector3Int());
            
            if (!osMap.TryGetValue(key, out BlockGroup group))
            {
                group = new BlockGroup();
                osMap[key] = group;
            }

            block.osXY = key;
            block.vsDepth = vp.z;
            group.Add(block);
        }
    }

    private void ConnectBlocks(Dictionary<Vector2Int, BlockGroup> osMap)
    {
        foreach (var v in osMap)
        {
            v.Value.ClearNeighbours();
            if (!v.Value.isWalkable)
            {
                continue;
            }

            BlockGroup group;

            if (osMap.TryGetValue(v.Key + Vector2Int.left, out group) && group.isWalkable)
            {
                v.Value.AddNeighbour(group);
            }

            if (osMap.TryGetValue(v.Key + Vector2Int.right, out group) && group.isWalkable)
            {
                v.Value.AddNeighbour(group);
            }

            if (osMap.TryGetValue(v.Key + Vector2Int.up, out group) && group.isWalkable)
            {
                v.Value.AddNeighbour(group);
            }

            if (osMap.TryGetValue(v.Key + Vector2Int.down, out group) && group.isWalkable)
            {
                v.Value.AddNeighbour(group);
            }
        }
    }

    private void CullBlockByDepth(Dictionary<Vector2Int, BlockGroup> osMap)
    {
        Dictionary<Vector2Int, float> zMap = new();
        foreach (var block in osMap.Values.SelectMany(g => g))
        {
            if ((block.remainPatch & BlockRemainPatch.UpLeft) != 0 || (block.remainPatch & BlockRemainPatch.UpRight) != 0)
            {
                SetZMap(block.osXY, block.vsDepth);
            }

            if ((block.remainPatch & BlockRemainPatch.LeftDown) != 0 ||
                (block.remainPatch & BlockRemainPatch.RightDown) != 0)
            {
                SetZMap(block.osXY + new Vector2Int(-1, -1), block.vsDepth);
            }

            if ((block.remainPatch & BlockRemainPatch.LeftUp) != 0)
            {
                SetZMap(block.osXY + new Vector2Int(-1, 0), block.vsDepth);
            }

            if ((block.remainPatch & BlockRemainPatch.RightUp) != 0)
            {
                SetZMap(block.osXY + new Vector2Int(0, -1), block.vsDepth);
            }
        }

        foreach (var block in osMap.Values.SelectMany(g => g))
        {
            if ((block.remainPatch & BlockRemainPatch.LeftUp) != 0 &&
                zMap.TryGetValue(block.osXY, out float a)
                && block.vsDepth < a)
            {
                block.remainPatch &= ~BlockRemainPatch.LeftUp;
            }

            if ((block.remainPatch & BlockRemainPatch.RightUp) != 0 &&
                zMap.TryGetValue(block.osXY, out float b) &&
                block.vsDepth < b)
            {
                block.remainPatch &= ~BlockRemainPatch.RightUp;
            }
        }

        return;

        void SetZMap(Vector2Int pos, float depth)
        {
            if (!zMap.TryGetValue(pos, out float z))
            {
                zMap[pos] = depth;
            }
            else
            {
                zMap[pos] = Mathf.Min(depth, z);
            }
        }
    }


    private void CullBlockByMirror(Vector2 A, Vector2 B, Vector2 C, Vector2 D, Block[] blocks,
        CullMode mode)
    {
        /*
         *      A    C
         *
         *
         *
         * B    D
         */

        if (mode == CullMode.Before)
        {
            //do nothing
            foreach (var block in blocks)
            {
                block.remainPatch = BlockRemainPatch.Full;
            }
        }
        else if (mode == CullMode.In)
        {
            // if out bounding box then cull
            foreach (var block in blocks)
            {
                block.remainPatch = BlockRemainPatch.None;
                if (IsPointInMirror(block.osXY + new Vector2(1 / 3f, 2 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.UpLeft;
                }

                if (IsPointInMirror(block.osXY + new Vector2(2 / 3f, 1 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.UpRight;
                }

                if (IsPointInMirror(block.osXY + new Vector2(-1 / 3f, 1 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.LeftUp;
                }

                if (IsPointInMirror(block.osXY + new Vector2(-2 / 3f, -1 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.LeftDown;
                }

                if (IsPointInMirror(block.osXY + new Vector2(1 / 3f, -1 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.RightUp;
                }

                if (IsPointInMirror(block.osXY + new Vector2(-1 / 3f, -2 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.RightDown;
                }
            }
        }
        else if (mode == CullMode.Behind)
        {
            
            //if in bounding box then cull
            foreach (var block in blocks)
            {
                block.remainPatch = BlockRemainPatch.None;
                if (!IsPointInMirror(block.osXY + new Vector2(1 / 3f, 2 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.UpLeft;
                }

                if (!IsPointInMirror(block.osXY + new Vector2(2 / 3f, 1 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.UpRight;
                }

                if (!IsPointInMirror(block.osXY + new Vector2(-1 / 3f, 1 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.LeftUp;
                }

                if (!IsPointInMirror(block.osXY + new Vector2(-2 / 3f, -1 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.LeftDown;
                }

                if (!IsPointInMirror(block.osXY + new Vector2(1 / 3f, -1 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.RightUp;
                }

                if (!IsPointInMirror(block.osXY + new Vector2(-1 / 3f, -2 / 3f)))
                {
                    block.remainPatch |= BlockRemainPatch.RightDown;
                }
            }
        }

        return;

        bool IsPointInMirror(Vector2 p)
        {
            Vector2 pa = A - p;
            Vector2 pb = B - p;
            Vector2 pc = C - p;
            Vector2 pd = D - p;

            float papb = pa.Cross(pb);
            float pbpc = pb.Cross(pc);
            float pcpa = pc.Cross(pa);

            float pcpd = pc.Cross(pd);
            float pdpb = pd.Cross(pb);
            bool v1 = papb >= 0 && pbpc >= 0 && pcpa >= 0 || papb <= 0 && pbpc <= 0 && pcpa <= 0;
            bool v2 = pbpc >= 0 && pcpd >= 0 && pdpb >= 0 || pbpc <= 0 && pcpd <= 0 && pdpb <= 0;
            return v1 || v2;
        }
    }

    private HashSet<Vector2Int> ops = new();
    private HashSet<Vector2> forDraw = new();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        /*foreach (var op in ops)
        {
            Gizmos.DrawWireCube(new Vector3(op.x, 1, op.y), new Vector3(1, 1, 1));
        }*/

        Gizmos.color = Color.red;
        foreach (var op in forDraw)
        {
            Gizmos.DrawWireCube(new Vector3(op.x, op.y,1), new Vector3(1, 1, 1));
        }
    }
}