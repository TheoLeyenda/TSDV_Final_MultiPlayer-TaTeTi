using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicaJugador : MonoBehaviour
{

    public GameObject fichaX;
    public GameObject fichaO;
    public GameObject miFicha { set; get; }
    public GameObject suFicha { set; get; }

    public Ficha ficha { set; get; }

    void Start()
    {
        ficha = new Ficha();
    }
    void Update()
    {
        if (miFicha == null)
            return;
        if (Input.GetMouseButton(0))
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

                slot.fichaSlot = Instantiate(ficha.MyGameObject, slot.transform).GetComponent<Ficha>();
                slot.ocupado = true;


            }
        }
    }

    public void SeleccionarFichaX()
    {
        miFicha = fichaX;
        suFicha = fichaO;
        ficha.MyGameObject = miFicha;
        ficha.teamFicha = Ficha.TeamFicha.X;
    }
    public void SeleccionarFichaO()
    {
        miFicha = fichaO;
        suFicha = fichaX;
        ficha.MyGameObject = miFicha;
        ficha.teamFicha = Ficha.TeamFicha.O;
    }
}
