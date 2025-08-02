using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGamePlay : UICanvas
{
    public void PauseBtn()
    {
        if (GameManager.Instance.gameState != GameState.Paused)
        {
            GameManager.Instance.gameState = GameState.Paused;
        }

        UIManager.Instance.OpenUI<UIPause>();
    }

    public void UpdateLVName()
    {
       
    }
    public void UpdateSpeed()
    {
        
    }
    public void RemoveCanon() 
    {
        GameManager.Instance.RemoveCanon();
    }

    public void SlipBox() 
    {
        GameManager.Instance.SlipBus();
    }

    public void InCreaseRange() 
    {
        GameManager.Instance.Boost();
    }

    public void PushBack() 
    {
        
    }


}
