using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExt
{
    public static Mesh GetMesh(this Transform aGO)
    {
        Mesh curMesh = null;
        if (aGO)
        {
            MeshFilter curFilter = aGO.GetComponent<MeshFilter>();
            SkinnedMeshRenderer curSkinnned = aGO.GetComponent<SkinnedMeshRenderer>();
            if (curFilter && !curSkinnned)
            {
                curMesh = curFilter.sharedMesh;
            }
            if (!curFilter && curSkinnned)
            {
                curMesh = curSkinnned.sharedMesh;
            }
        }

        return curMesh;
    }
}
