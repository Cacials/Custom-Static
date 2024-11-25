using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNode : MonoBehaviour
{
    public Bounds bound
    {
        get;
        set;
    }

    private int depth;
    private QuadTree belongTree;

    private QuadTreeNode[] childLst;
    private List<QuadTree_ObjData> objLst;

    public QuadTreeNode(Bounds bound, int depth, QuadTree belongTree)
    {
        this.belongTree = belongTree;
        this.bound = bound;
        this.depth = depth;
        objLst = new List<QuadTree_ObjData>();
    }

    public void InsertObj(QuadTree_ObjData obj)
    {
        QuadTreeNode node = null;
        bool bChild = false;//是否只有一个子节点包含该物体

        if (depth < belongTree.maxDepth && childLst == null)
        {
            //未到最大节点深度数，且没有子节点时创建子节点
            CreateChild();
        }

        if (childLst != null)
        {
            for (int i = 0; i < childLst.Length; i++)
            {
                QuadTreeNode item = childLst[i];
                // if (item == null)
                // {
                //     Debug.Log("123445");
                //     break;
                // }

                if (item.bound.Contains(obj.pos))//判断子节点的bound是否包含obj的position
                {
                    if (node != null)
                    {
                        //node中含有数据则已经有一个子节点包含obj了
                        bChild = false;
                        break;
                    }

                    node = item;
                    bChild = true;
                }
            }
        }

        if (bChild)
        {
            //只有一个子节点可以包含该物体则该物体放入该子节点
            node.InsertObj(obj);
            Debug.Log(node.depth);
        }
        else
        {
            //多个子节点同时包含该物体的时候，放入父节点的objLst
            objLst.Add(obj);
            Debug.LogWarning(depth);
            Debug.LogError(objLst.Count);
        }
    }

    // public void TriggerMove(Plane[] planes)
    // {
    //     for (int i = 0; i < objLst.Count; i++)
    //     {
    //         //创建或显示该节点的物体
    //         // ResourcesManager.Instance.LoadAsync(objLst[i]);
    //         objLst[i].prefab.SetActive(true);
    //     }
    //
    //     // if (depth == 0)
    //     // {
    //     //     ResourcesManager.Instance.RefreshStatus();
    //     // }
    //
    //     if (childLst != null)
    //     {
    //         for (int i = 0; i < childLst.Length; i++)
    //         {
    //             if (GeometryUtility.TestPlanesAABB(planes,childLst[i].bound))
    //             {
    //                 childLst[i].TriggerMove(planes);
    //             }
    //             else
    //             {
    //                 objLst[i].prefab.SetActive(false);
    //             }
    //         }
    //     }
    // }
    
    public void TriggerMove(Plane[] planes)//视锥体剔除
    {
        if (childLst != null)//如果子节点不为空
        {
            for (int i = 0; i < childLst.Length; i++)//便利所有子节点对象
            {
                childLst[i].TriggerMove(planes);//递归下一节点计算
            }
        }

        for (int i = 0; i < objLst.Count; i++)//便利所有集合列表
        {
            objLst[i].prefab.SetActive(GeometryUtility.TestPlanesAABB(planes, bound));//控制对象预制体开关
        }
    }

    public void CreateChild()
    {
        childLst = new QuadTreeNode[belongTree.maxChildCount];
        int index = 0;
        for (int i = -1; i <= 1; i+=2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                Vector3 centeroffset = new Vector3(bound.size.x / 4 * i, 0, bound.size.z / 4 * j);
                Vector3 cSize = new Vector3(bound.size.x / 2, bound.size.y, bound.size.z / 2);
                Bounds cBound = new Bounds(bound.center + centeroffset, cSize);
                childLst[index++] = new QuadTreeNode(cBound, depth + 1, belongTree);
            }
        }
    }
    
    public void DrawBound()
    {
        if (objLst.Count != 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bound.center,bound.size-Vector3.one*0.1f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bound.center,bound.size-Vector3.one*0.1f);
        }

        if (childLst != null)
        {
            for (int i = 0; i < childLst.Length; i++)
            {
                childLst[i].DrawBound();
            }
        }
    }
}
