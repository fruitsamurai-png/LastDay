using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextTrigger : MonoBehaviour
{
    public GameObject UIObject;
    public GameObject Trigger;
    public string scenename;
    public static bool gomenu=false;
    // Start is called before the first frame update
    void Start()
    {
        UIObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            //Debug.Log("here");
            UIObject.SetActive(true);
            StartCoroutine("WaitforSecs");
            gomenu = true;
        }
    }
    
    IEnumerator WaitforSecs()
    {
        yield return new WaitForSeconds(5);
        //Destroy(UIObject);
        Destroy(gameObject);
    }
}
