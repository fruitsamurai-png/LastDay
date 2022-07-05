/*******************************************************************************
File:      SpecialOccluder.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      10/1/2020

Description:
    This component is added to any object that should be treated specially when
    handling occlusion.

    The object will be ignored by the occlusion zoom and instead the
    HandleOcclusion function will be called.

    The typical way to handle the occlusion is to swap its material to a
    semi-transparent one, but more modes can be added to handle it in different
    ways.

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialOccluder : MonoBehaviour
{
    //Materials to swap upon occlusion
    public Material UsualMaterial;
    public Material TransparentMaterial;

    //Whether the object is currently occluding the camera
    private bool IsOccluding = false;
    
    //Reference to its own MeshRenderer to modify
    private MeshRenderer MR;

    // Start is called before the first frame update
    void Start()
    {
        MR = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOccluding)
            MR.material = TransparentMaterial;
        else
            MR.material = UsualMaterial;

        IsOccluding = false;
    }

    public void HandleOcclusion()
    {
        IsOccluding = true;
    }
}
