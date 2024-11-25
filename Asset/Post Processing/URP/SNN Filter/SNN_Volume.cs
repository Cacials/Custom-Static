using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[Serializable, VolumeComponentMenu("PostPro/SNN")]
public class SNN_Volume : VolumeComponent, IPostProcessComponent
{

    public BoolParameter use_PostPro =new BoolParameter(true) ;
    public ClampedIntParameter iterations = new ClampedIntParameter(2, 0, 8); 
    public ClampedIntParameter downSamples = new ClampedIntParameter(3, 1, 6);
    
    public bool IsActive()
    {
        return active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
