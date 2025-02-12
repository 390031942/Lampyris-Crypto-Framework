using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// this is used to prevent problems with the conversion of "." and "," in PC due to region.
/// </summary>
public class CultureInfoCustom : MonoBehaviour
{
    public static CultureInfoCustom instance;
    public CultureInfo ci;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //set culture info --> prevent problems between "," and "." for floats
        ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        ci.NumberFormat.NumberDecimalSeparator = ".";

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
