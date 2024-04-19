using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleManager : MonoBehaviour
{
    public Transform Content;
    public Slider Controller;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeDistance() 
    {
        Content.transform.localScale = new Vector3(Controller.value, Controller.value, Controller.value);
    }
}
