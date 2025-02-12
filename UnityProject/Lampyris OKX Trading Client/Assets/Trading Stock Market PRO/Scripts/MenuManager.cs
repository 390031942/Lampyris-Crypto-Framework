using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("Menu buttons on top of the app")]
    public GameObject[] menuButtons;

    [Header("Colors")]
    public Color colNormal;
    public Color colSelected;

    [Header("To know what button is active")]
    public int activeButton=0;

    [Header("Secondary menus associated to each of the buttons")]
    public GameObject[] secondaryMenus;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        SetMenuActive(0);
    }

    /// <summary>
    /// Sets a menu active by calling this function in a button
    /// </summary>
    /// <param name="a"> True or false --> enable/disable </param>
    public void SetMenuActive(int a)
    {
        // Color change if selected/deselected
        for(int ii=0;ii<menuButtons.Length;ii++)
        {
            menuButtons[ii].transform.GetChild(0).GetComponent<Text>().color = colNormal;
            menuButtons[ii].transform.GetChild(1).gameObject.SetActive(false);

        }

        for (int ii = 0; ii < secondaryMenus.Length; ii++)
        {
            if (secondaryMenus[ii]!=null)
            {
                secondaryMenus[ii].SetActive(false);
            }
        }

        
        // active button index updated
        activeButton = a;

        // display line at the bottom of the button
        menuButtons[a].transform.GetChild(0).GetComponent<Text>().color = colSelected;
        menuButtons[a].transform.GetChild(1).gameObject.SetActive(true);

    }

    /// <summary>
    /// Stop running the app
    /// </summary>
    public void QuitApp()
    {
        Application.Quit();
        Debug.Log("Quit app");
    }
}

