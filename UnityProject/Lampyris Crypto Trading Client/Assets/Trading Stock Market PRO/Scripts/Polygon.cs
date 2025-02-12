using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a polygon (triangles) and lines for the normal line drawing
/// </summary>
public class Polygon : Graphic
{
   
    [Header("Prefabs used")]
    public GameObject linePrefab;
    public GameObject line;
    public GameObject polygon;

    [Header("Colors used")]
    public Color colDOWN;
    public Color colUP;

    [Header("Polygon data")]
    public float x_left;
    public float x_right;
    public float y_left;
    public float y_right;
    public int indexData;

    Vector3 vLeft, vRight;

    /// <summary>
    /// Creates or updates the values of the polygon
    /// </summary>
    /// <param name="indx">Data index</param>
    /// <param name="xleft">X value of the left side of the line/polygon</param>
    /// <param name="xright">X value of the right side of the line/polygon</param>
    /// <param name="yleft">Y value of the left side of the line/polygon</param>
    /// <param name="yright">Y value of the right side of the line/polygon</param>
    public void CreatePolygon(int indx,float xleft, float xright, float yleft, float yright )
    {
        if (line == null)
        {
            line = GameObject.Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), transform);
        }

        //dump data
        indexData = indx;
        x_left = xleft;
        x_right = xright;
        y_left = yleft;
        y_right = yright;


        //get maximum point
        vLeft = new Vector3(x_left * Chart.instance.tf_FactorA_X + Chart.instance.tf_FactorB_X - Chart.instance.a / 2, y_left * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2, 0);

        //get the minimum point
        vRight = new Vector3(x_right * Chart.instance.tf_FactorA_X + Chart.instance.tf_FactorB_X - Chart.instance.a / 2, y_right * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2, 0);

        //obtain direction
        Vector3 dir = vRight - vLeft;
        float distance = (dir).magnitude;

        //positionning
        line.transform.localPosition = vLeft + dir.normalized * distance / 2;
        line.GetComponent<RectTransform>().sizeDelta = new Vector2(dir.magnitude, Chart.instance.L_width * Chart.instance.zoomCoef);
        line.transform.right = dir;

        //call the onpopulatemesh function
        SetVerticesDirty();


    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {

        //draw polygons in function of the cut value on the char (down or up colors)
        // draw trapezoid
        if (y_left > Chart.instance.y_cut && y_right > Chart.instance.y_cut)
        {
            DrawPolygon(vh,colUP);
        }
        else if (y_left < Chart.instance.y_cut && y_right < Chart.instance.y_cut)
        {
            DrawPolygon(vh,colDOWN);
        }
        //draw two triangles only
        else 
        {
            if(y_left >= Chart.instance.y_cut && y_right <= Chart.instance.y_cut)
            {
                DrawTriangle(vh, colUP, colDOWN, +1);                
            }
            else if(y_left <= Chart.instance.y_cut && y_right >= Chart.instance.y_cut)
            {
                DrawTriangle(vh, colDOWN,colUP,-1);
            }


        }


    }
    

    /// <summary>
    /// Draws a trapezoid using 4 points (with two triangles)
    /// </summary>
    /// <param name="vh">the vertex helper</param>
    /// <param name="col">color of the vertex</param>
    public void DrawPolygon(VertexHelper vh, Color col)
    {
        if(vh==null)
        {
            return;
        }

        Vector2[] corners = new Vector2[4];

        //set corbers
        corners[0] = vLeft;
        corners[1] = vRight;
        corners[2] = new Vector2(vRight.x, Chart.instance.y_cut * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2);
        corners[3] = new Vector2(vLeft.x, Chart.instance.y_cut * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2);


        vh.Clear();


        //you can use this to check geometry
        //col = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f), 1);
        
        //set verticies
        for (int ii = 0; ii < corners.Length; ii++)
        {
            vh.AddVert(corners[ii], col, new Vector2(0f, 0f));

        }

        //set triangles
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);

 
    }

    /// <summary>
    /// Draws two triangles at when the line cuts the y_cut value.
    /// </summary>
    /// <param name="vh">vertex helper</param>
    /// <param name="colL">color left part</param>
    /// <param name="colR">color right part</param>
    /// <param name="sens"> used t know if clockwise of not</param>
    public void DrawTriangle(VertexHelper vh, Color colL, Color colR, int sens)
    {
        if (vh == null)
        {
            return;
        }

        // obtain interpolationt 
        float m = (vRight.y - vLeft.y) / (vRight.x - vLeft.x);
        float n = vLeft.y - m * vLeft.x;

        float v_cut = Chart.instance.y_cut * Chart.instance.tf_FactorA_Y + Chart.instance.tf_FactorB_Y - Chart.instance.b / 2;
        float v_interpolated = (v_cut - n) / m + 1;

        Vector2[] corners = new Vector2[6];

        //set the corners
        corners[0] = new Vector2(vLeft.x, v_cut);
        corners[1] = vLeft;
        corners[2] = new Vector2(v_interpolated, v_cut);
        corners[3] = new Vector2(v_interpolated, v_cut);
        corners[4] = vRight;
        corners[5] = new Vector2(vRight.x, v_cut);


        vh.Clear();

        //create vertices
        if (sens == 1)
        {
            vh.AddVert(corners[0], colL, new Vector2(0f, 0f));
            vh.AddVert(corners[1], colL, new Vector2(0f, 0f));
            vh.AddVert(corners[2], colL, new Vector2(0f, 0f));
            vh.AddVert(corners[3], colR, new Vector2(0f, 0f));
            vh.AddVert(corners[4], colR, new Vector2(0f, 0f));
            vh.AddVert(corners[5], colR, new Vector2(0f, 0f));
        }

        else if (sens == -1)
        {
            vh.AddVert(corners[0], colL, new Vector2(0f, 0f));
            vh.AddVert(corners[1], colL, new Vector2(0f, 0f));
            vh.AddVert(corners[2], colL, new Vector2(0f, 0f));
            vh.AddVert(corners[3], colR, new Vector2(0f, 0f));
            vh.AddVert(corners[4], colR, new Vector2(0f, 0f));
            vh.AddVert(corners[5], colR, new Vector2(0f, 0f));
        }

        //create triangles
        vh.AddTriangle(0, 2, 1);
        vh.AddTriangle(3, 5, 4);
    
       

    }


    public void DestroyAll()
    {
        Destroy(line);
        Destroy(gameObject);

    }

  

}