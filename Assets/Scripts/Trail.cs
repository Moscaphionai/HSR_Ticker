using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    public TrailRenderer renderer;
    public float timePerBlock = 0.1f;
    [SerializeField] private float constY = 6f;

    private void Start()
    {
        renderer.emitting = true;
    }

    public IEnumerator Move(Block cur, Block to)
    {
        transform.position = GetConstraintPos(cur.transform.position);
        renderer.Clear();
        yield return null;

        renderer.emitting = true;
        /*
         * wp * worldToView=vp
         * 设约束后的wp2=(a,const b,c) wp2 * worldToView = (a',b',c')
         * 其中 a' =vp.x c' =vp.x
         *
         */
        while (cur != to)
        {
            float time = 0;
            Vector3 a = GetConstraintPos(cur.transform.position);
            Vector3 b = GetConstraintPos(cur.comeTo.transform.position);
            while (time < timePerBlock)
            {
                float p = time / timePerBlock;
                transform.position =Vector3.Lerp(a, b, p);
                time += Time.deltaTime;
                
                yield return null;
            }

            cur = cur.comeTo;
        }

        yield return new WaitForSeconds(renderer.time);
        renderer.emitting = false;
    }

    private Vector3 GetConstraintPos(Vector3 wp)
    {
        Matrix4x4 m = Camera.main.worldToCameraMatrix;
        Vector3 vp = m.MultiplyPoint(wp+new Vector3(0,1,0));

        // 计算2x2矩阵的行列式
        float a = m[0, 0], b = m[0, 2];
        float c = m[1, 0], d = m[1, 2];
        float det = a * d - b * c;

        if (Mathf.Abs(det) < 1e-6f)
            return new Vector3(0, constY, 0); // 防止除零

        // 计算补偿项
        float Cx = m[0, 1] * constY + m[0, 3];
        float Cy = m[1, 1] * constY + m[1, 3];

        // 解线性方程组
        float targetX = vp.x - Cx;
        float targetY = vp.y - Cy;
        
        float x = (d * targetX - b * targetY) / det;
        float z = (-c * targetX + a * targetY) / det;

        // 转换回世界坐标
        Vector3 worldPos = new Vector3(x, constY, z);
        return worldPos;
    }
}