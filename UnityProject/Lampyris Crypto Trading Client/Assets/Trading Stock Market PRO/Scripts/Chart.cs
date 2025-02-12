using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Simulation and url are different types of data
///     sim--> it is the app which generates the data
///     url--> gets real data from a php webpage
/// </summary>
public enum DataSource { simulation, url }

/// <summary>
/// candles are vertical images composed by ymax-ymin yopen-yclose
/// while polygon is a line that displays the y_value
/// </summary>
public enum PlotMode { candles, polygon }

/// <summary>
/// SMA it is the mean value for a given period
/// </summary>
public enum Indicator { none,SMA};

/// <summary>
/// Main script that generates the chart
/// </summary>
public class Chart : MonoBehaviour
{

    //singleton
    public static Chart instance;
    public int dataCurrentSize;

    [Header("Chart physical parameters")]
    public Transform[] corner;
    public int widthTeo, heightTeo;

    [Header("Select type of data")]
    public string url;
    public DataSource dataType;
    public int totalDataTypes=2;
    public int selectedData=0;

    Coroutine corr;
    bool isReady = false;

    [Header("Zoom and display zone")]
    public int ticksToShow = 30;
    public float zoomCoef=1;

    //public initial capital and benefit
    [Header("Capital for trading")]
    public float capital;
    public float initialCapital = 1000;
    Text capital_txt;
    Text benefit_txt;


    [Header("Drawing Style")]
    public PlotMode plotMode;
    public int ticksPerCandle=10;
    public GameObject candlePrefab;
    public GameObject linePrefab;
    public GameObject polygonPrefab;
  

    [Header("Indicators")]
    public int SMAPoints=20;
    public Indicator selectedIndicator;
    public float y_cut=100;


    [Header("Line parameters")]
    // line parameters
    [Range(0, 15)]
    public float L_width = 5f;

    [Range(0, 15)]
    public float M_width = 5f;

    [Range(2, 10)]
    public int nb__div = 5;

    [Range(0, 10)]
    public int HL_width = 1;

    [Range(0, 500)]
    public int nb_initial_Tick = 200;

    // line prefab for UP and DOWN behaviour
    public GameObject prefab_UP, prefab_DOWN, prefab_HORIZ, prefab_VERT;

    //time counting event 
    float elapsed = 100000, elapsed2=100000;

    //these two arrays contain the values for the line chart
    List<float>[] x_value;
    List<float>[] y_lastValue;
    List<float>[] y_mean;

    List<Candle>[] candles;
    List<Line>[] meanInd;
    List<Polygon>[] polygons;

    //maximum values for x and y
    public float xmax, ymax, xmin, ymin;

    [Header("Tick latency and duration")]
    //tick_latency [s]
    public float tick_latency = 1;
    //tick_duration [s]
    public float tick_duration = 10;

    //this is the value of the market at the instant given
    [Header("The actual value in the market")]
    public float[] y_value;

    //this is the initial conditions for the simulation
    [Header("Simulation values")]
    public float min_seed_value = 100;
    public float max_seed_value = 200;
    public float vol = 10;
    //fork
    public float fork = 1;

    //this is the horizontal line
    Transform lineH;

    //these are the variables used to change the size of the render of the chart
    [Header("Public factors")]
    public float tf_FactorA_Y;
    public float tf_FactorB_Y;
    public float tf_FactorA_X;
    public float tf_FactorB_X;
    public float tf_Factor;
    public float a;
    public float b;

    // colors for the markers
    [Header("Colors for the markers")]
    public Color colUP, colDOWN;
    

    //variables for trading
    Text buy_txt, sell_txt;
    Button buy_but, sell_but, close_but;


    // selling and buying values
    float set_buy = 0;
    float set_sel = 0;
    List<float> y_BuySell;

    //this is the trading state
    //    0--> none,  1-->buy  2-->sell
    [Header("Trading 0-->none, 1-->buy, 2-->sell")]
    public List<int> tradingState;
    List<Position> positions;
    public float balanceBenefit;

    // line horizontal for trading
    public GameObject tradingLine;

    //volume value
    public InputField input_Volume_txt;
    int val_volume;
    GameObject line_container;
    GameObject Hline_container;
    GameObject[] horiz_line;
    GameObject[] vert_line;

    [Header("Menus")]
    public Transform menu_Data_type;
    public GameObject menu_Trade;
    public GameObject menu_Historic;
    public GameObject prefab_UI_dataType;
    public GameObject positionPrefab;
    public GameObject historicPrefab;
         

