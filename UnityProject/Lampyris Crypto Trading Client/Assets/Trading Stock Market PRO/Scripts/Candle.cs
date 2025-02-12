using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Instanciates two gameobjects with images: one for the max-min values and one for the
/// open-close values.
/// </summary>
public class Candle : MonoBehaviour
{
    [Header("Color of the images")]
    public Color myColor;
    [Header("This allows us to know which data is used selected")]
    public int dataIndex;

    [Header("Stored data")]
    public float x_max;
    public float x_min;
    public float y_max;
    public float y_min;
    public float y_opn;
    public float y_cls;    
    public float x_value;
    public float y_value;

    [Header("Stored objects")]
    public GameObject goMaxMin;
    public GameObject goOpenClose;

    [Header("Prefab used for instancing")]
    public GameObject prefab;

    [Header("Colors")]
    public Color colUP;
    public Color colDOWN;

    float coef;

    /// <summary>
    /// Enables/disables the gameobjects representing the candle
    /// </summary>
    /// <param name="b">true=>enabled, false=>disabled</param>
    public void SetActiveGo(bool b)
    {
        goMaxMin.SetActive(b);
        goOpenClose.SetActive(b);

    }

     /// <summary>
     /// Creates the candle with the initial data
     /// </summary>
     /// <param name="indx"> Data index </param>
     /// <param name="xmax"> X maximum value </param>
     /// <param name="xmin"> X minimum value </param>
     /// <param name="ymax"> Y maximum value </param>
     /// <param name="ymin"> Y minimum value</param>
     /// <param name="yop"> Y open value </param>
     /// <param name="ycls"> Y close value </param>
     /// <param name="xvalue"> X actual value</param>
     /// <param name="yvalue"> Y actual value</param>
    public void CreateCandle(int indx, float xmax, float xmin, float ymax, float ymin, float yop, float ycls, float xvalue, float yvalue)
    {
        // Dump values to candle
        dataIndex = indx;

        x_max = xmax;
        x_min = xmin;

        y_max = ymax;
        y_min = ymin;
        y_opn = yop;
        y_cls = ycls;

        x_value = xvalue;
        y_value = yvalue;

        //create the two images
        goOpenClose = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), transform);
        goMaxMin = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), transform);

        //call this function to recalculate the candle
        UpdatePosition();

    }

    /// <summary>
    /// Recalculates the parameters of the candle and its geometry
    /// </summary>
    public void UpdatePosition()
    {        

        //get maximum point
        Vector3 vMax = new Vector3(x_value * Chart.instance.tf_FactorA_X + Chart.instance.tf_FactorB_X - Chart.instance.a / 2, y_max * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2, 0);

        //get the minimum point
        Vector3 vmin = new Vector3(x_value * Chart.instance.tf_FactorA_X + Chart.instance.tf_FactorB_X - Chart.instance.a / 2, y_min * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2, 0);

        //get the open point
        Vector3 vop = new Vector3(x_value * Chart.instance.tf_FactorA_X + Chart.instance.tf_FactorB_X - Chart.instance.a / 2, y_opn * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2, 0);

        //get the close point
        Vector3 vcl = new Vector3(x_value * Chart.instance.tf_FactorA_X + Chart.instance.tf_FactorB_X - Chart.instance.a / 2, y_cls * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2, 0);


        //set the text to the limits
        goMaxMin.transform.GetChild(2).GetComponent<Text>().text = "$" + Mathf.Round(y_max);
        goMaxMin.transform.GetChild(3).GetComponent<Text>().text = "$" + Mathf.Round(y_min);
        
        //this is for placement
        goMaxMin.transform.SetParent(transform);
        goMaxMin.transform.localScale = new Vector3(1, 1, 1);

        // local position in function of the obtained vectors
        Vector3 dir = (vMax - vmin) / 2;
        goMaxMin.transform.localPosition = vmin + dir;
        goMaxMin.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2 * dir.magnitude, Chart.instance.L_width * Chart.instance.zoomCoef);
        goMaxMin.transform.right = dir;

                
        //set correct direction and color in case close-open have a specific sign
        if (y_cls >= y_opn)
        {

            goOpenClose.transform.SetParent(transform);
            goOpenClose.transform.localScale = new Vector3(1, 1, 1);

            dir = (vcl - vop) / 2;

            goOpenClose.transform.localPosition = vop + dir;
            goOpenClose.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2 * dir.magnitude, Chart.instance.M_width*Chart.instance.zoomCoef );
            goOpenClose.transform.right = dir;


            //set the text to the limits
            goOpenClose.transform.GetChild(2).GetComponent<Text>().text = "$" + Mathf.Round(y_cls);
            goOpenClose.transform.GetChild(3).GetComponent<Text>().text = "$" + Mathf.Round(y_opn);


            myColor = colUP;

            goOpenClose.transform.GetComponent<Image>().color = colUP;
            goMaxMin.transform.GetComponent<Image>().color = colUP;

        }
        else   //invert direction
        {

            goOpenClose.transform.SetParent(transform);
            goOpenClose.transform.localScale = new Vector3(1, 1, 1);

            dir = (vop - vcl) / 2;

            goOpenClose.transform.localPosition = vcl + dir;
            goOpenClose.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2 * dir.magnitude, Chart.instance.M_width * Chart.instance.zoomCoef);
            goOpenClose.transform.right = dir;


            //set the text to the limits
            goOpenClose.transform.GetChild(2).GetComponent<Text>().text = "$" + Mathf.Round(y_opn);
            goOpenClose.transform.GetChild(3).GetComponent<Text>().text = "$" + Mathf.Round(y_cls);

            myColor = colDOWN;

            goOpenClose.transform.GetComponent<Image>().color = colDOWN;
            goMaxMin.transform.GetComponent<Image>().color = colDOWN;
        }

        // disable objects if there are outside the chart
        if(x_value<Chart.instance.xmin)
        {
            SetActiveGo(false);
        }
        else
        {
            SetActiveGo(true);
        }
    }


    /// <summary>
    /// Recalculates the candle with the current prize setting the limits again for ymax and ymin
    /// </summary>
    /// <param name="prize"></param>
    public void UpdateCurrentPrize(float prize)
    {
        y_cls = prize;

        //update max and min
        if(prize>=y_max)
        {
            y_max = prize;
        }

        if(prize<=y_min)
        {
            y_min = prize;
        }


        UpdatePosition();
    }

    /// <summary>
    /// Destroys all the candle
    /// </summary>
    public void DestroyAll()
    {
        Destroy(goMaxMin);
        Destroy(goOpenClose);
        Destroy(gameObject);

    }

}




