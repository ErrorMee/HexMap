using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherControl : MonoBehaviour
{
    public Transform snow;
    public Transform rain;


    void Start()
    {
        
    }

    public void ToggleSnow(bool isSelect)
    {
        snow.gameObject.SetActive(isSelect);
    }

    public void ToggleRain(bool isSelect)
    {
        rain.gameObject.SetActive(isSelect);
    }
}
