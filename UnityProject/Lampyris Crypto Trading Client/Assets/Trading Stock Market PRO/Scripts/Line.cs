using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Line is used to display the mean value in the chart for example
/// </summary>
public class Line : MonoBehaviour
{
    [Header("The object to instantiate")]
    public GameObject linePrefab;
    [Header("Public line gameobject")]
    public GameObject line;

    [Header("Line data")]
    public float x_left;
    public float x_right;
    public float y_left;
    public float y_right;
    public int indexData;

    /// <summary>
    /// Create the line only if it does not exist, otherwhise only update values
    /// </summary>
    /// <param name="indx"> Data index </param>
    /// <param name="xleft"> X value on the left side of the line</param>
    /// <param name="xright"> X value on the right side of the line </param>
    /// <param name="yleft"> y value on the left side of the line</param>
    /// <param name="yright"> y value on the right side of the line</param>
    public void CreateLine(int indx, float xleft, float xright, float yleft, float yright)
    {
                
        if (line == null)
        {
            line = GameObject.Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), transform);
        }

        //dump values
        indexData = indx;
        x_left = xleft;
        x_right = xright;
        y_left = yleft;
        y_right = yright;


        //get maximum point
        Vector3 vLeft = new Vector3(x_left * Chart.instance.tf_FactorA_X + Chart.instance.tf_FactorB_X - Chart.instance.a / 2, y_left * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2, 0);

        //get the minimum point
        Vector3 vRight = new Vector3(x_right * Chart.instance.tf_FactorA_X + Chart.instance.tf_FactorB_X - Chart.instance.a / 2, y_right * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2, 0);

        Vector3 dir = vRight - vLeft;
        float distance = (dir).magnitude;

        //positionning
        line.transform.localPosition = vLeft + dir.normalized * distance / 2;
        line.GetComponent<RectTransform>().sizeDelta = new Vector2(dir.magnitude, Chart.instance.L_width);// Chart.instance.L_width * Chart.instance.zoomCoef);
        line.transform.right = dir;

        //do not show if it is out of the chart
        if (x_left < Chart.instance.xmin)
        {
            SetActiveGo(false);
        }
        else
        {
            SetActiveGo(true);
        }

    }

     /// <summary>
     /// Enables/disbales gameobject
     /// </summary>
     /// <param name="b">b=true=> enable  b=false=>disable </param>
    public void SetActiveGo(bool b)
    {
        line.SetActive(b);
        
    }

    
    public void DestroyAll()
    {
        Destroy(line);
        Destroy(gameObject);

    }


}
