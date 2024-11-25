using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[System.Flags]
public enum VertexParam
{
    Position=1<< 0,
    Normal=1 << 1,
    Tangent=1 << 2,
    Color=1 << 3,
    UV0=1 << 4,
    UV1=1 << 5,
}
public class VertexDataCompression : EditorWindow
{

    private Mesh srcMesh;

    private VertexParam vertParam;
    // [LabelText("查找路径"), ShowInInspector, FolderPath(ParentFolder = "Assets/")]
    // public string str;
    
    [MenuItem("Tools/Model/顶点数据压缩")]
    public static void ShowWindow()
    {
        GetWindow(typeof(VertexDataCompression));//显示现有窗口实例。如果没有，请创建一个。
    }
    private void OnGUI()
    {
        
        GUILayout.Space(10);
        
        srcMesh=(Mesh)EditorGUILayout.ObjectField("需要压缩数据的Mesh",srcMesh, typeof(Mesh),false);
        
        GUILayout.Space(10);
        vertParam = (VertexParam)EditorGUILayout.EnumFlagsField("使用数据",vertParam);
        // vertParam = (VertexParam)EditorGUILayout.EnumMaskField("使用数据",vertParam);
        // vertParam=(VertexParam)EditorGUILayout.EnumPopup("写入目标",vertParam);
        GUILayout.Space(10);

        if (GUILayout.Button("Compress"))
        {
            Debug.LogError(vertParam);

            Compression();
        }
        
    }

    private void Compression()
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

        for (int i = 1; i < 65; i *=2)
        {
            int indx = (int)Mathf.Log(i,2);
            
            
            if (IsContained(vertParam, i))
            {
                paramLst.Add(allParam[indx]);
            }
        }

        VertexAttributeDescriptor[] usedParam = new VertexAttributeDescriptor[paramLst.Count];

        for (int i = 0; i < paramLst.Count; i++)
        {
            usedParam[i] = paramLst[i];
        }
        
        // srcMesh.SetVertexBufferParams(vertices.Length,
        //     new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float16,4),
        //     new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm8,4),
        //     new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.SNorm8, 4),
        //     new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2),
        //     new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float16, 2));

        srcMesh.SetVertexBufferParams(srcMesh.vertexCount,usedParam);
    }

    private bool IsContained(VertexParam param,int i)
    {
        if ((param & (VertexParam)i) != 0)
        {
            return true;
        }
        
        return false;

    }
    
    
    // // 保存序列化数据，否则会出现设置数据丢失情况
    // serializedObject.ApplyModifiedProperties ();
}
