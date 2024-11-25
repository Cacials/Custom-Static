using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class SDFGenerateTool : EditorWindow
{
    private Texture2D originTex;
    private ComputeShader computeShader;
    // debug用材质球
    // private Material unlitMat;
    private int sampleScale;

    private int IMG_WIDTH;
    private int IMG_HEIGHT;

    private int kernelID=0;
    private uint threadGroupSizeX;
    private uint threadGroupSizeY;

    private int spreadRange;

    private Color bgColor;

    private TextureFormat texFormat = TextureFormat.R8;

    [MenuItem("Tools/Texture/SDF图生成")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SDFGenerateTool));//显示现有窗口实例。如果没有，请创建一个。
    }
    
    private void OnGUI()
    {
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        computeShader=(ComputeShader)EditorGUILayout.ObjectField("ComputeShader",computeShader, typeof(ComputeShader),false);
        EditorGUILayout.EndHorizontal();
        // debug用材质球
        // unlitMat=(Material)EditorGUILayout.ObjectField("unlitMat",unlitMat, typeof(Material),false);
        
        sampleScale=EditorGUILayout.IntSlider("根据原图的缩放倍数",sampleScale, 1, 8);
        spreadRange = EditorGUILayout.IntField("描边宽度（单位：像素）", spreadRange);
        bgColor = EditorGUILayout.ColorField("描边（背景）颜色", bgColor);

        originTex=(Texture2D)EditorGUILayout.ObjectField("OriginTexture",originTex, typeof(Texture2D),false);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Render"))
        {
            Render();
        }

    }
    
    public void  SaveTexture2D(Texture2D tex, string file)
    {
        byte[] bytes = tex.EncodeToPNG ();
        UnityEngine.Object.DestroyImmediate (tex);
        System.IO.File.WriteAllBytes (file, bytes);
        Debug.Log ("write to File over");
        Debug.Log (file);
        UnityEditor.AssetDatabase.Refresh (); //自动刷新资源
        
        //自动修改压缩格式为不压缩，只留R通道 R8
        SetFormat(file);
    }

    public void SetFormat(string filePath)
    {
        string parseName = filePath.Replace('\\', '/').Replace(Application.dataPath.Replace("Assets",""),"");
        
        TextureImporter importer = AssetImporter.GetAtPath(parseName) as TextureImporter;
        // var setting = importer.GetPlatformTextureSettings("Android");
        if (importer != null)
        {
            var setting = importer.GetDefaultPlatformTextureSettings();
            setting.format = (TextureImporterFormat)texFormat;
            importer.SetPlatformTextureSettings(setting);
            AssetDatabase.Refresh (); //自动刷新资源
        }
        else
        {
            Debug.LogError("未找到importerPath , 未修改Format为R8");
        }
        
    }

    public void RunComputeShader(RenderTexture targetTex)
    {

        computeShader.SetTexture(kernelID,"Origin",originTex);
        computeShader.SetTexture(kernelID,"Result",targetTex);
        // unlitMat.SetTexture("_BaseMap",targetTex);
        computeShader.SetInt("_Width",IMG_WIDTH);
        computeShader.SetInt("_Height",IMG_HEIGHT);
        computeShader.SetInt("_SpreadRange",spreadRange*sampleScale);
        computeShader.SetVector("_BGColor",bgColor);
        
        computeShader.GetKernelThreadGroupSizes(kernelID,out threadGroupSizeX,out threadGroupSizeY,out _);
        int threadGroupsX = Mathf.CeilToInt((float)IMG_WIDTH / (float)threadGroupSizeX);
        int threadGroupsY = Mathf.CeilToInt((float)IMG_HEIGHT / (float)threadGroupSizeY);
        computeShader.Dispatch(kernelID,threadGroupsX,threadGroupsY,1);
    }

    private void Render()
    {
        IMG_WIDTH=originTex.width*sampleScale;
        IMG_HEIGHT=originTex.height*sampleScale;
        //创建文件夹路径
        string path = Application.dataPath + "/Editor/TextureShelf/SDFGenerator/Export";
        
        //判断文件夹路径是否存在
        if (!Directory.Exists(path))
        {  
            Directory.CreateDirectory(path);
        }
        //刷新
        AssetDatabase.Refresh();
        
        
        RenderTexture buffer0 = RenderTexture.GetTemporary(IMG_WIDTH, IMG_HEIGHT, 0);
        // buffer0.filterMode = FilterMode.Bilinear;
        buffer0.enableRandomWrite = true;
        
        Texture2D targetTex = new Texture2D (IMG_WIDTH, IMG_HEIGHT, TextureFormat.ARGB32, false);
        // targetTex.filterMode = FilterMode.Bilinear;

        
        RunComputeShader(buffer0);
        //设定buffer0为激活RT,才可以设置为ReadPixel的目标
        RenderTexture.active = buffer0;

        targetTex.ReadPixels(new Rect(0, 0, IMG_WIDTH, IMG_HEIGHT), 0, 0);
        RenderTexture.ReleaseTemporary(buffer0);


        SaveTexture2D(targetTex,path+"/"+originTex.name+".png");
    }
}
