﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Game : NetworkBehaviour
{
    public GameObject[] cards;
    private int indexCards;
    private GameObject[] playerCards;
    private GameObject[,] othersCards;
    private int[] indexOthersCards;

    public GameObject reversCard;

    private GameObject[] cardsSpawned = new GameObject[54];
    private int numPlayer;

    [SyncVar]
    private int playerTurn;

    private int numCardsForPlayer;
    private int numPlayerCards;

    private SyncListInt lastGame = new SyncListInt();
    private GameObject[] lastCards = new GameObject[8];
    private int indexLastCards;
    private int numPlayers;

    private float distanceLastCard;
    private bool[] playersHavePassedTurn, playersHaveFinished;
    private int lastPlayerGame;

    private String[] nameOfPlayers;
    private int position;

    public Text textPlayerTurnName, textNumYouEnded, textYouEnded, passedTextUp, passedTextDown, passedTextRight, passedTextLeft;
    public Text playerDownName, playerRightName, playerUpName, playerLeftName;
    private Text[] passedTextPlayers;

    [SyncVar]
    private bool startedGame;

    // Use this for initialization
    void Start () {
        numPlayer = GameObject.FindGameObjectsWithTag("Player").Length - 1;
        distanceLastCard = 50;
        indexCards = 0;
        indexLastCards = 0;
        playersHavePassedTurn = new bool[4];
        passedTextPlayers = new Text[4];
        nameOfPlayers = new String[4];
        playersHaveFinished = new bool[4];
        startedGame = false;

        for (int i = 0; i <4; i++)
        {
            playersHavePassedTurn[i] = false;
            playersHaveFinished[i] = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer) {
            if (Input.GetKeyDown(KeyCode.Space) && GameObject.FindGameObjectsWithTag("Player").Length > 1 && !startedGame)
            {
                Debug.Log("RETURN PRESSED");
                BeginGame();
                startedGame = true;
            }
        }
	}

    private void BeginGame()
    {
        int index;
        GameObject aux;
        for (int i = 0; i < 54; i++)
        {
            cardsSpawned[i] = Instantiate(cards[i], transform.position, cards[i].transform.rotation);
            NetworkServer.Spawn(cardsSpawned[i]);
        }

        for (int i = 0; i < 54; i++)
        {
            index = UnityEngine.Random.Range(0, 54);
            aux = cardsSpawned[i];
            cardsSpawned[i] = cardsSpawned[index];
            cardsSpawned[index] = aux;
        }
        for (int i = 0; i < 54; i++)
        {
            RpcAddCard(cardsSpawned[i]);
        }

        numPlayers = GameObject.FindGameObjectsWithTag("Player").Length;

        numCardsForPlayer = 54 / numPlayers;
        playerTurn = UnityEngine.Random.Range(0, numPlayers);
        RpcChangePlayerTurnName(Player_control.getplayersName()[playerTurn]);
        lastGame.Add(-1);
        lastGame.Add(-1);

        position = 1;

        RpcSendNameOfPlayers(Player_control.getplayersName());

        RpcDealCards(numCardsForPlayer, numPlayers);
        
    }

    [ClientRpc]
    private void RpcSendNameOfPlayers(String[] nameOfPlayers)
    {
        for (int i=0; i<4; i++)
        {
            if (nameOfPlayers[i] != null)
            {
                this.nameOfPlayers[i] = nameOfPlayers[i];
            }
        }
    }

    [ClientRpc]
    private void RpcAddCard(GameObject card)
    {
        cardsSpawned[indexCards] = card;
        //Debug.Log(numPlayer + " " + cardsSpawned[indexCards]);
        indexCards++;
    }

    [ClientRpc]
    private void RpcDealCards(int numCardsForPlayer, int numPlayers)
    {
        //Debug.Log("\nDEAL CARDS");
        int count = 0;
        playerCards = new GameObject[numCardsForPlayer];
        numPlayerCards = numCardsForPlayer;
        //Debug.Log(numPlayer + " " + numCardsForPlayer);
        this.numCardsForPlayer = numCardsForPlayer;
        for (int i=0; i<54; i++)
        {
            //Debug.Log(numPlayer + ": " + cardsSpawned[i]);
            if (i >= (numPlayer * numCardsForPlayer) && i < (numPlayer * numCardsForPlayer) + numCardsForPlayer)
            {
                //Debug.Log(i);
                playerCards[count] = cardsSpawned[i];
                count++;
            }
            else
            {
                cardsSpawned[i].SetActive(false);
            }
        }

        //order by number (smallest to biggest)
        int smallestCardIndex, smallestCardNumber;
        GameObject aux;
        for (int i = 0; i < numCardsForPlayer; i++)
        {
            smallestCardIndex = i;
            smallestCardNumber = 20;
            for (int j = i; j < numCardsForPlayer; j++) {
                //Debug.Log(smallestCardNumber + " > " + int.Parse(playerCards[j].tag));
                if (smallestCardNumber > int.Parse(playerCards[j].tag))
                {
                    smallestCardIndex = j;
                    smallestCardNumber = int.Parse(playerCards[j].tag);
                }
            }
            aux = playerCards[i];
            playerCards[i] = playerCards[smallestCardIndex];
            playerCards[smallestCardIndex] = aux;
            //Debug.Log(playerCards[i].name);
        }

        positionCards();

        othersCards = new GameObject[numPlayers, numCardsForPlayer];
        indexOthersCards = new int[numPlayers];

        for (int i=0; i<numPlayers; i++)
        {
            indexOthersCards[i] = 0;
        }

        positionOthersCards(numPlayers);
    }

    private void positionCards()
    {
        for (int i=0; i < numCardsForPlayer; i++)
        {
            playerCards[i].transform.position = new Vector3((i*0.45f)-((numCardsForPlayer/2)*0.45f), -4, 50-i);
        }

        for (int i = 0; i < numCardsForPlayer; i++)
        {
            playerCards[i].transform.position = new Vector3((i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), -4, 50 - i);
        }
    }

    private void positionOthersCards(int numPlayers)
    {
        passedTextPlayers[numPlayer] = passedTextDown;
        playerDownName.GetComponent<Text>().text = nameOfPlayers[numPlayer];
        if (numPlayers == 2)
        {
            passedTextPlayers[(numPlayer + 1) % numPlayers] = passedTextUp;
            playerUpName.text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            for (int i = 0; i < numCardsForPlayer; i++)
            {
                othersCards[(numPlayer + 1) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.position = new Vector3((i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 4, 50 - i);
            }
        } else if (numPlayers == 3)
        {
            passedTextPlayers[(numPlayer + 1) % numPlayers] = passedTextRight;
            playerRightName.GetComponent<Text>().text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            passedTextPlayers[(numPlayer + 2) % numPlayers] = passedTextUp;
            playerUpName.GetComponent<Text>().text = nameOfPlayers[(numPlayer + 2) % numPlayers];
            for (int i = 0; i < numCardsForPlayer; i++)
            {
                othersCards[(numPlayer + 1) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.position = new Vector3(5.5f, (i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 50 - i);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.Rotate(new Vector3(0, 0, 90));
                

                othersCards[(numPlayer + 2) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 2) % numPlayers, i].transform.position = new Vector3((i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 4, 50 - i);
            }
        }
        else
        {
            passedTextPlayers[(numPlayer + 1) % numPlayers] = passedTextRight;
            playerRightName.GetComponent<Text>().text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            passedTextPlayers[(numPlayer + 2) % numPlayers] = passedTextUp;
            playerUpName.GetComponent<Text>().text = nameOfPlayers[(numPlayer + 2) % numPlayers];
            passedTextPlayers[(numPlayer + 3) % numPlayers] = passedTextLeft;
            playerLeftName.GetComponent<Text>().text = nameOfPlayers[(numPlayer + 3) % numPlayers];

            for (int i = 0; i < numCardsForPlayer; i++)
            {
                othersCards[(numPlayer + 1) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.position = new Vector3(5.5f, (i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 50 - i);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.Rotate(new Vector3(0, 0, 90));
                
                othersCards[(numPlayer + 2) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 2) % numPlayers, i].transform.position = new Vector3((i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 4, 50 - i);
               
                othersCards[(numPlayer + 3) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 3) % numPlayers, i].transform.position = new Vector3(-4f, (i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 50 - i);
                othersCards[(numPlayer + 3) % numPlayers, i].transform.Rotate(new Vector3(0, 0, -90));
            }
        }
    }

    public bool isCardPosible(int cardNumber)
    {
        if (lastGame[0] == -1) return true;
        else if (cardNumber == 14) return true;
        if (lastGame[0] == 11)
        {
            if (cardNumber >= 10) return true;
            else return false;
        }
        else if (lastGame[0] == 8)
        {
            if (lastGame[0] > cardNumber) return true;
            else return false;
        }
        else if (lastGame[0] == 5)
        {
            if (cardNumber == 5 || cardNumber == 6) return true;
            else return false;
        }
        else
        {
            if (lastGame[0] <= cardNumber) return true;
            else return false;
        }
    }

    public Text GetPassedTextDown()
    {
        return passedTextDown;
    }

    public bool IsMyTurn()
    {
        //Debug.Log(numPlayer + " " +playerTurn);
        if (numPlayer == playerTurn) return true;
        else return false;
    }

    public void throwCard(GameObject[] cards, int cardsNumber, int numPlayerCards, int numPlayer)
    {
        //RpcSynchronizeTable(cards, number);

        indexLastCards = 0;
        for (int i=0; i < cardsNumber; i++)
        {
            cards[i].transform.position = new Vector3((i*0.4f)-(cardsNumber / (2f/0.4f)), i*(-0.1f), distanceLastCard);
            cards[i].transform.Rotate(0, 0, i*20);
            distanceLastCard -= 0.5f;
            RpcSetCardVisible(cards[i]);
            lastCards[indexLastCards] = cards[i];
            indexLastCards++;
        }
        int numberPlayersPassed = 0;
        if(int.Parse(cards[0].tag) == lastGame[0])
        {
            numberPlayersPassed = cardsNumber;
        } else if (int.Parse(cards[0].tag) == 14)
        {
            numberPlayersPassed = 4;
        }
        lastGame.Insert(0, int.Parse(cards[0].tag));
        lastGame.Insert(1, cardsNumber);
        lastPlayerGame = playerTurn;

        Debug.Log(numPlayerCards);
        if (numPlayerCards <= 0)
        {
            playersHaveFinished[numPlayer] = true;
            RpcPlayerHasFinished(numPlayer, position, playersHaveFinished);
            position++;
        }

        RpcEliminateOtherCards(cardsNumber, playerTurn);
        
        changeTurn(numberPlayersPassed);
    }

    [ClientRpc]
    public void RpcEliminateOtherCards(int cardsNumber, int playerTurn)
    {
        for(int i=0; i<cardsNumber; i++)
        {
            if (indexOthersCards[playerTurn] % 2 == 0)
            {
                Destroy(othersCards[playerTurn, indexOthersCards[playerTurn]/2]);
            }
            else
            {
                Destroy(othersCards[playerTurn, (numCardsForPlayer-1) - (indexOthersCards[playerTurn]/2)]);
            }
            indexOthersCards[playerTurn]++;
        }
    }

    [ClientRpc]
    public void RpcPlayerHasFinished(int numPlayer, int position, bool[] playersHaveFinished)
    {
        if (this.numPlayer == numPlayer)
        {
            textNumYouEnded.GetComponent<Text>().text = "#" + position;
            textYouEnded.gameObject.SetActive(true);
        }
        else
        {
            bool lastPlayer = true;
            for (int i=0; i<numPlayers; i++)
            {
                if (i != numPlayer)
                {
                    if (!playersHaveFinished[i]) lastPlayer = false;
                }
            }
            if (lastPlayer)
            {
                textNumYouEnded.GetComponent<Text>().text = "#" + numPlayers;
                textYouEnded.gameObject.SetActive(true);
            }
        }
    }

    [ClientRpc]
    public void RpcSetCardVisible(GameObject card)
    {
        StartCoroutine("SetCardVisible", card);
        card.GetComponent<BoxCollider>().enabled = false;
    }

    private IEnumerator SetCardVisible(GameObject card)
    {
        yield return new WaitForSeconds(.3f);
        card.SetActive(true);
    }

    private void changeTurn(int numberPlayersPassed)
    {
        bool playerDisponible = false;
        int i = 0;

        while (i < numPlayers && !playerDisponible) {
            playerTurn++;
            if (playerTurn == numPlayers) playerTurn = 0;

            if (playerTurn == lastPlayerGame) playerDisponible = true;
            if (numberPlayersPassed > 0) numberPlayersPassed--;
            else if (!playersHavePassedTurn[playerTurn] && !playersHaveFinished[playerTurn]) playerDisponible = true;

            i++;
        }
        if (playerTurn == lastPlayerGame) playerDisponible = false;
        Debug.Log("PLAYER DISPONIBLE: " + playerDisponible);
        if (!playerDisponible)
        {
            StartCoroutine("ResetTable");

            playerTurn = lastPlayerGame;
            
            lastGame.Insert(0, -1);
            lastGame.Insert(1, -1);
        }
    }

    private IEnumerator ResetTable()
    {
        yield return new WaitForSeconds(2f);
        int i;
        for (i = 0; i < indexLastCards; i++)
        {
            lastCards[i].transform.Rotate(new Vector3(180, 0, 0));
        }

        for (i = 0; i < numPlayers; i++)
        {
            playersHavePassedTurn[i] = false;
        }
        indexLastCards = 0;
        RpcDisablePassedText();
        RpcChangePlayerTurnName(Player_control.getplayersName()[playerTurn]);
    }

    [ClientRpc]
    private void RpcDisablePassedText()
    {
        for (int i=0; i<4; i++)
        {
            if (passedTextPlayers[i] != null)
            {
                passedTextPlayers[i].gameObject.SetActive(false);
            }
        }
    }

    [ClientRpc]
    private void RpcChangePlayerTurnName(string playerTurnName)
    {
        textPlayerTurnName.GetComponent<Text>().text = playerTurnName;
    }

    public void ButtonPassTurn()
    {
        Debug.Log("Button pass turn");
        if (lastGame[0] != -1)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                player.GetComponent<Player>().PassTurn(numPlayer, playerTurn);
            }
        }
    }

    public void PlayerPass(int numPlayer)
    {
        playersHavePassedTurn[numPlayer] = true;
        RpcPlayerHasPassed(numPlayer);
        changeTurn(0);
    }

    [ClientRpc]
    private void RpcPlayerHasPassed(int numPlayer)
    {
        Debug.Log("AQUIII" + numPlayer);
        passedTextPlayers[numPlayer].gameObject.SetActive(true);
    }

    public bool lessNumberOfCards(int number)
    {
        //Debug.Log(number + " " + lastGame[1]);
        if (lastGame[0] == -1) return true;
        else if (number <= lastGame[1]) return true;
        else return false;
    }

    public bool sameNumberOfCards(int number)
    {
        if (lastGame[1] == -1) return true;
        else if (number == lastGame[1]) return true;
        else return false;
    }

    public GameObject[] GetPlayerCards()
    {
        return playerCards;
    }

    public int GetNumPlayer()
    {
        return numPlayer;
    }

    public int GetNumPlayerCards()
    {
        return numPlayerCards;
    }

    public int GetPlayerTurn()
    {
        return playerTurn;
    }

    public bool GetStartedGame()
    {
        return startedGame;
    }

    public void EliminatePlayerCards(GameObject[] cardsChoosed, int numberChoosed)
    {
        for (int indexCard = 0; indexCard < numberChoosed; indexCard++)
        {
            for (int i = 0; i < numPlayerCards; i++)
            {
                Debug.Log(playerCards[i].name);
            }
            bool found = false;
            for (int i = 0; i < numPlayerCards; i++)
            {
                Debug.Log(i+" - "+ cardsChoosed[indexCard]+" - "+ playerCards[i]);
                if (!found && cardsChoosed[indexCard].Equals(playerCards[i]))
                {
                    found = true;
                }
                if (found && i < (numPlayerCards - 1))
                {
                    playerCards[i] = playerCards[i + 1];
                }
            }
            numPlayerCards--;
            for (int i = 0; i < numPlayerCards; i++)
            {
                Debug.Log(playerCards[i].name);
            }
        }
        Debug.Log("Length player cards: "+playerCards.Length+" "+ numPlayerCards);
    }
}