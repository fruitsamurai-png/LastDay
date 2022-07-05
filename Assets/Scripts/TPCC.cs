/*******************************************************************************
File:      TPCC.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      10/1/2020

Description:
    This component provides a simple implementation for a Third Person Character
    Controller (TPCC). It is best used at the root of the player hierarchy and
    controls its physics. In addition, it makes some assumptions on how the
    hierarchy is built:

    Player (Root) - [Transform, RigidBody, Collider, TPCC]
        PlayerModel - [Transform, Mesh Filter, MeshRenderer]
        GroundDetector - [Transform, Collider (Trigger), GroundDetector]
        YawPivot - [Transform (Position 0,0,0)]
            MovePivot - [Transform]
                PitchPivot - [Transform (Position 0,0,0)]
                    Camera - [Transform, Camera, CameraOcclusion]

    Each part of the hierarchy is built with purpose, pay close attention to
    subsequent comments if you wish to modify / customize it.

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TPCC : MonoBehaviour
{
    //Physics Control Parameters
    public float MoveSpeed = 5f;
    public float RotateSpeed = 3f;
    public float JumpSpeed = 5f;

    //Camera Pitch Parameters
    public float PitchInterpolationSpeed = 0.1f;
    public float MaxPitchAngle = +85f;
    public float MinPitchAngle = -85f;
    private float PitchAngle = 0f;
    private bool cheatenabled = false;
    //Camera Yaw Parameters
    public float YawInterpolationSpeed = 2f;
    private float originalmovespeed;
    //External References (Within Prefab)
    public GroundDetector GD;
    public Transform YawPivotTransform;
    public Transform PitchPivotTransform;
    public Transform CameraTransform;
    public Transform PlayerModelRootTransform;

    //Components on this object
    private Rigidbody RB;

    // Start is called before the first frame update
    void Start()
    {
        //Cache component references
        RB = GetComponent<Rigidbody>();
        originalmovespeed = MoveSpeed;
        //Make sure the camera starts behind the player
        YawPivotTransform.localRotation = Quaternion.Euler(0f, PlayerModelRootTransform.localRotation.y, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMove();
        CameraYaw();
        CameraPitch();
        PlayerJump();
    }

    //All code responsible for moving the player. 
    //The chracter controller chosen is stiff but responsive with 
    //instant character acceleration and deceleration.
    private void PlayerMove()
    {
        //Rotate player
        if (Input.GetKey(KeyCode.A))
            PlayerModelRootTransform.Rotate(0.0f, -1.0f * RotateSpeed, 0.0f, Space.Self);

        if (Input.GetKey(KeyCode.D))
            PlayerModelRootTransform.Rotate(0.0f, 1.0f * RotateSpeed, 0.0f, Space.Self);

        //Reset direction vector (every frame)
        Vector3 dir = Vector3.zero;

        //Set movement direction based on where the YawPivot is facing
        if (Input.GetKey(KeyCode.W))
            dir += PlayerModelRootTransform.forward;

        if (Input.GetKey(KeyCode.S))
            dir -= PlayerModelRootTransform.forward;

        if (Input.GetKey(KeyCode.E))
            dir += PlayerModelRootTransform.right;

        if (Input.GetKey(KeyCode.Q))
            dir -= PlayerModelRootTransform.right;

        //Normalize direction and apply MoveSpeed
        dir = dir.normalized * MoveSpeed;
        RB.velocity = new Vector3(dir.x, RB.velocity.y, dir.z);
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            if(!cheatenabled)
            {
                cheatenabled = true;
                Debug.Log("Cheat enabled");
                MoveSpeed = originalmovespeed * 2.5f;
            }
            else
            {
                cheatenabled = false;
                Debug.Log("Cheat disabled");
                MoveSpeed = originalmovespeed;
            }
        }
        if(TextTrigger.gomenu==true)
        {
            Invoke("GoToMenu", 6.5f);
        }
    }
    
    //Interpolates the YawPivot around the Y axis as the player rotates (DO NOT ROTATE AROUND OTHER AXIS)
    private void CameraYaw()
    {
        //Get the difference between the current camera yaw and the player yaw
        var yawDiff = PlayerModelRootTransform.localEulerAngles.y - YawPivotTransform.localEulerAngles.y;
        //Correct any Euler wrap-around issues
        if (yawDiff > 180.0f)
            yawDiff -= 360.0f;
        if (yawDiff < -180.0f)
            yawDiff += 360.0f;
        //Determine what the new yaw should be (defaulting to the player yaw)
        var newYaw = PlayerModelRootTransform.localEulerAngles.y;
        //Interpolate the camera yaw over time, making sure not to overshoot
        if (yawDiff > 180.0f * Time.deltaTime * YawInterpolationSpeed)
            newYaw = YawPivotTransform.localEulerAngles.y + 180.0f * Time.deltaTime * YawInterpolationSpeed;
        if (yawDiff < -180.0f * Time.deltaTime * YawInterpolationSpeed)
            newYaw = YawPivotTransform.localEulerAngles.y - 180.0f * Time.deltaTime * YawInterpolationSpeed;
        //Set the camera yaw to it's new rotation
        YawPivotTransform.localRotation = Quaternion.Euler(0f, newYaw, 0f);
    }
    private void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    //Rotates the PitchPivot around the X axis based on the terrain ahead of the player (DO NOT ROTATE AROUND OTHER AXIS)
    private void CameraPitch()
    {
        //Adjust these distances and logic to work better with your terrain and scale
        var shortAngle = CameraPitchCheck(2.0f, 5.0f);
        var mediumAngle = CameraPitchCheck(5.0f, 100.0f);
        var longAngle = CameraPitchCheck(5.0f, 25.0f);
        PitchAngle = shortAngle;
        if (shortAngle >= mediumAngle && mediumAngle >= longAngle) //Ground going up
            PitchAngle = longAngle;
        if (shortAngle <= mediumAngle && mediumAngle <= longAngle) //Ground going down
            PitchAngle = longAngle;
        if (shortAngle >= mediumAngle && mediumAngle <= longAngle) //Ground going up then down
            PitchAngle = mediumAngle;
        if (shortAngle <= mediumAngle && mediumAngle >= longAngle) //Ground going down then up
            PitchAngle = mediumAngle;

        //Get the difference between the current camera pitch and the desired pitch
        var pitchDiff = PitchAngle - PitchPivotTransform.localEulerAngles.x;
        //Correct any Euler wrap-around issues
        if (pitchDiff > 180.0f)
            pitchDiff -= 360.0f;
        if (pitchDiff < -180.0f)
            pitchDiff += 360.0f;
        //Determine what the new yaw should be (defaulting to the desired pitch)
        var newPitch = PitchAngle;
        //Interpolate the camera pitch over time, making sure not to overshoot
        if (pitchDiff > 180.0f * Time.deltaTime * PitchInterpolationSpeed)
            newPitch = PitchPivotTransform.localEulerAngles.x + 180.0f * Time.deltaTime * PitchInterpolationSpeed;
        if (pitchDiff < -180.0f * Time.deltaTime * PitchInterpolationSpeed)
            newPitch = PitchPivotTransform.localEulerAngles.x - 180.0f * Time.deltaTime * PitchInterpolationSpeed;
        //Correct any Euler wrap-around issues
        if (newPitch > 180.0f)
            newPitch -= 360.0f;
        if (newPitch < -180.0f)
            newPitch += 360.0f;
        //Update camera pitch while clamping it
        newPitch = Mathf.Clamp(newPitch, MinPitchAngle, MaxPitchAngle);
        PitchPivotTransform.localRotation = Quaternion.Euler(newPitch, 0f, 0f);
    }

    private float CameraPitchCheck(float forwardDistance, float downDistance)
    {
        //How far forward are we going to look?
        var forwardCastDistance = forwardDistance;
        //Look upwards a bit to handle ramps better
        var forwardAndUp = (PlayerModelRootTransform.forward * 2.0f + PlayerModelRootTransform.up).normalized;
        //Do the raycast
        Ray forwardRay = new Ray(PlayerModelRootTransform.position, forwardAndUp);
        RaycastHit[] forwardResults = Physics.RaycastAll(forwardRay, forwardCastDistance);
        //Find the nearest thing hit
        if (forwardResults.Length > 0) //If any objects are hit
        {
            foreach (RaycastHit result in forwardResults) //Loop through all objects hit
            {
                if (result.distance < forwardCastDistance)
                    forwardCastDistance = result.distance;
            }
        }
        //Reduce the distance 10% from the thing hit
        forwardCastDistance *= 0.9f;

        //How far down are we going to look?
        var downCastDistance = downDistance;
        //Look down from the point the forward cast collided (minus the 10%)
        var downCastPoint = PlayerModelRootTransform.position + forwardAndUp * forwardCastDistance;
        //Do the raycast
        Ray downRay = new Ray(downCastPoint, -PlayerModelRootTransform.up);
        RaycastHit[] downResults = Physics.RaycastAll(downRay, downCastDistance);
        //Find the nearest thing hit
        if (downResults.Length > 0) //If any objects are hit
        {
            foreach (RaycastHit result in downResults) //Loop through all objects hit
            {
                if (result.distance < downCastDistance)
                    downCastDistance = result.distance;
            }
        }
        //Determine the relative (to the player) height of the collision
        var relativeGroundHeight = forwardAndUp.y * forwardCastDistance - downCastDistance;
        //Remove the vertical component so we can get the horizontal distance
        forwardAndUp.y = 0.0f;
        //Determine the relative (to the player) distance to the collision
        var relativeGroundDistance = (forwardAndUp * forwardCastDistance).magnitude;
        //Return the angle in degrees
        return -180.0f * Mathf.Atan2(relativeGroundHeight, relativeGroundDistance) / Mathf.PI;
    }

    //Applies a character jump when the spacebar is pressed and the player
    // is grounded.
    private void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && GD.IsGrounded())
        {
            RB.velocity = new Vector3(RB.velocity.x, JumpSpeed, RB.velocity.z);
        }
    }

}
