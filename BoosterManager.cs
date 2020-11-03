using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterManager : Singleton<BoosterManager>
{
    public GameObject removeBooster;
    public GameObject extendingBooster;
    public GameObject colorBombBooster;

    public int removeBoosCount = 9;
    public int extendBoosCount = 8;
    public int colorBoosCount = 7;

    public Text removeBoosterText;
    public Text extendingBoosterText;
    public Text colorBombBoosterText;

    private void Start()
    {
        UpdateAndCheckBoosterCounter();
    }

    public void UpdateAndCheckBoosterCounter()
    {
        if(removeBooster != null && removeBoosterText != null)
        {
            removeBoosterText.text = removeBoosCount.ToString();

            IsThereRightFotRemove();
        }
        
        if(extendingBooster != null && extendingBoosterText != null)
        {
            extendingBoosterText.text = extendBoosCount.ToString();

            IsThereRightForExtending();
        }  
        
        if(colorBombBooster != null && colorBombBoosterText != null)
        {
            colorBombBoosterText.text = colorBoosCount.ToString();

            IsThereRightForColorBomb();
        }
    }

    public bool IsThereRightFotRemove()
    {
        if (removeBooster != null)
        {
            if(removeBoosCount <= 0)
            {
                return false;
            }
        }

        return true;
    }
    
    public bool IsThereRightForExtending()
    {
        if (extendingBooster != null)
        {
            if(extendBoosCount <= 0)
            {
                return false;
            }
        }

        return true;
    }  
    
    public bool IsThereRightForColorBomb()
    {
        if (colorBombBooster != null)
        {
            if(colorBoosCount <= 0)
            {
                return false;
            }
        }

        return true;
    }
}
