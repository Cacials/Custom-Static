using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class QuadTreeTest : MonoBehaviour
{
    public bool isDebug;

    public bool isInitOver;

    private QuadTree tree;

    public Bounds mainBound;
    // Start is called before the first frame update

    public GameObject testObj;
    public Camera camera;
    private Plane[] planes;
    void Start()
    {

        planes = new Plane[6];//初始化
        Bounds bounds = new Bounds(transform.position, new Vector3(200, 20, 200));//生成包围盒
        tree = new QuadTree(bounds);//初始化行为树

        for (int x = -100; x < 100; x+=10)//随机生成对象放到树中
        {
            for (int z = -100; z < 100; z+=10)
            {
                for (int y = -10; y < 10; y+=10)
                {
                    if (Random.Range(0, 20) < 1)
                    {
                        GameObject c = Instantiate(testObj, transform);
                        c.transform.localScale = new Vector3(Random.Range(5, 25), Random.Range(5, 25), Random.Range(5, 25));
                        c.transform.position = new Vector3(x, y, z);
                        c.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);

                        tree.InsertObj(new QuadTree_ObjData(c, c.transform.position, c.transform.eulerAngles));
                    }
                }

            }
        }
        
        isInitOver = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInitOver)
        {
            GeometryUtility.CalculateFrustumPlanes(camera, planes);
            
            tree.TriggerMove(planes);
        }
    }

    private void OnDrawGizmos()
    {
        if (!isDebug)
        {
            return;
        }

        if (isInitOver)
        {
            tree.DrawBound();
        }
        else
        {
            Gizmos.DrawWireCube(mainBound.center,mainBound.size);
        }

    }
}
