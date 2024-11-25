using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    public Bounds bound
    {
        get;
        set;
    }

    private QuadTreeNode root;
    public int maxDepth { get; }
    public int maxChildCount { get; }

    public QuadTree(Bounds bound)
    {
        this.bound = bound;
        this.maxDepth = 5;
        this.maxChildCount = 4;
        root = new QuadTreeNode(bound, 0, this);
    }
    
    public void InsertObj(QuadTree_ObjData obj)
    {
        root.InsertObj(obj);
    }

    public void TriggerMove(Plane[] planes)
    {
        root.TriggerMove(planes);
    }

    public void DrawBound()
    {
        root.DrawBound();
    }
}