    #region INITIAL DATA GENERATION
    //generates the initial data of the market
    public void Generate_initial_data()
    {

        y_value = new float[totalDataTypes];


        //set menu data info
        for (int ii=0;ii< totalDataTypes; ii++)
        {
            GameObject go = GameObject.Instantiate(prefab_UI_dataType,menu_Data_type);

           
            int a = ii;
            go.GetComponent<Button>().onClick.AddListener(delegate () 
                { SelectDataType(a);
                    //menu_Data_type.gameObject.SetActive(false);
                });
            go.transform.GetChild(0).GetComponent<Text>().text = Enum.GetNames(dataType.GetType())[ii];
        }



        // get containers for the lines and the markers
        line_container = GameObject.Find("lines");
        Hline_container = GameObject.Find("Hlines");


        //we use float values
        x_value = new List<float>[totalDataTypes];
        y_lastValue = new List<float>[totalDataTypes];
        y_mean = new List<float>[totalDataTypes];

        candles = new List<Candle>[totalDataTypes];
        meanInd = new List<Line>[totalDataTypes];
        polygons = new List<Polygon>[totalDataTypes];

        for (int ii=0;ii<totalDataTypes;ii++)
        {
            x_value[ii] = new List<float>();
            y_lastValue[ii] = new List<float>();
            y_mean[ii] = new List<float>();


            candles[ii] = new List<Candle>();
            meanInd[ii] = new List<Line>();
            polygons[ii] = new List<Polygon>();
        }


        float ym = UnityEngine.Random.Range(min_seed_value, max_seed_value);

        //maximum limits
        xmax = -100000000;
        ymax = -100000000;

        xmin = 100000000;
        ymin = 100000000;

        if (dataType==DataSource.simulation)
        {
            for (int i = 0; i < nb_initial_Tick; i++)
            {
                //obtaining values of the finantial behaviour
                ym = ym + UnityEngine.Random.Range(-vol, vol);

                y_value[selectedData] = ym;
                x_value[selectedData].Add(i);
                y_lastValue[selectedData].Add(ym);

                xmax = Mathf.Max(xmax, x_value[selectedData][i]);
                ymax = Mathf.Max(ymax, y_lastValue[selectedData][i]);

                xmin = Mathf.Min(xmin, x_value[selectedData][i]);
                ymin = Mathf.Min(ymin, y_lastValue[selectedData][i]);

            }

        }
        else if(DataSource.url==dataType)
        {

            corr=StartCoroutine(DataFromServer_Co());
            
        }

        //benefit arrows
        benefit_txt.transform.GetChild(0).GetComponent<Image>().color = colUP;
        benefit_txt.transform.GetChild(0).gameObject.SetActive(false);
        benefit_txt.transform.GetChild(1).GetComponent<Image>().color = colDOWN;
        benefit_txt.transform.GetChild(1).gameObject.SetActive(false);

        if (plotMode == PlotMode.candles)
        {
            RecalculateCandles(false);
        }
        else if (plotMode == PlotMode.polygon)
        {
            RecalculatePolygons(false);
        }

        RecalculateMean(false);


        //initialize cut
        y_cut = (ymax + ymin) / 2;
    }

    #endregion


