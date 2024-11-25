using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class VertexCompress : AssetPostprocessor
{
    
    //todo: 导入的model不包含MeshFilter的情况还未处理，先不使用
    // void OnPostprocessModel(GameObject model)
    // {
    //     Transform[] trans = model.GetComponentsInChildren<Transform>();
    //     foreach (Transform transform in trans)
    //     {
    //         // 修改网格
    //         Mesh mesh = transform.GetMesh();
    //         Compression(mesh);
    //         
    //     }
    // }
    
    private void Compression(Mesh srcMesh)
    {
        VertexAttributeDescriptor[] allParam = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float16,4),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm8,4),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.SNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.SNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float16, 2)
        };
        
        List<VertexAttributeDescriptor> paramLst = new List<VertexAttributeDescriptor>();
        for (int i = 0; i < allParam.Length; i++)
        {
            if (srcMesh.HasVertexAttribute(allParam[i].attribute))
            {
                paramLst.Add(allParam[i]);
            }
        }
        VertexAttributeDescriptor[] usedParam = new VertexAttributeDescriptor[paramLst.Count];
        
        for (int i = 0; i < paramLst.Count; i++)
        {
            usedParam[i] = paramLst[i];
        }
        
        srcMesh.SetVertexBufferParams(srcMesh.vertexCount,usedParam);
        
    }
}
