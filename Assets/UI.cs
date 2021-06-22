using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private bool showUI = false;
    public Transform ui;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.G))
        {
            showUI = !showUI;
            Debug.Log("111111");
        }
    }

    private void OnGUI()
    {
        
        if (showUI)
        {

        }
    }
}
