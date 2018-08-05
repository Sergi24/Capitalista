using System;
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

    private GameObject[] cardsSpawned;
    private int numPlayer;

    [SyncVar]
    private int playerTurn;

    private int numCardsForPlayer;
    private int numPlayerCards;

    private SyncListInt lastGame = new SyncListInt();
    private GameObject[] lastCards;
    private int indexLastCards;
    private int numPlayers;

    private float distanceLastCard;
    private bool[] playersHavePassedTurn, playersHaveFinished;
    private int lastPlayerGame;

    private String[] nameOfPlayers;
    private int position;

    public Text textNumYouEnded, textYouEnded, passedTextUp, passedTextDown, passedTextRight, passedTextLeft;
    public Text playerDownName, playerRightName, playerUpName, playerLeftName, textNumPlayers, textSended;
    public GameObject puntuations;
    public Text[] textNamesPuntuationPlayers, puntuationPlayers;
    private Text[] passedTextPlayers, textPlayersName;

    public GameObject spaceToBegin;

    public Color red, orange;

    public GameObject buttonNextGame;

    [SyncVar]
    private bool available;

    [SyncVar]
    private bool startedGame;

    // Use this for initialization
    void Start() {
        numPlayer = GameObject.FindGameObjectsWithTag("Player").Length - 1;
        
        startedGame = false;
        numPlayers = 1;

        if (isServer)
        {
            spaceToBegin.SetActive(true);
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
                spaceToBegin.SetActive(false);
                RpcDisableTextNumPlayers();
            }
        }
        if (!startedGame)
        {
            textNumPlayers.gameObject.SetActive(true);
            textNumPlayers.GetComponent<Text>().text = "Number of players in the room: " + GameObject.FindGameObjectsWithTag("Player").Length;
        }
    }

    public void Initialize()
    {
        distanceLastCard = 50;
        indexCards = 0;
        indexLastCards = 0;
        playersHavePassedTurn = new bool[4];
        passedTextPlayers = new Text[4];
        textPlayersName = new Text[4];
        nameOfPlayers = new String[4];
        playersHaveFinished = new bool[4];
        available = true;

        cardsSpawned = new GameObject[54];

        lastCards = new GameObject[8];

        textYouEnded.gameObject.SetActive(false);
        buttonNextGame.SetActive(false);
        textSended.gameObject.SetActive(false);
        puntuations.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            playersHavePassedTurn[i] = false;
            playersHaveFinished[i] = false;
            Player_control.playersPreparedForNextGame[i] = false;
        }
    }

    [ClientRpc]
    private void RpcInitialize()
    {
        distanceLastCard = 50;
        indexCards = 0;
        indexLastCards = 0;
        playersHavePassedTurn = new bool[4];
        passedTextPlayers = new Text[4];
        textPlayersName = new Text[4];
        nameOfPlayers = new String[4];
        playersHaveFinished = new bool[4];
        available = true;

        cardsSpawned = new GameObject[54];

        lastCards = new GameObject[8];

        textYouEnded.gameObject.SetActive(false);
        buttonNextGame.SetActive(false);
        textSended.gameObject.SetActive(false);
        puntuations.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            playersHavePassedTurn[i] = false;
            playersHaveFinished[i] = false;
            Player_control.playersPreparedForNextGame[i] = false;
        }
    }

    [ClientRpc]
    private void RpcDisableTextNumPlayers()
    {
        textNumPlayers.gameObject.SetActive(false);
    }

    private void BeginGame()
    {
        int index;
        GameObject aux;

        Initialize();
        RpcInitialize();

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

        lastGame.Insert(0, -1);
        lastGame.Insert(1, -1);

        position = 1;

        RpcSendNameOfPlayers(Player_control.getplayersName());

        RpcDealCards(numCardsForPlayer, numPlayers);

        RpcChangeColorPlayerTurn(playerTurn, playersHaveFinished);
    }

    [ClientRpc]
    private void RpcSendNameOfPlayers(String[] nameOfPlayers)
    {
        for (int i = 0; i < 4; i++)
        {
            if (nameOfPlayers[i] != null)
            {
                this.nameOfPlayers[i] = nameOfPlayers[i];
                textNamesPuntuationPlayers[i].text = nameOfPlayers[i];
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
        for (int i = 0; i < 54; i++)
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

        this.numPlayers = numPlayers;
        othersCards = new GameObject[numPlayers, numCardsForPlayer];
        indexOthersCards = new int[numPlayers];

        for (int i = 0; i < numPlayers; i++)
        {
            indexOthersCards[i] = 0;
        }

        positionOthersCards();
    }

    private void positionCards()
    {
        for (int i = 0; i < numCardsForPlayer; i++)
        {
            playerCards[i].transform.position = new Vector3((i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), -4, 50 - i);
        }

        for (int i = 0; i < numCardsForPlayer; i++)
        {
            playerCards[i].transform.position = new Vector3((i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), -4, 50 - i);
        }
    }

    private void positionOthersCards()
    {
        passedTextPlayers[numPlayer] = passedTextDown;
        playerDownName.text = nameOfPlayers[numPlayer];
        textPlayersName[numPlayer] = playerDownName;
        if (numPlayers == 2)
        {
            passedTextPlayers[(numPlayer + 1) % numPlayers] = passedTextUp;
            playerUpName.text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            textPlayersName[(numPlayer + 1) % numPlayers] = playerUpName;
            for (int i = 0; i < numCardsForPlayer; i++)
            {
                othersCards[(numPlayer + 1) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.position = new Vector3((i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 4, 50 - i);
            }
        } else if (numPlayers == 3)
        {
            passedTextPlayers[(numPlayer + 1) % numPlayers] = passedTextRight;
            playerRightName.text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            textPlayersName[(numPlayer + 1) % numPlayers] = playerRightName;
            passedTextPlayers[(numPlayer + 2) % numPlayers] = passedTextUp;
            playerUpName.text = nameOfPlayers[(numPlayer + 2) % numPlayers];
            textPlayersName[(numPlayer + 2) % numPlayers] = playerUpName;
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
            playerRightName.text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            textPlayersName[(numPlayer + 1) % numPlayers] = playerRightName;
            passedTextPlayers[(numPlayer + 2) % numPlayers] = passedTextUp;
            playerUpName.text = nameOfPlayers[(numPlayer + 2) % numPlayers];
            textPlayersName[(numPlayer + 2) % numPlayers] = playerUpName;
            passedTextPlayers[(numPlayer + 3) % numPlayers] = passedTextLeft;
            playerLeftName.text = nameOfPlayers[(numPlayer + 3) % numPlayers];
            textPlayersName[(numPlayer + 3) % numPlayers] = playerLeftName;

            for (int i = 0; i < numCardsForPlayer; i++)
            {
                othersCards[(numPlayer + 1) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.position = new Vector3(5.5f, (i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 50 - i);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.Rotate(new Vector3(0, 0, 90));

                othersCards[(numPlayer + 2) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 2) % numPlayers, i].transform.position = new Vector3((i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 4, 50 - i);

                othersCards[(numPlayer + 3) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 3) % numPlayers, i].transform.position = new Vector3(-5.5f, (i * 0.45f) - ((numCardsForPlayer / 2) * 0.45f), 50 - i);
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
        if (numPlayer == playerTurn && available) return true;
        else return false;
    }

    public void ThrowCard(GameObject[] cards, int cardsNumber, int numPlayerCards, int numPlayer)
    {

        if (int.Parse(cards[0].tag) == 14)
        {
            for (int i = 0; i < indexLastCards; i++)
            {
                lastCards[i].transform.Rotate(new Vector3(180, 0, 0));
            }
        }

        indexLastCards = 0;
        for (int i = 0; i < cardsNumber; i++)
        {
            cards[i].transform.position = new Vector3((i * 0.4f) - (cardsNumber / (2f / 0.4f)), i * (-0.1f), distanceLastCard);
            cards[i].transform.Rotate(0, 0, i * 20);
            distanceLastCard -= 0.5f;
            RpcSetCardVisible(cards[i]);

            lastCards[indexLastCards] = cards[i];
            indexLastCards++;
        }
        int numberPlayersPassed = 0;
        if (int.Parse(cards[0].tag) == lastGame[0] || ((lastGame[0] == 10 || lastGame[0] == 11) && (int.Parse(cards[0].tag) == 10 || int.Parse(cards[0].tag) == 11)))
        {
            numberPlayersPassed = cardsNumber;
        } else if (int.Parse(cards[0].tag) == 14)
        {
            numberPlayersPassed = 3;
        }
        lastGame.Insert(0, int.Parse(cards[0].tag));
        lastGame.Insert(1, cardsNumber);
        lastPlayerGame = playerTurn;

        Debug.Log(numPlayerCards);
        if (numPlayerCards <= 0)
        {
            playersHaveFinished[numPlayer] = true;
            RpcPlayerHasFinished(numPlayer, position, playersHaveFinished);
            Player_control.playersPuntuation[numPlayer] += numPlayers - position;

            int count = 0;
            for (int i = 0; i < numPlayers; i++)
            {
                if (!playersHaveFinished[i]) count++;
            }
            if (count < 2)
            {
                RpcShowPuntuation(Player_control.playersPuntuation);
            }
            position++;

        }

        RpcEliminateOtherCards(cardsNumber, playerTurn);

        available = false;

        changeTurn(numberPlayersPassed);
    }

    [ClientRpc]
    public void RpcShowPuntuation(int[] playersPuntuation)
    {
        puntuations.SetActive(true);
        for (int i = 0; i < numPlayers; i++)
        {
            textNamesPuntuationPlayers[i].gameObject.SetActive(true);
            puntuationPlayers[i].gameObject.SetActive(true);
            puntuationPlayers[i].text = "" + playersPuntuation[i];
        }
    }

    [ClientRpc]
    public void RpcEliminateOtherCards(int cardsNumber, int playerTurn)
    {
        for (int i = 0; i < cardsNumber; i++)
        {
            if (indexOthersCards[playerTurn] % 2 == 0)
            {
                Destroy(othersCards[playerTurn, indexOthersCards[playerTurn] / 2]);
            }
            else
            {
                Destroy(othersCards[playerTurn, (numCardsForPlayer - 1) - (indexOthersCards[playerTurn] / 2)]);
            }
            indexOthersCards[playerTurn]++;
        }
    }

    private bool IsLastPlayer(bool[] playersHaveFinished)
    {
        bool lastPlayer = true;
        for (int i = 0; i < numPlayers; i++)
        {
            if (i != numPlayer)
            {
                if (!playersHaveFinished[i]) lastPlayer = false;
            }
        }
        return lastPlayer;
    }

    private bool IsOnlyOnePlayer(bool[] playersHaveFinished)
    {
        int count = 0;
        for (int i = 0; i < numPlayers; i++)
        {
            if (!playersHaveFinished[i]) count++;
        }
        if (count < 2) return true;
        else return false;
    }

    private void DestroyAllOthersCards()
    {
        Debug.Log(numPlayer + "DESTROY ALL OTHER CARDS");
        int currentIndex;
        for (int i = 0; i < numPlayers; i++)
        {
            if (indexOthersCards[i] % 2 == 0) currentIndex = indexOthersCards[i] / 2;
            else currentIndex = indexOthersCards[i] / 2 + 1;

            for (int j = indexOthersCards[i]; j < numCardsForPlayer; j++)
            {
                Debug.Log(othersCards[i, currentIndex] + " - " + currentIndex + " - " + j);
                Destroy(othersCards[i, currentIndex]);
                currentIndex++;
            }
        }
    }

    [ClientRpc]
    public void RpcPlayerHasFinished(int playerHasFinished, int position, bool[] playersHaveFinished)
    {
        if (numPlayer == playerHasFinished)
        {
            textNumYouEnded.GetComponent<Text>().text = "#" + position;
            textYouEnded.gameObject.SetActive(true);

            if (IsOnlyOnePlayer(playersHaveFinished))
            {
                Debug.Log("IS ONLY ONE PLAYER 1" + numPlayer);
                DestroyAllOthersCards();
            }

            buttonNextGame.SetActive(true);
        }
        else
        {
            if (IsLastPlayer(playersHaveFinished))
            {
                Debug.Log("IS LAST PLAYER" + numPlayer);
                textNumYouEnded.GetComponent<Text>().text = "#" + numPlayers;
                textYouEnded.gameObject.SetActive(true);

                for (int i = 0; i < numPlayerCards; i++)
                {
                    playerCards[i].SetActive(false);
                }

                buttonNextGame.SetActive(true);
            }
            else if (IsOnlyOnePlayer(playersHaveFinished))
            {
                Debug.Log("IS ONLY ONE PLAYER 2" + numPlayer);
                DestroyAllOthersCards();
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

            if (!playersHavePassedTurn[playerTurn] && !playersHaveFinished[playerTurn])
            {
                if (numberPlayersPassed > 0)
                {
                    numberPlayersPassed--;
                    if (i < (numPlayers - 1)) RpcSkipPlayer(playerTurn);
                }
                else playerDisponible = true;
            }
            i++;
        }
        if (playerTurn == lastPlayerGame) playerDisponible = false;
        Debug.Log("PLAYER DISPONIBLE: " + playerDisponible);
        available = false;
        if (!playerDisponible)
        {
            if (!playersHaveFinished[playerTurn])
            {
                playerTurn = lastPlayerGame;
            }else if (!playersHaveFinished[(playerTurn + 1) % numPlayers])
            {
                playerTurn = (playerTurn + 1) % numPlayers;
            }
            else
            {
                playerTurn = (playerTurn + 2) % numPlayers;
            }
            
            IEnumerator resetTable = ResetTable();
            StartCoroutine(resetTable);
            
            lastGame.Insert(0, -1);
            lastGame.Insert(1, -1);

            IEnumerator changePlayerTurnName = ChangeColorPlayerTurn(2f);
            StartCoroutine(changePlayerTurnName);
        }
        else
        {
            IEnumerator changePlayerTurnName = ChangeColorPlayerTurn(0.3f);
            StartCoroutine(changePlayerTurnName);
        }
    }

    [ClientRpc]
    private void RpcSkipPlayer(int numPlayer)
    {
        passedTextPlayers[numPlayer].text ="SKIPPED";
        passedTextPlayers[numPlayer].color = orange;
        passedTextPlayers[numPlayer].gameObject.SetActive(true);

        IEnumerator disableSkippedPlayer = DisableSkippedPlayer(2f, numPlayer);
        StartCoroutine(disableSkippedPlayer);
    }

    private IEnumerator DisableSkippedPlayer(float time, int numPlayer)
    {
        yield return new WaitForSeconds(time);
        passedTextPlayers[numPlayer].text = "PASSED";
        passedTextPlayers[numPlayer].color = red;
        passedTextPlayers[numPlayer].gameObject.SetActive(false);
    }

    private IEnumerator ChangeColorPlayerTurn(float time)
    {
        yield return new WaitForSeconds(time);
        RpcChangeColorPlayerTurn(playerTurn, playersHaveFinished);
        available = true;
    }

    private IEnumerator ResetTable()
    {
        yield return new WaitForSeconds(2f);
        int i;
        for (i = 0; i < indexLastCards; i++)
        {
            if (int.Parse(lastCards[i].gameObject.tag) != 14)
            {
                /*GameObject reversCardInst = Instantiate(reversCard, new Vector3(-0.4f/2f, 0, distanceLastCard), reversCard.transform.rotation);
                reversCardInst.transform.Rotate(0, 0, 180);
                distanceLastCard -= 0.5f;
                NetworkServer.Spawn(reversCardInst);*/
                lastCards[i].transform.Rotate(new Vector3(180, 0, 0));
            }
        }

        for (i = 0; i < numPlayers; i++)
        {
            playersHavePassedTurn[i] = false;
        }
        indexLastCards = 0;
        RpcDisablePassedText();
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
    private void RpcChangeColorPlayerTurn(int playerTurn, bool[] playersHaveFinished)
    {
        for (int i = 0; i < 4; i++)
        {
            if (textPlayersName[i] != null)
            {
                if (i != playerTurn || IsOnlyOnePlayer(playersHaveFinished)) textPlayersName[i].color = Color.black;
                else textPlayersName[i].color = Color.green;
            }
        }
    }

    public void ButtonPassTurn()
    {
        Debug.Log("Button pass turn");
        if (numPlayers > 1 && lastGame[0] != -1)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                player.GetComponent<Player>().PassTurn(numPlayer, playerTurn);
            }
        }
    }

    public void ButtonNextGame()
    {
        Debug.Log("Button next game");
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<Player>().PlayerPrepared(numPlayer);
        }
        textSended.gameObject.SetActive(true);
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
        //Debug.Log("AQUIII" + numPlayer);
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

    public void AddPlayerPrepared(int numPlayer)
    {
        bool beginNextGame = true;
        Player_control.playersPreparedForNextGame[numPlayer] = true;
        for (int i = 0; i < numPlayers; i++)
        {
            Debug.Log(i+"-"+Player_control.playersPreparedForNextGame[i]);
            if (!Player_control.playersPreparedForNextGame[i]) beginNextGame = false;
        }
        if (beginNextGame)
        {
            for (int i = 0; i < 54; i++)
            {
                Destroy(cardsSpawned[i]);
            }
            BeginGame();
        }
    }
}
