using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPause : UICanvas
{
    [SerializeField] private GameObject InGameUI;
    void OnEnable()
    {
        if (InGameUI == null)
        {
            Debug.LogError("InGameUI is not assigned in UIPause");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager instance not ready yet, disabling InGameUI.");
            InGameUI.SetActive(false);
            return;
        }

        InGameUI.SetActive(GameManager.Instance.InGame);
    }

    void OnDisable()
    {
      
    }

    public void Restart() 
    {
        
    }
    public void Resume()
    {
        
    }
    public void Home()
    {
        
    }



}
