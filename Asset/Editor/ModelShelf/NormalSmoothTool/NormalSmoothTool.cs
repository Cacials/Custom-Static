using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum DestType
{
    VertexColor=0,
    Tangent=1,//不建议，用到切线数据的地方多了去了
    UV2=2,//第三套uv
}
public class NormalSmoothTool : EditorWindow
{
    public DestType destType;
    private Mesh srcMesh;
    private GameObject srcGO;

    private Vector3[] normals;
    private Vector3[] vertices;
    private Vector4[] tangents;
    private Color[] colors;
    private Matrix4x4 tbnMatrix = Matrix4x4.identity;


    private ComputeShader m_cs;
    struct MeshProperties
    {
        public Vector3 vertices;
        public Vector3 normals;
        public Vector4 tangents;
        public Vector3 smoothNormal;
    };

    private MeshProperties[] m_meshProp;
    ComputeBuffer m_meshPropBuffer;

    public Vector3[] csResultNormal;
    
    [MenuItem("Tools/Model/平滑法线工具")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(NormalSmoothTool));//显示现有窗口实例。如果没有，请创建一个。
    }

    private void OnGUI()
    {
        
        GUILayout.Space(10);
        
        srcMesh=(Mesh)EditorGUILayout.ObjectField("需要平滑的Mesh",srcMesh, typeof(Mesh),false);
        m_cs=(ComputeShader)EditorGUILayout.ObjectField("加速用CS",m_cs, typeof(ComputeShader),false);
        GUILayout.Space(10);
        destType=(DestType)EditorGUILayout.EnumPopup("写入目标",destType);
        GUILayout.Space(10);

        if (GUILayout.Button("Smooth"))
        {
            // SmoothNormalPrev(destType);
            NormalSmooth();
        }

        if (GUILayout.Button("SmoothByComputeShder"))
        {
            NormalSmooth_CS();
        }
        
        if (GUILayout.Button("ResetVC"))
        {
            SetMeshInfo(true);
            ResetMesh();
        }
        
        if (GUILayout.Button("SaveMesh"))
        {
            // selectMesh();
            SaveMesh();
        }
    }

    private void NormalSmooth()
    {
        SetMeshInfo(true);
        Vector3[] result=SmoothData();
        Write2Mesh(srcMesh, result);
        EditorUtility.ClearProgressBar();
    }
    
    /// <summary>
    /// 使用CS平滑预览
    /// </summary>
    private void NormalSmooth_CS()
    {
        SetMeshInfo(true);
        Vector3[] result=SmoothDataByCS();
        Write2Mesh(srcMesh, result);
    }

    /// <summary>
    /// 设置目标数据
    /// </summary>
    /// <param name="isPreview">是否为场景中预览</param>
    private void SetMeshInfo(bool isPreview)
    {
        if (isPreview)
        {
            if(Selection.activeGameObject==null){//检测是否获取到物体
                Debug.LogError("请选择物体");
                return ;
            }
            srcMesh = Selection.activeGameObject.transform.GetMesh();
        }
        
        normals = srcMesh.normals;
        vertices = srcMesh.vertices;
        colors = srcMesh.colors;
        tangents = srcMesh.tangents;
    }

    
    
    /// <summary>
    /// 直接写到目标mesh里临时看看结果
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="averageNormals"></param>
    public void Write2Mesh(Mesh mesh,Vector3[] averageNormals){
        switch(destType){
            case DestType.Tangent://执行写入到 顶点切线（除非确定不使用法线贴图和动画，否则最好别）
                    var tangents = new Vector4[mesh.vertexCount];
                    for (var j = 0; j < mesh.vertexCount; j++)
                    {
                        tangents[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z, 0);
                    }
                    mesh.tangents = tangents;
            break;
            case DestType.VertexColor:// 写入到顶点色
                    Color[] _colors = new Color[mesh.vertexCount];
                    for (var j = 0; j < mesh.vertexCount; j++)
                    {
                        // _colors[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z,colors[j].a);
                        _colors[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z,1);
                    }   
                    mesh.colors = _colors;
            break;
            
            case DestType.UV2://写入第三套UV
                var uv2normal = new Vector2[mesh.vertexCount];
                for (var j = 0; j < mesh.vertexCount; j++)
                {
                    uv2normal[j] = new Vector2(averageNormals[j].x, averageNormals[j].y);
                }

                mesh.uv3 = uv2normal;
                break;
            }
        }
    

    /// <summary>
    /// 保存结果
    /// </summary>
    public void SaveMesh()
    {
        SetMeshInfo(false);
        // Vector3[] result= SmoothData();
        Vector3[] result= SmoothDataByCS();
        ExportMesh(srcMesh,result);
        EditorUtility.ClearProgressBar();
    }
    
     public void Copy(Mesh dest, Mesh src)
    {
        dest.Clear();
        dest.vertices = src.vertices;

        List<Vector4> uvs = new List<Vector4>();

        src.GetUVs(0, uvs); dest.SetUVs(0, uvs);
        src.GetUVs(1, uvs); dest.SetUVs(1, uvs);
        src.GetUVs(2, uvs); dest.SetUVs(2, uvs);
        src.GetUVs(3, uvs); dest.SetUVs(3, uvs);

        dest.normals = src.normals;
        dest.tangents = src.tangents;
        dest.boneWeights = src.boneWeights;
        dest.colors = src.colors;
        dest.colors32 = src.colors32;
        dest.bindposes = src.bindposes;

        dest.subMeshCount = src.subMeshCount;

        for (int i = 0; i < src.subMeshCount; i++)
            dest.SetIndices(src.GetIndices(i), src.GetTopology(i), i);

        dest.name = src.name ;
    }
    public void ExportMesh(Mesh mesh,Vector3[] averageNormals){
        
        //创建文件夹路径
        string DeletePath = Application.dataPath + "/Editor/NormalSmoothTool/Export";
        Debug.Log(DeletePath);
        //判断文件夹路径是否存在
        if (!Directory.Exists(DeletePath))
        {  //创建
            Directory.CreateDirectory(DeletePath);
        }
        //刷新
        AssetDatabase.Refresh();
        
        Mesh mesh2=new Mesh();
        mesh2.name=mesh.name+"_SMNormal";
        string meshPath = "Assets/Editor/ModelShelf/NormalSmoothTool/Export/" + mesh2.name + ".asset";
        bool exists = File.Exists(DeletePath+"/"+mesh2.name+ ".asset");
        // Debug.LogError(exists);
        // Debug.LogError(mesh2.name);
        if (exists)
        {
            Debug.Log("覆盖");
            mesh2 = AssetDatabase.LoadAssetAtPath(meshPath,typeof(Mesh))as Mesh;
            
        }

        Copy(mesh2,mesh);
        
        switch(destType){
            case DestType.Tangent://执行写入到 顶点切线
                var tangents = new Vector4[mesh2.vertexCount];
                    for (var j = 0; j < mesh2.vertexCount; j++)
                    {
                        tangents[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z, 0);
                    }
                    mesh2.tangents = tangents;
            break;
            
            case DestType.VertexColor:// 写入到顶点色
                    Color[] _colors = new Color[mesh.vertexCount];
                    for (var j = 0; j < mesh.vertexCount; j++)
                    {
                        // _colors[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z,colors[j].a);
                        _colors[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z,1);
                    }   
                    mesh2.colors = _colors;
            break;
            case DestType.UV2://写入第三套UV
                var uv2normal = new Vector2[mesh.vertexCount];
                for (var j = 0; j < mesh.vertexCount; j++)
                {
                    uv2normal[j] = new Vector2(averageNormals[j].x, averageNormals[j].y);
                }

                mesh2.uv3 = uv2normal;
                break;
            }

        if (!exists)
        {
            AssetDatabase.CreateAsset(mesh2, meshPath);
            Debug.Log("创建");
        }
        Debug.Log(mesh2.vertexCount);
    }

    public Vector3[] TBN_normal(Mesh mesh,Vector3[] smoothNormals)
    {
        normals = mesh.normals;
        vertices = mesh.vertices;
        tangents = mesh.tangents;
        Vector3[] tbnnormal=new Vector3[smoothNormals.Length];
        for (var i = 0; i < smoothNormals.Length; i++)
        {
            //tangent无需再归一化，否则会有轻微的断开
            Vector4 tangent = tangents[i];
            Vector4 normal = normals[i].normalized;
            Vector4 bitangent  = (Vector3.Cross(normal, tangent)* tangents[i].w).normalized;

            var tbn = new Matrix4x4(
                tangent,
                bitangent,
                normal,
                Vector4.zero);
            tbn = tbn.transpose;
            tbnnormal[i] = tbn.MultiplyVector(smoothNormals[i]).normalized*0.5f+new Vector3(0.5f,0.5f,0.5f);
        }
        
        return tbnnormal;
        
    }
    

    private Vector3[] SmoothData()
    {
        Vector3[] averageNormal = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            EditorUtility.DisplayProgressBar("正在处理中...", i.ToString(), (float)i / normals.Length);
            Vector3 nor = Vector3.zero;
            for (int j = 0; j < normals.Length; j++)
            {
                if(vertices[i] == vertices[j])
                {
                    nor += normals[j];
                }
            }
            //[-1, 1] -> [0, 1]
            //obj -> tangent
            averageNormal[i] = nor.normalized;
            // averageNormal[i] = Obj2Tangent2(nor.normalized, i)* 0.5f + Vector3.one * 0.5f;
        }
        Vector3[] resultNormal = TBN_normal(srcMesh, averageNormal);
        return resultNormal;
        Write2Mesh(srcMesh, resultNormal);
        EditorUtility.ClearProgressBar();
    }
    
    private Vector3[] SmoothDataByCS()
    {
        uint threadGroupSizeX;
        m_cs.GetKernelThreadGroupSizes(0, out threadGroupSizeX, out _, out _); 
        int xSize = (int)threadGroupSizeX;
        int meshSize = srcMesh.vertexCount;
        m_meshProp = new MeshProperties[meshSize];

        for(int i = 0; i < meshSize; i++)
        {
            MeshProperties meshProp = m_meshProp[i];
            meshProp.vertices = vertices[i];
            meshProp.normals = normals[i];
            meshProp.tangents = tangents[i];
            // meshProp.smoothNormal = Vector3.one;
            m_meshProp[i] = meshProp;
        }
        int stride = (3 + 3 + 4 + 3) * 4;
        m_meshPropBuffer = new ComputeBuffer(m_meshProp.Length, stride, ComputeBufferType.Default);
        m_meshPropBuffer.SetData(m_meshProp);
        m_cs.SetBuffer(0, "MeshPropBuffer", m_meshPropBuffer);
        m_cs.SetInt("vertexCount",meshSize);
        // Debug.LogError(meshSize);
        int dispatchGroupSize = Mathf.CeilToInt((float)meshSize / (float)xSize);
        // Debug.LogError(xSize);
        // Debug.LogError(dispatchGroupSize);
        m_cs.Dispatch(0, dispatchGroupSize, 1, 1);

        
        m_meshPropBuffer.GetData(m_meshProp);
        csResultNormal = new Vector3[meshSize];
        for (int i = 0; i < meshSize; i++)
        {
            csResultNormal[i] = m_meshProp[i].smoothNormal;
        }
        
        m_meshPropBuffer.Release();
        
        return csResultNormal;
    }

    private void ResetMesh()
    {
        Vector3[] resetVC = new Vector3[vertices.Length];
        for (int i = 0; i < resetVC.Length; i++)
        {
            resetVC[i] = new Vector3(1,1,1);
        }
        Write2Mesh(srcMesh, resetVC);
    }
    
    #region 计算结果有点问题的部分

    public  void SmoothNormalPrev(DestType wt)//Mesh选择器 修改并预览
    {  


        if(Selection.activeGameObject==null){//检测是否获取到物体
            Debug.LogError("请选择物体");
            return ;
        }

        srcMesh = Selection.activeGameObject.transform.GetMesh();
        
        MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinMeshRenders = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var meshFilter in meshFilters)//遍历两种Mesh 调用平滑法线方法
        {
            Mesh mesh = meshFilter.sharedMesh;
            Vector3 [] averageNormals= AverageNormal(mesh);
            Write2Mesh(mesh,averageNormals);
        }
        foreach (var skinMeshRender in skinMeshRenders)
        {   
            Mesh mesh = skinMeshRender.sharedMesh;
            Vector3 [] averageNormals= AverageNormal(mesh);
            Write2Mesh(mesh,averageNormals);
        }
    }

    public Vector3[] AverageNormal(Mesh mesh)
    {
        
        var averageNormalHash = new Dictionary<Vector3, Vector3>();
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            if (!averageNormalHash.ContainsKey(mesh.vertices[j]))
            {
                averageNormalHash.Add(mesh.vertices[j], mesh.normals[j]);
            }
            else
            {
                averageNormalHash[mesh.vertices[j]] =
                    (averageNormalHash[mesh.vertices[j]] + mesh.normals[j]).normalized;
            }
        }

        var averageNormals = new Vector3[mesh.vertexCount];
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            averageNormals[j] = averageNormalHash[mesh.vertices[j]];
            // averageNormals[j] = averageNormals[j].normalized;
        }
        
        // return averageNormals;
        return TBN_normal(mesh, averageNormals);
        
    } 
    
    public void selectMesh(){

        if(Selection.activeGameObject==null){//检测是否获取到物体
            Debug.LogError("请选择物体");
            return ;
        }
        MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinMeshRenders = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var meshFilter in meshFilters)//遍历两种Mesh 调用平滑法线方法
        {
            Mesh mesh = meshFilter.sharedMesh;
            Vector3 [] averageNormals= AverageNormal(mesh);
            ExportMesh(mesh,averageNormals);
            
        }
        foreach (var skinMeshRender in skinMeshRenders)
        {   
            Mesh mesh = skinMeshRender.sharedMesh;
            Vector3 [] averageNormals= AverageNormal(mesh);
            ExportMesh(mesh,averageNormals);
        }
    }

    #endregion
    private Vector3 Obj2Tangent(Vector3 ori, int id)
    {
        Vector4 t4 = tangents[id];
        //tbn
        Vector3 t = new Vector3(t4.x, t4.y, t4.z);
        Vector3 n = normals[id];
        Vector3 b = Vector3.Cross(n, t) * t4.w;

        Vector3 tNor = Vector3.zero;
        tNor.x = t.x * ori.x + t.y * ori.y + t.z * ori.z;
        tNor.y = b.x * ori.x + b.y * ori.y + b.z * ori.z;
        tNor.z = n.x * ori.x + n.y * ori.y + n.z * ori.z;

        return tNor;
    }
    
    private Vector3 Obj2Tangent2(Vector3 ori, int i)
    {
        Vector4 tangent = srcMesh.tangents[i];
        Vector4 normal = srcMesh.normals[i].normalized;
        Vector4 bitangent  = (Vector3.Cross(normal, tangent)* srcMesh.tangents[i].w).normalized;

        var tbn = new Matrix4x4(
            tangent,
            bitangent,
            normal,
            Vector4.zero);
        tbn = tbn.transpose;
        return tbn.MultiplyVector(ori).normalized;
    }
}