    #region UNITY FUNCTIONS
    // in the start function the data is loaded and the chart is drawn
    void Start()
    {
        instance = this;


        tradingState = new List<int>();
        y_BuySell = new List<float>();
        positions = new List<Position>();


        //get trading variables
        buy_txt = GameObject.Find("buy_txt").GetComponent<Text>();
        sell_txt = GameObject.Find("sell_txt").GetComponent<Text>();
        buy_but = GameObject.Find("button_buy_but").GetComponent<Button>();
        sell_but = GameObject.Find("button_sell_but").GetComponent<Button>();
        
        //get horizontal line
        lineH = GameObject.Find("positioningLineH").transform;
        tradingLine.GetComponent<Image>().enabled = false ;
        //initialcapital
        capital = initialCapital;
        capital_txt = GameObject.Find("capital_txt").GetComponent<Text>();
        UpdateCaptital(0);
        benefit_txt = GameObject.Find("benefit_txt").GetComponent<Text>();
        //generate data

        Generate_initial_data();

        //change ticks of the subtitle
        //GameObject.FindGameObjectWithTag("subtitle").GetComponent<Text>().text = "" + nb_initial_Ticks + " ticks";

        //get initial volume
        lineH.gameObject.SetActive(false);
        horiz_line = new GameObject[nb__div + 1];

        for (int j = 0; j <= nb__div; j++)
        {
            horiz_line[j] = GameObject.Instantiate(prefab_HORIZ, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            horiz_line[j].transform.SetParent(transform);
            horiz_line[j].transform.localScale = new Vector3(1, 1, 1);
        }

        vert_line = new GameObject[nb__div + 1];
        for (int j = 0; j <= nb__div; j++)
        {
            vert_line[j] = GameObject.Instantiate(prefab_VERT, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            vert_line[j].transform.SetParent(transform);
            vert_line[j].transform.localScale = new Vector3(1, 1, 1);
        }


        ChangeVolume();
        Replot();

        GameObject.FindGameObjectWithTag("title").GetComponent<Text>().text = "Cardboard Buddies: " + dataType.ToString();

    }

    
    public void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        elapsed2 += Time.fixedDeltaTime;

        if (elapsed > tick_latency)
        {

            elapsed = 0;

            lineH.gameObject.SetActive(true);

            UpdateTradingValues();



            //////////////////////////////
            //**SIMULATION EVENTS **//
            // ///////////////////////////

            if (dataType == DataSource.simulation)
            {
                x_value[selectedData].Add(x_value[selectedData][x_value[selectedData].Count - 1] + 1);
                y_value[selectedData] = y_value[selectedData] + UnityEngine.Random.Range(-vol / 5, vol / 5);
                y_lastValue[selectedData].Add(y_value[selectedData]);

                dataCurrentSize = x_value[selectedData].Count;
            
                if (plotMode == PlotMode.polygon)
                {
                    PlotChartParam();
                
                    RecalculatePolygons(true);
                    
                    if (selectedIndicator == Indicator.SMA)
                    {
                        RecalculateMean(false);
                    }
                }
            }

            //////////////////////////////
            //**php data  EVENTS **//
            // ///////////////////////////
            else if (dataType == DataSource.url)
            {
                if (corr == null)
                {
                    corr = StartCoroutine(DataFromServer_Co());
                }
            }

       
            //replot the chart
            Replot();

           
            //position of the line that show the value of the market at that moment
            lineH.localPosition = new Vector3(0, y_value[selectedData] * tf_FactorA_Y + tf_FactorB_Y - b / 2, 0);
            lineH.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(a, 3);
            lineH.SetSiblingIndex(transform.childCount - 1);

            //set value of the y_value
            lineH.transform.GetChild(1).GetComponent<Text>().text = "$" + Mathf.Round(y_value[selectedData]);
            //lineH.transform.forward = transform.forward;


            if (plotMode == PlotMode.candles)
            {
                if (candles[selectedData].Count > 0)
                {
                    //set same color
                    lineH.transform.GetChild(0).GetComponent<Image>().color = candles[selectedData][candles[selectedData].Count - 1].myColor;

                    candles[selectedData][candles[selectedData].Count - 1].UpdateCurrentPrize(y_value[selectedData]);

                }
            }



        }
    }
    #endregion

    #region DRAWING FUNCTIONS

    /// <summary>
    /// Changes the indicator  SMA/MACD
    /// </summary>
    /// <param name="tg">sets the toogle active or deactive</param>
    public void ChangeIndicator(Toggle tg)
    {
        if (tg.isOn)
        {
            selectedIndicator = Indicator.SMA;
            RecalculateMean(false);
        }
        else
        {
            selectedIndicator = Indicator.none;
            for (int ii = 0; ii < meanInd[selectedData].Count; ii++)
            {
                meanInd[selectedData][ii].DestroyAll();
            }
            meanInd[selectedData].Clear();
        }
    }

    /// <summary>
    /// this function is called when a variable is changed in the inspector 
    /// </summary>
    public void Replot()
    {

        if (plotMode == PlotMode.candles)
        {
            // only when a new candle must be created, recalculate the candles
            if ((x_value[selectedData].Count % ticksPerCandle)==1)
            {
                Debug.Log("Add new candle");
                RecalculateCandles(false);
                PlotChartParam();

                if (selectedIndicator == Indicator.SMA)
                {
                    RecalculateMean(false);
                }

            }
        
            //PlotChartParam();

            UpdateAllCandles();
           
        }
        


    }

    /// <summary>
    /// Changes the size of the candle
    /// </summary>
    /// <param name="dp">the dropdown in the canvas with the associated value </param>
    public void ChangeCandleSize(Dropdown dp)
    {
        if (plotMode == PlotMode.candles)
        {
            ticksPerCandle = int.Parse(dp.options[dp.value].text);

            PlotChartParam();
            RecalculateCandles(false);

            if (selectedIndicator == Indicator.SMA)
            {
                PlotChartParam();
                RecalculateMean(false);
            }

        }
    }

    /// <summary>
    /// Changes the MACD/SMA parameters
    /// </summary>
    /// <param name="field">the associated value</param>
    public void ChangeMACD(InputField field)
    {
        SMAPoints=int.Parse(field.text);

        if(SMAPoints<5)
        {
            SMAPoints = 5;
            field.text = ""+5;
        }

        if (selectedIndicator == Indicator.SMA)
        {
            PlotChartParam();
            RecalculateMean(false);
        }
    }

    /// <summary>
    /// Changes the y_cut
    /// </summary>
    /// <param name="field">the associated value</param>
    public void ChangeCut(InputField field)
    {
        y_cut = float.Parse(field.text);        
    }

    /// <summary>
    /// Updates all candle positions and geometry
    /// </summary>
    public void UpdateAllCandles()
    {
        //update candles
        for (int i = 0; i < candles[selectedData].Count; i++)
        {
           candles[selectedData][i].UpdatePosition();           
        }
    }

    /// <summary>
    /// Recalculates the polygons
    /// </summary>
    /// <param name="last"> if last=true, only updates last value </param>
    public void RecalculatePolygons(bool last)
    {
        //destroy all polygons
        if (last == false)
        {
            for (int ii = 0; ii < polygons[selectedData].Count; ii++)
            {
                polygons[selectedData][ii].DestroyAll();
            }
            polygons[selectedData].Clear();
        }

        
        int start = Math.Max((int)(x_value[selectedData].Count-ticksToShow), 0);
        int end = (int)(x_value[selectedData].Count-1);

        int counter = 0;

        for (int polyIndex = start; polyIndex < end; polyIndex++)
        {
            //limits for the data
            float xleft = x_value[selectedData][polyIndex];
            float xright = x_value[selectedData][polyIndex+1];

            float yleft = y_lastValue[selectedData][polyIndex];
            float yright = y_lastValue[selectedData][polyIndex+1];

            if (last == false)
            {
                GameObject tempPolygon = GameObject.Instantiate(polygonPrefab, transform);
                polygons[selectedData].Add(tempPolygon.GetComponent<Polygon>());
                polygons[selectedData][polygons[selectedData].Count - 1].CreatePolygon(selectedData, xleft, xright, yleft, yright);
            }
            else
            {
                polygons[selectedData][counter].CreatePolygon(selectedData, xleft, xright, yleft, yright);
                counter++;
            }
            
          
            
        }
    }

    /// <summary>
    /// Recalculate mean values
    /// </summary>
    /// <param name="last">if it is true, only update last</param>
    public void RecalculateMean(bool last)
    {
        //destroy all mean objects
        if (last == false)
        {
            for (int ii = 0; ii < meanInd[selectedData].Count; ii++)
            {
                meanInd[selectedData][ii].DestroyAll();
            }
            meanInd[selectedData].Clear();
        }

        int start = 0;
        int end = 0;

        start =SMAPoints;
        end = x_value[selectedData].Count;


        y_mean[selectedData].Clear();

        for (int lineIndex = start; lineIndex < end; lineIndex++)
        {
            //mean values
            float ymeanTemp = 0;
            
            //get min max values
            for (int i = Mathf.Max(lineIndex - SMAPoints, 0);  i <  lineIndex; i++)
            {
                ymeanTemp = ymeanTemp + y_lastValue[selectedData][i];

            }
            y_mean[selectedData].Add(ymeanTemp / SMAPoints);
                       
        }
   
        for (int lineIndex = 0; lineIndex < y_mean[selectedData].Count-1; lineIndex++)
        {
            if (last == false)
            {
                GameObject tempLineSMA = GameObject.Instantiate(linePrefab, transform);
                meanInd[selectedData].Add(tempLineSMA.GetComponent<Line>());
                meanInd[selectedData][meanInd[selectedData].Count - 1].CreateLine(selectedData, x_value[selectedData][lineIndex+SMAPoints], x_value[selectedData][lineIndex + SMAPoints + 1], y_mean[selectedData][lineIndex], y_mean[selectedData][lineIndex + 1]);
            }
            else
            {
                if (lineIndex< meanInd[selectedData].Count)
                {
                    meanInd[selectedData][lineIndex].CreateLine(selectedData, x_value[selectedData][lineIndex + SMAPoints], x_value[selectedData][lineIndex + SMAPoints + 1], y_mean[selectedData][lineIndex], y_mean[selectedData][lineIndex + 1]);
                }
            }
        }

    }

    /// <summary>
    /// recalculates all candles
    /// </summary>
    /// <param name="last">if is true, only update last </param>
    public void RecalculateCandles(bool last)
    {
        //destroy all candles in this case
        if (last == false)
        {
            for (int ii = 0; ii < candles[selectedData].Count; ii++)
            {
                candles[selectedData][ii].DestroyAll();
            }
            candles[selectedData].Clear();
        }

        int start = 0;
        int end = 0;

        if (last)
        {
            start = Math.Max((int)(x_value[selectedData].Count / ticksPerCandle-1) ,0);
            end = (int)(x_value[selectedData].Count / ticksPerCandle);
        }
        else
        {
            start = 0;
            end = (int)(x_value[selectedData].Count / ticksPerCandle);
        }


        int counter = 0;

        for (int candleIndex =start; candleIndex < end; candleIndex++)
        {
            //limits for the data
            float xmin_candle = x_value[selectedData][candleIndex * ticksPerCandle];
            float xmax_candle = x_value[selectedData][(candleIndex + 1) * ticksPerCandle - 1];

            float yopen_candle = y_lastValue[selectedData][candleIndex * ticksPerCandle];
            float yclose_candle = y_lastValue[selectedData][(candleIndex + 1) * ticksPerCandle-1];


            float ymax_candle = -10000000;
            float ymin_candle = 10000000;

            //get min max values
            for (int i = candleIndex*ticksPerCandle; i < (candleIndex+1) * ticksPerCandle; i++)
            {
                ymax_candle = Mathf.Max(ymax_candle, y_lastValue[selectedData][i]);
                ymin_candle = Mathf.Min(ymin_candle, y_lastValue[selectedData][i]);
            }

            
            if (last == false)
            {
                GameObject tempCandle = GameObject.Instantiate(candlePrefab, transform);
                candles[selectedData].Add(tempCandle.GetComponent<Candle>());
                candles[selectedData][candles[selectedData].Count - 1].CreateCandle(selectedData, xmax_candle, xmin_candle, ymax_candle, ymin_candle, yopen_candle, yclose_candle, xmax_candle, yclose_candle);

            }
            else
            {
                candles[selectedData][counter].CreateCandle(selectedData, xmax_candle, xmin_candle, yclose_candle, yclose_candle, yclose_candle, yclose_candle, xmax_candle, yclose_candle);
                counter++;
            }
                                 
        }




    }



    public void DeleteAll()
    {
        //candles
        for (int ii = 0; ii < candles[selectedData].Count; ii++)
        {
            candles[selectedData][ii].DestroyAll();
        }
        candles[selectedData].Clear();

        //MACD
        for (int ii = 0; ii < meanInd[selectedData].Count; ii++)
        {
            meanInd[selectedData][ii].DestroyAll();
        }
        meanInd[selectedData].Clear();
        
        //polygons
        for (int ii = 0; ii < polygons[selectedData].Count; ii++)
        {
            polygons[selectedData][ii].DestroyAll();
        }
        polygons[selectedData].Clear();
    


    }

    /// <summary>
    /// changes the ticks to show
    /// </summary>
    /// <param name="value">new zoom param</param>
    public void ChangeZoom(int value)
    {
        if (plotMode == PlotMode.candles)
        {
            ticksToShow += value * ticksPerCandle;


            if (ticksToShow >= candles[selectedData].Count)
            {
                ticksToShow = candles[selectedData].Count;
            }

            if (ticksToShow <= 5)
            {
                ticksToShow = 5;
            }
        }
        else if(plotMode == PlotMode.polygon)
        {
            ticksToShow += value * 10;

            if (ticksToShow >= x_value[selectedData].Count)
            {
                ticksToShow = x_value[selectedData].Count;
            }

            if (ticksToShow <= 25)
            {
                ticksToShow = 25;
            }
        }

        PlotChartParam();

   
        if (selectedIndicator == Indicator.SMA)
        {
            RecalculateMean(false);
        }
                
        
        if (plotMode == PlotMode.candles)
        {
            RecalculateCandles(false);
        }
        else if (plotMode == PlotMode.polygon)
        {
            RecalculatePolygons(false);
        }

          

    }


    /// <summary>
    /// This goes from candles to polygons and vice-versa
    /// </summary>
    public void ChangeCandlesPolygon()
    {
        if(plotMode==PlotMode.candles)
        {
            for (int ii = 0; ii < candles[selectedData].Count; ii++)
            {
                candles[selectedData][ii].DestroyAll();
            }
            candles[selectedData].Clear();

            plotMode = PlotMode.polygon;
            RecalculatePolygons(false);


        }
        else if(plotMode == PlotMode.polygon)
        {

            for (int ii = 0; ii < polygons[selectedData].Count; ii++)
            {
                polygons[selectedData][ii].DestroyAll();
            }
            polygons[selectedData].Clear();


            plotMode = PlotMode.candles;
            RecalculateCandles(false);


        }


    }

    /// <summary>
    /// draws the chart and obtains the chart basic geometrical parameters a,b,Factors_i
    /// </summary>
    public void PlotChartParam()
    {

        //graphic parameters not valid if not centered
        //Vector2 sizeChart = transform.GetComponent<RectTransform>().sizeDelta;

        //size of the chart in two components

        //new limits for the chart
        xmax = -1000000000;
        xmin = 1000000000;
        ymax = -1000000000;
        ymin = 1000000000;


        if (plotMode == PlotMode.candles)
        {
            for (int i = Mathf.Max(0, candles[selectedData].Count - 1 - ticksToShow); i < candles[selectedData].Count; i++)
            {
                xmax = Mathf.Max(xmax, candles[selectedData][i].x_max);
                ymax = Mathf.Max(ymax, candles[selectedData][i].y_max);

                xmin = Mathf.Min(xmin, candles[selectedData][i].x_min);
                ymin = Mathf.Min(ymin, candles[selectedData][i].y_min);
            }
        }
        else if(plotMode==PlotMode.polygon)
        {
            for (int i = Mathf.Max(0, polygons[selectedData].Count - 1 - ticksToShow); i < polygons[selectedData].Count; i++)
            {
                xmax = Mathf.Max(xmax, polygons[selectedData][i].x_right)+1;
                ymax = Mathf.Max(ymax, polygons[selectedData][i].y_left);

                xmin = Mathf.Min(xmin, polygons[selectedData][i].x_left);
                ymin = Mathf.Min(ymin, polygons[selectedData][i].y_right);
            }
        }

        //singularities
        if (xmax == xmin)
        {
            xmax = xmin + 0.01f;
        }

        if (ymax == ymin)
        {
            ymax = ymin + 0.01f;
        }

        a = corner[1].position.x -corner[0].position.x;
        b = corner[1].position.y - corner[0].position.y;

        //adapt to screensize
        tf_Factor = (float)Camera.main.pixelWidth / widthTeo;
        a = a / (tf_Factor+0.01f);
        b = b / (tf_Factor+0.01f);


        //a = a / (float)Camera.main.pixelWidth * widthTeo;
        //b = b / (float)Camera.main.pixelHeight * heightTeo;

        //Debug.Log("a=" + a + " b=" + b);

        tf_FactorA_X = a / (xmax - xmin);
        tf_FactorB_X = -xmin / (xmax - xmin) * a;



        tf_FactorA_Y = b / (ymax - ymin);
        tf_FactorB_Y = -ymin / (ymax - ymin) * b;

        
        // create horizontal lines
        for (int j = 0; j <= nb__div; j++)
        {
            //horiz_line[j] = GameObject.Instantiate(prefab_HORIZ, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            //horiz_line[j].transform.SetParent(Hline_container.transform);

            float y_HLvalue = (float)j / nb__div * (ymax - ymin) + ymin;

            horiz_line[j].transform.localPosition = new Vector3(0, (y_HLvalue) * tf_FactorA_Y + tf_FactorB_Y - b / 2, 0);

            horiz_line[j].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(a, HL_width);
            horiz_line[j].transform.right = transform.right;

            // set text with the value
            horiz_line[j].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = "$" + Mathf.Round(y_HLvalue);

        }

        // create vertical lines
        for (int j = 0; j <= nb__div; j++)
        {
            float xValueMN = (float)j / nb__div * (xmax - xmin) + xmin;

            vert_line[j].transform.localPosition = new Vector3(xValueMN * tf_FactorA_X +tf_FactorB_X - Chart.instance.a / 2, 0, 0);

            vert_line[j].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(HL_width,b);
            
            // set text with the value
            vert_line[j].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text =""+ DateTime.Now.AddSeconds(xValueMN-xmax).ToString("HH:mm");

        }


        zoomCoef = a / ticksToShow/25;
    }


    #endregion


    #region TRADING FUNCTIONS

    //////////////////////////////
    // trading operations
    // ///////////////////////////

    /// <summary>
    /// Adds capital to the game
    /// </summary>
    /// <param name="inpt">the amount</param>
    public void AddFunds(InputField inpt)
    {
        UpdateCaptital(float.Parse(inpt.text));
    }

    /// <summary>
    /// calculates the total benefit of the positions
    /// </summary>
    /// <returns>total benefit</returns>
    public float CalculateTotalBenefit()
    {
        float benf = 0;

        for (int ii = 0; ii < y_BuySell.Count; ii++)
        {
            if (tradingState[ii] == 1)
            {
                benf += (y_value[selectedData] - y_BuySell[ii]);
            }
            else if (tradingState[ii] == -1)
            {
                benf -= (y_value[selectedData] - y_BuySell[ii]);

            }
           
        }

        return benf;
    }

    /// <summary>
    /// calculates only partial benefit
    /// </summary>
    /// <param name="ii">the index of the position</param>
    /// <returns>partial benefit</returns>
    public float CalculatePartialBenefit(int ii)
    {
        float benf = 0;

        if (tradingState[ii] == 1)
        {
            benf += (y_value[selectedData] - y_BuySell[ii]);
        }
        else if (tradingState[ii] == -1)
        {
            benf -= (y_value[selectedData] - y_BuySell[ii]);

        }

   
        return benf;
    }

    /// <summary>
    /// Updates the text, geometry and the color of the total positions
    /// </summary>
    public void UpdateTradingValues()
    {

        //forks
        set_buy = y_value[selectedData] + fork;
        set_sel = y_value[selectedData] - fork;

        buy_txt.text = "$" + Mathf.Round((set_buy) * 100) / 100;
        sell_txt.text = "$" + Mathf.Round((set_sel) * 100) / 100;


        // benefit
        balanceBenefit = Mathf.Round((float)val_volume * (CalculateTotalBenefit()) * 100) / 100;
        benefit_txt.text = "$" + balanceBenefit;

        if (balanceBenefit > 0)
        {
            benefit_txt.color = colUP;
            benefit_txt.transform.GetChild(0).gameObject.SetActive(true);
            benefit_txt.transform.GetChild(1).gameObject.SetActive(false);
        }
        else if(balanceBenefit < 0)
        {
            benefit_txt.color = colDOWN;
            benefit_txt.transform.GetChild(0).gameObject.SetActive(false);
            benefit_txt.transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (balanceBenefit == 0)
        {
            benefit_txt.text = "$" + 0;
            benefit_txt.color = Color.black;

            //benefit arrows
            benefit_txt.transform.GetChild(0).gameObject.SetActive(false);
            benefit_txt.transform.GetChild(1).gameObject.SetActive(false);
        }

        if (y_BuySell.Count > 0)
        {
            tradingLine.GetComponent<Image>().enabled = true;

            tradingLine.transform.localPosition = new Vector3(0, y_BuySell[y_BuySell.Count - 1] * tf_FactorA_Y + tf_FactorB_Y - b / 2, 0);
            tradingLine.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(a, 30);
        }
        else
        {
            tradingLine.GetComponent<Image>().enabled = false;
        }



        //update trade menu
        for (int ii = 0; ii < positions.Count; ii++)
        {
            positions[ii].UpdateValues(Mathf.Round((float)val_volume*CalculatePartialBenefit(ii)*100)/100);
        }

    }

   
    /// <summary>
    /// changes the operating volume
    /// </summary>
    public void ChangeVolume()
    {
        val_volume = int.Parse(input_Volume_txt.text);
    }

    /// <summary>
    /// opens a buy position
    /// </summary>
    public void PerformBuy()
    {

        tradingState.Add(1);
        y_BuySell.Add(set_buy);

        GameObject go=GameObject.Instantiate(positionPrefab,menu_Trade.transform);
        positions.Add(go.GetComponent<Position>());
        positions[positions.Count-1].type="BUY";
        positions[positions.Count - 1].index = positions.Count - 1;

        go.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate ()
        {
            GameObject thisGo = go ;
            CloseTransaction(go.GetComponent<Position>().index);

        });


    }

    /// <summary>
    /// opens a sell position
    /// </summary>
    public void PerformSell()
    {        

        tradingState.Add(-1);
        y_BuySell.Add(set_sel);

        GameObject go = GameObject.Instantiate(positionPrefab, menu_Trade.transform);
        positions.Add(go.GetComponent<Position>());
        positions[positions.Count - 1].type = "SELL";
        positions[positions.Count - 1].index = positions.Count - 1;


        go.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate ()
        {
            int a = positions.Count-1;
            CloseTransaction(a);

        });



    }

