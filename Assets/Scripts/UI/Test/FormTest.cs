using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            UIManager.Instance.ShowUIForm("TestPanel1");
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            UIManager.Instance.ShowUIForm("TestPanel2");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            UIManager.Instance.HideUIForm("TestPanel1");
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            UIManager.Instance.HideUIForm("TestPanel2");
        }
    }
}
