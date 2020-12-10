using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RedesDeJugador : MonoBehaviour
{
    // Start is called before the first frame update
    public MonoBehaviour[] codigoQueIgnorar;

    private PhotonView photonView;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        //Verifica si el photonView actual no es el mio.
        if (!photonView.IsMine)
        {
            foreach (var codigo in codigoQueIgnorar)
            {
                codigo.enabled = false;
            }
        }
    }

    public bool MyPhotonView()
    {
        return photonView.IsMine;
    }
}
