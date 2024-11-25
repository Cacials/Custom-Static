using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
// using UnityEditor;
using UnityEngine;

public class PosDebug : MonoBehaviour
{
    public Transform Trans;
    public Vector3 posTarget;//挖洞位置
    public Vector3 posOrigin;//木条pos
    public Vector3 rotOrigin;//木条Rotation
    public PolygonCollider2D poly;

    public Vector4[] posOSs;
    public Material filterMat;
    public List<Vector4> outPos;

    public Texture2D originTex;
    private int downSample = 1;
    public Shader shader;

    //开洞木板用，把挖洞位置的WS转换到OS,再传到shader中
    [Button]
    public void posdebug()
    {
        Matrix4x4 mat=Matrix4x4.TRS(posOrigin, Quaternion.Euler(rotOrigin), Vector3.one);
        Vector3 OS = mat.inverse.MultiplyPoint(posTarget);
        Debug.LogError(mat);
        Debug.LogError(OS);
        if (Trans)
        {
            Debug.LogError(Trans.transform.position);
            Debug.LogError(Trans.transform.worldToLocalMatrix);
        }
        
    }

    [Button]
    public void PosOSSubmit()
    {
        if (posOSs == null)
        {
            Debug.LogError(121212);
        }
        else
        {
            filterMat.SetVectorArray("_PosTargetsOS",posOSs);
            filterMat.SetInt("_TargetCount",posOSs.Length);
            
            
            
            filterMat.GetVectorArray("_PosTargetsOS",outPos);
            
            Debug.LogError(outPos[outPos.Count-1]);
            Debug.LogError(posOSs[posOSs.Length-1]);
            Debug.LogError(posOSs.Length);
        }
        
    }
    
    public void  SaveTexture2D(Texture2D tex, string file)
    {
        byte[] bytes = tex.EncodeToPNG ();
        UnityEngine.Object.DestroyImmediate (tex);
        System.IO.File.WriteAllBytes (file, bytes);
        Debug.Log ("write to File over");
        // AssetDatabase.Refresh (); //自动刷新资源
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

    [Button]
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
        // AssetDatabase.Refresh();
        
        filterMat = CreateFilterMat(shader, filterMat);
        
        RenderTexture buffer0 = RenderTexture.GetTemporary(IMG_WIDTH, IMG_HEIGHT, 0);
        // Sprite sprite = Sprite.Create(buffer0,new Rect(0, 0, IMG_WIDTH, IMG_HEIGHT),Vector2.one*0.5);
        buffer0.filterMode = FilterMode.Bilinear;
        filterMat.SetColor("_Color", Color.cyan);
        PosOSSubmit();
        Texture2D targetTex = new Texture2D (IMG_WIDTH, IMG_HEIGHT, TextureFormat.ARGB32, false);
        Graphics.Blit(originTex, buffer0, filterMat, 0);
        // Graphics.Blit(originTex, buffer0);
        targetTex.ReadPixels(new Rect(0, 0, IMG_WIDTH, IMG_HEIGHT), 0, 0);
        RenderTexture.ReleaseTemporary(buffer0);
        SaveTexture2D(targetTex,path+"/"+originTex.name+".png");
    }
}
