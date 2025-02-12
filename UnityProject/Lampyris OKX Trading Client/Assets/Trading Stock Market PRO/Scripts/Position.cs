using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// creates a position for storing/displaying the partila benefits on the app
/// </summary>
public class Position : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Internal references to UI")]
    public Text text_name;
    public Image[] arrow;

    [Header("Colors for Sell/buy")]
    public Color[] col;

    [Header("Position data")]
    public int index;
    public string type;
    public float _benefit;


   /// <summary>
   /// Updates the data
   /// </summary>
   /// <param name="benefit">this is the value of the benefit of the position</param>
    public void UpdateValues(float benefit)
    {
        _benefit = benefit;

        arrow[0].gameObject.SetActive(false);
        arrow[1].gameObject.SetActive(false);

        if (benefit > 0)
        {
            text_name.color = col[0];
            arrow[0].color = col[0];
            arrow[0].gameObject.SetActive(true);
        }
        else if(benefit==0)
        {
            text_name.color = Color.red;
        }
        else if(benefit<0)
        {
            text_name.color = col[1];
            arrow[1].color = col[1];
            arrow[1].gameObject.SetActive(true);
        }

        text_name.text = type + ": " + benefit;


    }

    public void DestroyAll()
    {
        Destroy(gameObject);
    }
}
