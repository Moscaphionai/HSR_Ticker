using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForDraw : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(!Application.isPlaying)
            Gizmos.DrawSphere(transform.position,0.01f);
    }
}
