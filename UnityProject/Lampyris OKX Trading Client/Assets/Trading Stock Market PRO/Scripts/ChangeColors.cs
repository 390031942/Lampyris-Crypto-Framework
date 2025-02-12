using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to change the screen colors of the menus and display
/// </summary>
public class ChangeColors : MonoBehaviour
{
    // Start is called before the first frame update
    public static ChangeColors instance;


    [Header("Dark Colors")]
    public Color backgroundColor_Dark;
    public Color menuColor_Dark;
    public Color textColor_Dark;
    public Color textColor_selected_Dark;
    public Color menuLinesColor_Dark;

    [Header("Light Colors")]
    public Color backgroundColor_Light;
    public Color menuColor_Light;
    public Color textColor_Light;
    public Color textColor_selected_Light;
    public Color menuLinesColor_Light;

    [Header("Color state")]
    public bool colDark;

    [Header("Texts")]
    public Text[] texts;
    public Image background;
    public Image[] menus;
    public Image[] menuLines;

    void Start()
    {
        instance = this;

        
    }

    
    /// <summary>
    /// Called from the button
    /// </summary>
    public void ChangeColorsCallback()
    {
        colDark = !colDark;


        if(colDark)
        {
            //texts colors
            for(int ii=0; ii<texts.Length; ii++)
            {
                texts[ii].color = textColor_Dark;
            }
            //menu colors
            for (int ii = 0; ii < menus.Length; ii++)
            {
                menus[ii].color = menuColor_Dark;
            }
            //line colors
            for (int ii = 0; ii < menuLines.Length; ii++)
            {
                menuLines[ii].color = menuLinesColor_Dark;
            }

            background.color = backgroundColor_Dark;

            MenuManager.instance.colNormal = textColor_Dark;
            MenuManager.instance.colSelected = textColor_selected_Dark;

           

        }
        else
        {
            //texts colors
            for (int ii = 0; ii < texts.Length; ii++)
            {
                texts[ii].color = textColor_Light;
            }
            //menu colors
            for (int ii = 0; ii < menus.Length; ii++)
            {
                menus[ii].color = menuColor_Light;
            }
            //line colors
            for (int ii = 0; ii < menuLines.Length; ii++)
            {
                menuLines[ii].color = menuLinesColor_Light;
            }


            background.color = backgroundColor_Light;

            MenuManager.instance.colNormal = textColor_Light;
            MenuManager.instance.colSelected = textColor_selected_Light;

       
        }

        MenuManager.instance.SetMenuActive(MenuManager.instance.activeButton);
    }


}
