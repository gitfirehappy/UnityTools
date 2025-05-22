using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormTest : MonoBehaviour
{
    public GameObject[] preloadPrefabs;

    private void Start()
    {
        UIManager.Instance.PreLoadForms(preloadPrefabs);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            UIManager.Instance.ShowUIForm<TestPanel1>();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            UIManager.Instance.ShowUIForm("TestPanel2");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            UIManager.Instance.ShowUIForm("TestPanel3");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            UIManager.Instance.HideUIForm<TestPanel1>();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            UIManager.Instance.HideUIForm("TestPanel2");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            UIManager.Instance.HideUIForm("TestPanel3");
        }

        if (UIManager.Instance.HasActiveForm())
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIManager.Instance.HideUIFormTurn();
            }
        }
    }
}
