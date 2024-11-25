using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class HoleCal : MonoBehaviour
{
    public ComputeShader cs;
    public Texture m_tex;
    private RenderTexture m_mainTex;

    int m_texSize = 256;
    Renderer m_rend;

    [Range(0,2)]
    public float colDist;
    public Color bgCol;

    private ComputeBuffer allPosBuffer;
    private ComputeBuffer centersBuffer;
    private ComputeBuffer singleHolePosBuffer;
    private Vector2[] allPos;
    private List<Vector2> allPosLst = new List<Vector2>();
    private List<Vector2> singleHolePosLst = new List<Vector2>();

    private List<Vector2> centerLst = new List<Vector2>();

    public float diameter=30;
    
    public int missThreshold=5;
    
    

    // Start is called before the first frame update
    void Start()
    {
        InitData();
    }
    
    
    public List<Vector3> GetCenterLst(Vector3 posWS,float width = 9.5f,float height=11.8f)
    {
        HolePosCalculate();
        var lst = new List<Vector3>();
        Vector3 origin = posWS + new Vector3(-width / 2, -height / 2, 0);
        foreach (var center in centerLst)
        {
            Vector3 pos = origin + new Vector3(width*center.x/m_texSize, height*center.y/m_texSize, 0);
            lst.Add(pos);
            Debug.LogError(pos);
        }

        return lst;
    }
    
    
    private void InitData()
    {
        
        //如果需要可视化则保留
        if (m_mainTex == null)
        {
            m_mainTex = new RenderTexture(m_texSize, m_texSize, 0, RenderTextureFormat.ARGB32);
            m_mainTex.enableRandomWrite = true;
            m_mainTex.Create();
        }
        
        
        
        

        // send the values to the compute shader
        cs.SetFloat("_ColDist", colDist);
        cs.SetTexture(0, "Result", m_mainTex);
        cs.SetTexture(0, "ColTex", m_tex);

        m_rend = GetComponent<Renderer>();
        if (m_rend != null)
        {
            m_rend.enabled = true;
            m_rend.sharedMaterial.SetTexture("_BaseMap", m_mainTex);
        }
        

        if (allPosBuffer != null)
        {
            allPosBuffer.Release();
        }
        allPosBuffer = new ComputeBuffer(m_texSize*m_texSize, 8, ComputeBufferType.Append);
        allPosBuffer.SetCounterValue(0);
        
        cs.SetBuffer(0,"_AllPosBuffer",allPosBuffer);
        
        cs.SetFloat("_ColDist", colDist);
        cs.SetVector("_BgCol", bgCol);
        
        //dispatch the threads
        cs.Dispatch(0, m_texSize/32, m_texSize/32, 1);
    }
    
    
    private void RunCS()
    {
        cs.SetFloat("_ColDist", colDist);
        cs.SetVector("_BgCol", bgCol);
        cs.Dispatch(0, m_texSize/32, m_texSize/32, 1);
    }
    
    //计算结果可视化，Debug用
    [Button]
    private void RunCSShowResult()
    {
        
        if (centersBuffer != null)
        {
            centersBuffer.Release();
        }

        centersBuffer = new ComputeBuffer(centerLst.Count, 8, ComputeBufferType.Default);
        centersBuffer.SetData(centerLst);

        cs.SetTexture(1, "Result", m_mainTex);
        cs.SetInt("_CenterCount", centerLst.Count);
        cs.SetBuffer(1,"_CenterPosBuffer",centersBuffer);
        cs.Dispatch(1, m_texSize/32, m_texSize/32, 1);
    }
    
    [Button]
    void HolePosCalculate()
    {
        InitData();
        InitAllPosData();
        
        centerLst.Clear();
        
        while (allPosLst.Count>50)
        {

            singleHolePosLst.Add(allPosLst[0]);
            
            for (int j = 1; j < allPosLst.Count; j++)
            {
                if (Vector2.Distance(allPosLst[0],allPosLst[j]) < diameter)
                {
                    singleHolePosLst.Add(allPosLst[j]);
                }
            }

            for (int i = singleHolePosLst.Count-1; i>=0; i--)
            {
                if (IsIndependent(allPosLst[0],singleHolePosLst[i],singleHolePosLst))
                {
                    singleHolePosLst.Remove(singleHolePosLst[i]);
                }
            }
            

            foreach (var pos in singleHolePosLst)
            {
                allPosLst.Remove(pos);
            }
            HoleData data = new HoleData(singleHolePosLst);

            centerLst.Add(data.CenterPos());

            singleHolePosLst.Clear();
        }
        
        
    }

    //用CS筛出所有颜色范围内像素点的坐标
    void InitAllPosData()
    {
        allPosLst.Clear();
        allPos = new Vector2[m_texSize*m_texSize];
        for (int j = 0; j < allPos.Length; j++)
        {
            allPos[j]=Vector2.zero;
        }
        
        allPosBuffer.SetCounterValue(0);
        RunCS();
        
        allPosBuffer.GetData(allPos);
        foreach (var item in allPos)
        {
            if (item !=Vector2.zero)
            {
                allPosLst.Add(item);
            }
        }

    }
    
    //是否互相独立，属于不同的孔
    private bool IsIndependent(Vector2 origin ,Vector2 target,List<Vector2> posLst)
    {

        Vector2 dir = target - origin;

        Vector2 compRoundPos = new Vector2();
        
        float dist = Vector2.Distance(target, origin);

        int missCount = 0;
        for (int i = 0; i < dist; i++)
        {
            Vector2 stepPos;
            stepPos = origin+dir * i / dist;
            compRoundPos.x = Mathf.RoundToInt(stepPos.x);
            compRoundPos.y = Mathf.RoundToInt(stepPos.y);
            
            if (!posLst.Contains(compRoundPos))
            {
                missCount++;
            }

            if (missCount >= missThreshold)
            {
                return true;
            }
        }
        
        return false;
    }
    
    //是否互相独立，属于不同的孔_CS加速版本
    //todo: 没找到回传Bool的方式，暂时搁置
    private void RunCSIsIndependent(Vector2 origin ,Vector2 target,List<Vector2> posLst)
    {
        
        if (singleHolePosBuffer != null)
        {
            singleHolePosBuffer.Release();
        }

        singleHolePosBuffer = new ComputeBuffer(posLst.Count, 8, ComputeBufferType.Default);
        singleHolePosBuffer.SetData(posLst);

        // cs.SetTexture(1, "Result", m_mainTex);
        cs.SetInt("_SingleHolePosCount", posLst.Count);
        cs.SetInt("_MissThreshold", missThreshold);
        cs.SetVector("_Origin_Target_Pos", new Vector4(origin.x,origin.y,target.x,target.y));
        cs.SetBuffer(2,"_SingleHolePosLst",singleHolePosBuffer);
        float dist=Vector2.Distance(target, origin);
        int groupsCount=Mathf.CeilToInt(dist / 128f);
        cs.Dispatch(2, groupsCount, 1, 1);

        bool ss;
        
    }
}

public class HoleData
{
    private List<Vector2> singlePosLst;

    public HoleData(List<Vector2> lst)
    {
        singlePosLst = lst;
    }

    public Vector2 CenterPos()
    {
        Vector2 center=new Vector2();
        foreach (var pos in singlePosLst)
        {
            center += pos;
        }

        center /= singlePosLst.Count;

        return center;
    }
}
