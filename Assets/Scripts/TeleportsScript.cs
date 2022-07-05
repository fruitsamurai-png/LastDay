using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportsScript : MonoBehaviour
{
    public Transform teleporttarget;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("here");
        if (other.CompareTag("Player"))
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            cc.enabled = false;
            player.transform.position = teleporttarget.transform.position;
            cc.enabled = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