    /// <summary>
    /// closes all positions
    /// </summary>
    public void CloseTransactions()
    {
        
        y_BuySell.Clear();
        tradingState.Clear();

        for(int ii=0;ii<positions.Count;ii++)
        {
            GameObject go = GameObject.Instantiate(historicPrefab, menu_Historic.transform);
            Position hist = go.GetComponent<Position>();
            hist.type = positions[ii].type;
            hist.UpdateValues(positions[ii]._benefit);
                                    

            positions[ii].DestroyAll();
        }
        positions.Clear();

        UpdateCaptital(balanceBenefit);

    }

    
    /// <summary>
    /// closes only selected position 
    /// </summary>
    /// <param name="i">position index</param>
    public void CloseTransaction(int i)
    {

        float partialBenefit = Mathf.Round((float)val_volume * CalculatePartialBenefit(i) * 100) / 100;

        GameObject go = GameObject.Instantiate(historicPrefab, menu_Historic.transform);
        Position hist = go.GetComponent<Position>();
        hist.type = positions[i].type;
        hist.UpdateValues(positions[i]._benefit);


        y_BuySell.RemoveAt(i);
        tradingState.RemoveAt(i);
        positions[i].DestroyAll();
        positions.RemoveAt(i);

        for (int ii = 0; ii < positions.Count; ii++)
        {
            positions[ii].index =ii;
        }
        UpdateCaptital(balanceBenefit);

    }

