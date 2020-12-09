using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    // Start is called before the first frame update

    public bool ocupado { set; get; } 
    public Ficha fichaSlot { set; get; }

    private void Start()
    {
        ocupado = false;
    }
}
