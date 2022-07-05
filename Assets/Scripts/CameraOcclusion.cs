/*******************************************************************************
File:      CameraOcclusion.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      10/1/2020

Description:
    This component is added to the camera to detact and resolve occlusion and
     regulate camera zoom.

    There are 3 implemented ways that this script handles occlusion:

        1. Move the camera closer to the player (Zoom)
        2. Ignore the occluding object
        3. Handle objects with SpecialOccluder according to its rules

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOcclusion : MonoBehaviour
{
    //Camera Zoom Parameters
    public float Zoom = -10f;
    public float MaxZoom = 0f;
    public float MinZoom = -15f;
    public float ZoomSensitivity = 1.0f;

    //Interpolant factor for moving the camera (0.1 - 0.25 recommended)
    [Range(0f, 1.0f)]
    public float ZoomInterpolant = 0.15f;

    //Additional Zoom to add whenever occlusion is detected
    public float AddedOcclusionZoomOffset = 0.5f;

    //Mask applied to occlusion raycast
    public LayerMask OcclusionMask;

    //MovePivotObject
    public Transform MovePivotTransform;

    //Whether to print debug messages and do debug draw
    public bool DoDebug = false;

    // Start is called before the first frame update
    void Start()
    {
        //Set initial zoom (in case the camera's position doesn't match Zoom)
        Zoom = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        CameraZoom();
        OcclusionCheck();
    }

    void OcclusionCheck()
    {
        //zCamDist starts the frame at Zoom but can be shortened if occluded
        //Its value at the end of frame will be used to determine how much to zoom the camera in
        float zCamDist = Zoom;

        //Construct ray to cast (Pivot to Player)
        Vector3 playerPos = MovePivotTransform.position;
        Vector3 playerToCamDir = (transform.position - playerPos).normalized;

        //DebugDraw ray
        if (DoDebug)
            Debug.DrawRay(playerPos, playerToCamDir, Color.red);

        //Perform raycast
        Ray occlusionRay = new Ray(playerPos, playerToCamDir);
        RaycastHit[] results = Physics.RaycastAll(occlusionRay, Mathf.Abs(Zoom), OcclusionMask);

        //Debug Message - Print Objects hit by raycast
        if (DoDebug)
            Debug.Log("Camera Occlusion - Raycast Hit " + results.Length + " objects.");

        if (results.Length > 0) //If any objects are hit
        {
            foreach (RaycastHit result in results) //Loop through all objects hit
            {
                if (result.distance < Mathf.Abs(Zoom)) //If they are between the player and camera
                {
                    //Determine whether they have a component to handle occlusion differently
                    SpecialOccluder occluder = result.transform.GetComponent<SpecialOccluder>();
                    if (occluder != null)
                    {
                        //Debug Message - Print whether a special occluder was hit
                        if (DoDebug)
                            Debug.Log("Camera Occlusion - Special Occluder Hit");

                        occluder.HandleOcclusion();
                    }
                    else
                    {
                        //If they are not a special occluder, attempt to change ZCamDist if it is closer to player
                        zCamDist = Mathf.Max(zCamDist, -(result.distance - AddedOcclusionZoomOffset));
                    }
                }
            }
        }

        //Finally apply the movement to camera at the end of frame (lerped for smooth motion)
        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0f, 0f, zCamDist), ZoomInterpolant);
    }

    //Controls the camera Zoom (Camera's local Z position) as the
    //player scrolls the mouse wheel.
    private void CameraZoom()
    {
        Zoom += Input.mouseScrollDelta.y * ZoomSensitivity;
        Zoom = Mathf.Clamp(Zoom, MinZoom, MaxZoom);
    }

}
