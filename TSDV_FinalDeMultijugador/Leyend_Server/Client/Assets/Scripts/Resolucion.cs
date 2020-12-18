using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resolucion : MonoBehaviour
{
    // Start is called before the first frame update
    public float Resolucion_X;
    public float Resolucion_Y;
    private float OriginalResolution_X;
    private float OriginalResolution_Y;
    public bool fullScreen;
    public bool automaticRescaledResolution;
    void Start()
    {
        OriginalResolution_X = Resolucion_X;
        OriginalResolution_Y = Resolucion_Y;

        Screen.SetResolution((int)Resolucion_X, (int)Resolucion_Y, fullScreen);
        if (automaticRescaledResolution)
        {
            if (Screen.width < Resolucion_X && Screen.height < Resolucion_Y)
            {
                Resolucion_X = Resolucion_X / 1.2f;
                Resolucion_Y = Resolucion_Y / 1.2f;
                if (Screen.width < Resolucion_X && Screen.height < Resolucion_Y)
                {
                    Resolucion_X = OriginalResolution_X / 1.5f;
                    Resolucion_Y = OriginalResolution_Y / 1.5f;
                    if (Screen.width < Resolucion_X && Screen.height < Resolucion_Y)
                    {
                        Resolucion_X = OriginalResolution_X / 2f;
                        Resolucion_Y = OriginalResolution_Y / 2f;
                    }
                }
                Screen.SetResolution((int)Resolucion_X, (int)Resolucion_Y, fullScreen);
            }
        }
        //Debug.Log(Screen.width + " "Screen.height);
    }
    
}
