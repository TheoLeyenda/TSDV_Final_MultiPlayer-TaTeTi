using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DisplayDataPlayers : MonoBehaviour
{
    public GameObject camvasDisplay;
    public TextMeshProUGUI textNamePlayerOne;
    public TextMeshProUGUI textNamePlayerTwo;
    public string namePlayer1;
    public string namePlayer2;
    public bool namePlayerOneAssigned;
    public bool namePlayerTwoAssigned;
    public int myTeam;
    public bool allAssigned = false; 
    public static DisplayDataPlayers instanceDisplayDataPlayers;
    void Awake()
    {
        camvasDisplay.SetActive(false);
        if (instanceDisplayDataPlayers == null)
            instanceDisplayDataPlayers = this;
        else
            Destroy(gameObject);

        textNamePlayerOne.text = "Equipo ¿? = Esperando jugador...";
        textNamePlayerTwo.text = "Equipo ¿? = Esperando jugador...";
    }
    void Update()
    {
        if (namePlayerOneAssigned && namePlayerTwoAssigned && !allAssigned && (myTeam == (int)Ficha.TeamFicha.O || myTeam == (int)Ficha.TeamFicha.X))
        {
            Debug.Log("ENTRE");
            allAssigned = true;
            Debug.Log(myTeam);
            if (myTeam == (int)Ficha.TeamFicha.O)
            {
                textNamePlayerOne.text = "Equipo O = " + namePlayer1;
                textNamePlayerTwo.text = "Equipo X = " + namePlayer2;
                Debug.Log("ENTRE O");
            }
            else if(myTeam == (int)Ficha.TeamFicha.X)
            {
                textNamePlayerOne.text = "Equipo X = " + namePlayer1;
                textNamePlayerTwo.text = "Equipo O = " + namePlayer2;
                Debug.Log("ENTRE X");
            }
        }
    }
}
