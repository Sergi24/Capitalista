using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Card : NetworkBehaviour
{
    private bool selected;

	// Use this for initialization
	void Start () {
        selected = false;
    }

    private void SelectIfSelectionIsPosible()
    {
        bool isPosibleToChoose = false;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<Player>().isPosibleToChoose(int.Parse(gameObject.tag))) isPosibleToChoose = true;
        }
        if (!selected && isPosibleToChoose && GameObject.Find("Game").GetComponent<Game>().IsCardPosible(int.Parse(tag)) && GameObject.Find("Game").GetComponent<Game>().IsMyTurn())
        {

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in players)
            {
                player.GetComponent<Player>().setCardChoosed(gameObject);
            }

        }
        else if (selected)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject player in players)
            {
                player.GetComponent<Player>().unSetCardChoosed(gameObject);
            }
        }
    }

    void OnMouseDown()
    {
        SelectIfSelectionIsPosible();
    }

    void OnMouseEnter()
    {
        if (Input.GetKey(KeyCode.Mouse0) && !selected)
        { 
            SelectIfSelectionIsPosible();
        }
    }

    public void SetSelected(bool selected)
    {
        this.selected=selected;
    }

    public bool GetSelected()
    {
        return selected;
    }
}
