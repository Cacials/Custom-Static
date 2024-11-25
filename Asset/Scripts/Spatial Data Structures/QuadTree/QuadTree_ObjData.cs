using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree_ObjData 
{
    public string uid;//id标识
    public GameObject prefab;//预制体
    public Vector3 pos;//坐标点
    public Vector3 ang;//欧拉角度

    public QuadTree_ObjData(GameObject prefab, Vector3 pos,Vector3 ang)
    {
        this.uid = System.Guid.NewGuid().ToString();
        this.prefab = prefab;
        this.pos = pos;
        this.ang = ang;
    }
}
