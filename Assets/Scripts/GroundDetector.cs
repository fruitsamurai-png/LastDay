/*******************************************************************************
File:      GroundDetector.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      10/1/2020

Description:
    This component is used to track whether the player is grounded. It uses a
    trigger collider to count the number of obejects currently in contact.

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    //Number of non-trigger objects currently in contact
    public int ContactCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
            ++ContactCount;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
            --ContactCount;
    }

    public bool IsGrounded()
    {
        if (ContactCount > 0)
            return true;
        else
            return false;
    }
}