    void UpdateCaptital(float a)
    {
        capital += a;
        capital_txt.text = "$" + Mathf.Round(capital * 100) / 100;

    }

    #endregion

       
    #region SERVER DATA

    public IEnumerator DataFromServer_Co()
    {
        //call to the server
        WWW w = new WWW(url);

        

        //if it is not done, do nothing
        while (!w.isDone)
        {
            yield return w;
        }

        //if it is done, use the text and parse to json format
        string urlText = w.text;

        //Debug.Log("here");
        if (urlText != null && urlText != "")
        {
            RealTimeData data = JsonUtility.FromJson<RealTimeData>(urlText);

            y_value[selectedData] = float.Parse(data.last, CultureInfoCustom.instance.ci);
            y_lastValue[selectedData].Add(y_value[selectedData]);

            if (x_value[selectedData].Count>0)
            {
                x_value[selectedData].Add(x_value[selectedData][x_value[selectedData].Count - 1] + 1);
            }
            else
            {
                x_value[selectedData].Add(0);
            }

            Debug.Log("ReadingData price="+data.last);
            isReady = true;
        }

        corr = null;

    }

    public class RealTimeData
    {
        public string high;
        public string last;
        public string timestamp;
        public string bid;
        public string wap;
        public string volume;
        public string low;
        public string ask;
        public string open;

    }
   
    //from time to date
    private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
        return dtDateTime;
    }

    #endregion

    #region DATA_TYPE
    public void SelectDataType(int a)
    {
        StopAllCoroutines();


        DeleteAll();

        selectedData = a;

        
        if(selectedData==0)
        {
            dataType = DataSource.simulation;

        }
        else if(selectedData==1)
        {
            dataType = DataSource.url;
            corr = StartCoroutine(DataFromServer_Co());
        }

        //disable all
        for (int j = 0; j < totalDataTypes; j++)
        {
            for (int i = 0; i < x_value[j].Count; i++)
            {
                /* EDIT
                goMaxMin[j][i].SetActive(false);
                goOpenClose[j][i].SetActive(false);
                */
            }
        }


        GameObject.FindGameObjectWithTag("title").GetComponent<Text>().text = "Cardboard Buddies: " +dataType.ToString();

    }

 

    #endregion
}