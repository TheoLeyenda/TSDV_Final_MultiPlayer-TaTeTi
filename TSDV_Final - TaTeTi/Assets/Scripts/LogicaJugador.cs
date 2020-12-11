using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
public class LogicaJugador : MonoBehaviour
{

    public GameObject fichaX;
    public GameObject fichaO;
    public GameObject miFicha { set; get; }

    public Ficha ficha { set; get; }

    private bool isMyTurn = true;

    private DataPlayer dataPlayer;

    void Start()
    {
        ficha = new Ficha();
        dataPlayer = GetComponent<DataPlayer>();
    }
    void Update()
    {
        if (miFicha == null)
            return;
        if (Input.GetMouseButton(0) && isMyTurn)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform == null && hit.transform.tag != "Slot")
                {
                    return;
                }

                Slot slot = hit.transform.gameObject.GetComponent<Slot>();
                if (slot == null)
                    return;

                if (slot.ocupado)
                    return;


                slot.fichaSlot = PhotonNetwork.Instantiate("Ficha", slot.transform.position, Quaternion.identity).GetComponent<Ficha>();
                slot.fichaSlot.myPlayer = this;
                slot.ocupado = true;

            }
        }
    }
    private void OnEnable()
    {
        Ficha.OnAlive += SettingDataFicha;
    }
    private void OnDisable()
    {
        Ficha.OnAlive -= SettingDataFicha;
    }
    public void SettingDataFicha(Ficha ficha)
    {
        
        if (ficha.myPlayer != this)
        {
            if (miFicha == fichaO)
            {
                //Debug.Log("HOLA PUTA");
                ficha.meshRenderer.material = ficha.matTeamX;
                ficha.teamFicha = Ficha.TeamFicha.X;
            }
            else if (miFicha == fichaX)
            {
                //Debug.Log("HOLA PUTA");
                ficha.meshRenderer.material = ficha.matTeamO;
                ficha.teamFicha = Ficha.TeamFicha.O;
            }
        }
        else if (ficha.myPlayer == this)
        {
            if (miFicha == fichaO)
            {
                //Debug.Log("HOLA PUTA");
                ficha.meshRenderer.material = ficha.matTeamO;
                ficha.teamFicha = Ficha.TeamFicha.O;
            }
            else if (miFicha == fichaX)
            {
                //Debug.Log("HOLA PUTA");
                ficha.meshRenderer.material = ficha.matTeamX;
                ficha.teamFicha = Ficha.TeamFicha.X;
            }
        }
    }
    public void SeleccionarFichaX()
    {
        miFicha = fichaX;
        ficha.MyGameObject = miFicha;
        ficha.teamFicha = Ficha.TeamFicha.X;
        dataPlayer.team = (int)Ficha.TeamFicha.X;
        //dataPlayer.SetDisplayPlayer();
    }
    public void SeleccionarFichaO()
    {
        miFicha = fichaO;
        ficha.MyGameObject = miFicha;
        ficha.teamFicha = Ficha.TeamFicha.O;
        dataPlayer.team = (int)Ficha.TeamFicha.O;
        //dataPlayer.SetDisplayPlayer();
    }
}
