using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIrrorPlane : MonoBehaviour
{
    [Header("Mirror")] [Min(1)] public float width = 14;
    [Min(1)] public float height = 7;

    [Header("Movement")] public float horizonMax = 5;
    public float horizonMin = 0;
    private void OnValidate()
    {
        this.transform.localScale = new Vector3(width * 0.1f, height * 0.1f,1);
    }
}
