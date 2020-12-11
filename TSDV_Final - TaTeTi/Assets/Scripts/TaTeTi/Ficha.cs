using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Ficha : MonoBehaviour
{
    public GameObject MyGameObject;
    public TeamFicha teamFicha;
    public static event Action<Ficha> OnInstanceFicha;
    private bool instaceEvent = false;
    public static event Action<Ficha>OnAlive;
    public Material matTeamX;
    public Material matTeamO;
    public LogicaJugador myPlayer { set; get; }
    public MeshRenderer meshRenderer { set; get; }
    public enum TeamFicha
    {
        O = 1,
        X = 2,
    }
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if(OnAlive != null)
            OnAlive(this);
    }
}
