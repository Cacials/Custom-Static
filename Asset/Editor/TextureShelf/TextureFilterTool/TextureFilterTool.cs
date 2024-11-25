using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public enum FilterType
{
    SNN=0,
    Kuwahara=1,
}
public class TextureFilterTool : EditorWindow
{
    private Texture2D originTex;
    private FilterType filterType;
    
    private Shader snnShader;
    private Shader kuwaharaShader;

    private Material filterMat;
    
    //迭代次数
    [Range(0, 6)]
    private int iterations = 3;

    //降采样
    [Range(1, 8)]
    private int downSample = 2;

    [MenuItem("Tools/Texture/Texture后处理工具")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureFilterTool));//显示现有窗口实例。如果没有，请创建一个。
    }
    
    private void OnGUI()
    {
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        snnShader=(Shader)EditorGUILayout.ObjectField("SNNShader",snnShader, typeof(Shader),false);
        kuwaharaShader=(Shader)EditorGUILayout.ObjectField("KuwaharaShader",kuwaharaShader, typeof(Shader),false);
        EditorGUILayout.EndHorizontal();
        iterations=EditorGUILayout.IntSlider("iterations",iterations, 1, 12);
        downSample=EditorGUILayout.IntSlider("downSample",downSample, 1, 8);
        originTex=(Texture2D)EditorGUILayout.ObjectField("OriginTexture",originTex, typeof(Texture2D),false);
        GUILayout.Space(10);
        filterType=(FilterType)EditorGUILayout.EnumPopup("Filter",filterType);
        GUILayout.Space(10);

        if (GUILayout.Button("Render"))
        {
            FilterRender();
        }

        // if (GUILayout.Button("SaveTex"))
        // {
        //     // SaveTexture2D();
        // }
        
    }

    public void  SaveTexture2D(Texture2D tex, string file)
    {
        byte[] bytes = tex.EncodeToPNG ();
        UnityEngine.Object.DestroyImmediate (tex);
        System.IO.File.WriteAllBytes (file, bytes);
        Debug.Log ("write to File over");
        UnityEditor.AssetDatabase.Refresh (); //自动刷新资源
    }

    private Material CreateFilterMat(Shader shader, Material material)
    {
        if (shader == null)
        {
            Debug.LogError("对应shder未选择");
            return null;
        }

        if (shader.isSupported && material && material.shader == shader)
        {
            return material;
        }
        
        if (!shader.isSupported) 
        {
            return null;
        }
        else 
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            if (material)
                return material;
            else 
                return null;
        }
        return material;
    }

    private void FilterRender()
    {
        int IMG_WIDTH=originTex.width/downSample;
        int IMG_HEIGHT=originTex.height/downSample;
        //创建文件夹路径
        string path = Application.dataPath + "/TextureFilterTools/Export";
        Debug.Log(path);
        //判断文件夹路径是否存在
        if (!Directory.Exists(path))
        {  
            Directory.CreateDirectory(path);
        }
        //刷新
        AssetDatabase.Refresh();
        
        switch (filterType)
        {
            case(FilterType.SNN):
                filterMat = CreateFilterMat(snnShader, filterMat);
                
                break;
            case(FilterType.Kuwahara):
                filterMat = CreateFilterMat(kuwaharaShader, filterMat);
                break;
        }
        
        RenderTexture buffer0 = RenderTexture.GetTemporary(IMG_WIDTH, IMG_HEIGHT, 0);
        buffer0.filterMode = FilterMode.Bilinear;
        filterMat.SetInt("_Iterations", iterations);
        Texture2D targetTex = new Texture2D (IMG_WIDTH, IMG_HEIGHT, TextureFormat.ARGB32, false);
        Graphics.Blit(originTex, buffer0, filterMat, 0);
        // Graphics.Blit(originTex, buffer0);
        targetTex.ReadPixels(new Rect(0, 0, IMG_WIDTH, IMG_HEIGHT), 0, 0);
        RenderTexture.ReleaseTemporary(buffer0);
        Debug.LogError(iterations);
        SaveTexture2D(targetTex,path+"/"+originTex.name+".png");
    }
}
