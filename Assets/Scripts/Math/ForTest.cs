using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForTest : MonoBehaviour
{
    public Block block;
    public Vector2 op;

    public GameObject upLeftObj;
    public GameObject upRightObj;
    
    public GameObject leftUpObj;
    public GameObject leftDownObj;
    
    public GameObject rightUpObj;
    public GameObject rightDownObj;
    
    public Vector2 upLeft;
    public Vector2 upRight;
    
    public Vector2 leftUp;
    public Vector2 leftDown;
    
    public Vector2 rightUp;
    public Vector2 rightDown;

    public List<Vector2> list=new ();
    public List<Vector3> goList=new ();

    private void Awake()
    {
        list.Add(upRight);
        list.Add(upLeft);
        list.Add(rightUp);
        list.Add(rightDown);
        list.Add(leftUp);
        list.Add(leftDown);
        
        goList.Add(upRightObj.transform.position);
        goList.Add(upLeftObj.transform.position);
        goList.Add(rightUpObj.transform.position);
        goList.Add(rightDownObj.transform.position);
        goList.Add(leftUpObj.transform.position);
        goList.Add(leftDownObj.transform.position);
        
    }

    private void Start()
    {
        Matrix4x4 worldToViewMatrix = Camera.main.worldToCameraMatrix;
        //in view space
        Vector2 axisX = worldToViewMatrix.MultiplyVector(Vector3.right);
        Vector2 axisZ = worldToViewMatrix.MultiplyVector(Vector3.forward);
        Vector3 originWS = block.transform.position; //原点
        Vector2 originVS = worldToViewMatrix.MultiplyPoint(originWS);
        
        float det = axisX.x * axisZ.y - axisX.y * axisZ.x;
        Matrix4x4 MInv = new Matrix4x4(
            new Vector4(axisZ.y, -axisX.y, 0, 0) / det,
            new Vector4(-axisZ.x, axisX.x, 0, 0) / det,
            Vector4.zero,
            Vector4.zero);
        Vector3 wp = block.transform.position;
        Vector3 vp = worldToViewMatrix.MultiplyPoint(wp);
        op = MInv * ((Vector2)vp - originVS);
        for (int i = 0; i < 6; i++)
        {
            Vector3 w = goList[i];
            Vector3 v = worldToViewMatrix.MultiplyPoint(wp);
            list[i] = MInv * ((Vector2)vp - originVS);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color=Color.green;
        Gizmos.DrawWireCube(transform.position,Vector3.one);
        Gizmos.color = Color.red;
        if (op != null)
        {
            Gizmos.DrawWireCube(op,new Vector3(0.1f,0.1f,0.1f));
        }
        if (upLeft != null)
        {
            Gizmos.DrawWireCube(op,new Vector3(0.1f,0.1f,0.1f));
        }
        if (upRight != null)
        {
            Gizmos.DrawWireCube(op,new Vector3(0.1f,0.1f,0.1f));
        }
        if (leftUp != null)
        {
            Gizmos.DrawWireCube(op,new Vector3(0.1f,0.1f,0.1f));
        }
        if (leftDown != null)
        {
            Gizmos.DrawWireCube(op,new Vector3(0.1f,0.1f,0.1f));
        }
        if (rightUp != null)
        {
            Gizmos.DrawWireCube(op,new Vector3(0.1f,0.1f,0.1f));
        }
        if (rightDown != null)
        {
            Gizmos.DrawWireCube(op,new Vector3(0.1f,0.1f,0.1f));
        }
    }
}
