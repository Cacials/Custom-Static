using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adobe.Substance.Runtime;
using Sirenix.OdinInspector;

public class SbsarController : MonoBehaviour
{
    public SubstanceRuntimeGraph mySubstance;
    // public Adobe.Substance.SubstanceGraphSO m_SubstanceGO;
    
    void Start()
    {
        // mySubstance.AttachGraph(m_SubstanceGO);
        // UpdateSubstance();
    }

    [Button]
    public void UpdateSubstance(string text) 
    { 
          
        // Color color = new Color(0.237f, 0.834f, 0.045f, 1.0f); 
        // Vector2 panelSize = new Vector2(0.101f, 0.209f); 
        // float wearLevel = 0.977f; 
          
        // // panel color 
        // mySubstance.SetInputColor("paint_color", color); 
        mySubstance.SetInputString("text", text); 
  
        // // panel size 
        // mySubstance.SetInputVector2("square_open", panelSize); 
  
        // // wear level 
        // mySubstance.SetInputFloat("wear_level", wearLevel);

        mySubstance.RenderAsync();
        // // queue for render 
        // mySubstance.QueueForRender(); 
        // //render all substances async 
        // Substance.Game.Substance.RenderSubstancesAsync(); 

    } 
}
