using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    private GameObject[] cardsChoosed;
    private int numberChoosed;
    Game game;

    // Use this for initialization
    void Start () {
        cardsChoosed = new GameObject[8];
        numberChoosed = 0;
        game = GameObject.Find("Game").GetComponent<Game>();
        if (!isLocalPlayer)
        {
            gameObject.GetComponentInChildren<AudioListener>().enabled = false;
            gameObject.GetComponentInChildren<Camera>().enabled = false;
        }

        if (isLocalPlayer) CmdAddPlayer(Player_control.playerLocalName, GameObject.FindGameObjectsWithTag("Player").Length-1);

    }

    public void PlaySelectedCards()
    {
        if (isLocalPlayer && game.GetStartedGame())
        {
            if ((numberChoosed > 0) && (game.sameNumberOfCards(numberChoosed) || (numberChoosed == 1 && int.Parse(cardsChoosed[0].tag) == 14)) && game.IsMyTurn())
            {
                CmdThrowCard(cardsChoosed, numberChoosed, game.GetNumPlayerCards() - numberChoosed, game.GetNumPlayer());

                game.EliminatePlayerCards(cardsChoosed, numberChoosed);

                numberChoosed = 0;
            }
        }
    }

    [Command]
    private void CmdAddPlayer(string playerLocalName, int numPlayer)
    {
        Player_control.addPlayerName(playerLocalName, numPlayer);
        Debug.Log(Player_control.getplayersName()[numPlayer]);
    }

    [Command]
    public void CmdThrowCard(GameObject[] cardsChoosed, int numberChoosed, int numPlayerCards, int numPlayer)
    {
        game.ThrowCard(cardsChoosed, numberChoosed, numPlayerCards, numPlayer);
    }

    public bool isPosibleToChoose(int cardNumber)
    {
        if (isLocalPlayer)
        {
            //Debug.Log(numberChoosed+1 + " " + game.lessNumberOfCards(numberChoosed + 1));
            if (numberChoosed == 0) return true;
            else if (game.lessNumberOfCards(numberChoosed+1))
            {
                if ((int.Parse(cardsChoosed[0].tag) == 10 || int.Parse(cardsChoosed[0].tag) == 11) && (cardNumber == 10 || cardNumber == 11)) return true;
                else if (int.Parse(cardsChoosed[0].tag) == cardNumber) return true;
                else return false;
            }
            else return false;
        }
        else return false;
    }

    public void setCardChoosed(GameObject card)
    {
        if (isLocalPlayer)
        {
            cardsChoosed[numberChoosed] = card;
            card.transform.position = new Vector3(card.transform.position.x, card.transform.position.y + 0.3f, card.transform.position.z);
            card.GetComponent<Card>().SetSelected(true);
            numberChoosed++;
        }
    }

    public void unSetCardChoosed(GameObject card)
    {
        if (isLocalPlayer)
        {
            bool trobat = false;
            for (int i = 0; i < numberChoosed; i++)
            {
                if (!trobat && card.Equals(cardsChoosed[i]))
                {
                    trobat = true;
                }
                if (trobat)
                {
                    cardsChoosed[i] = cardsChoosed[i + 1];
                }
            }
            card.transform.position = new Vector3(card.transform.position.x, card.transform.position.y - 0.3f, card.transform.position.z);
            card.GetComponent<Card>().SetSelected(false);
            numberChoosed--;
        }
    }

    public void PassTurn(int numPlayer, int playerTurn)
    {
        if (isLocalPlayer)
        {
            if (numPlayer == playerTurn)
            {
                CmdPassTurn(numPlayer);

                for (int i=0; i<game.GetNumPlayerCards();  i++)
                {
                    if (game.GetPlayerCards()[i].GetComponent<Card>().GetSelected())
                    {
                        unSetCardChoosed(game.GetPlayerCards()[i]);
                    }
                }
                numberChoosed = 0;
                game.GetPassedTextDown().gameObject.SetActive(true);
            }
        }
    }

    [Command]
    public void CmdPassTurn(int numPlayer)
    {
        Debug.Log("playersHavePassedTurn[" + numPlayer + "] = true");
        game.PlayerPass(numPlayer);
    }

    public void PlayerPrepared(int numPlayer)
    {
        if (isLocalPlayer)
        {
            CmdPlayerPrepared(numPlayer);
        }
    }

    [Command]
    public void CmdPlayerPrepared(int numPlayer)
    {
        game.AddPlayerPrepared(numPlayer);
    }
}
