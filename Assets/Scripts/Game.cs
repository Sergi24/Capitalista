using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Game : NetworkBehaviour
{
    public GameObject[] cards;
    public GameObject notPossibleCube;
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
    public GameObject passedTextUpImage, passedTextDownImage, passedTextRightImage, passedTextLeftImage;
    public Text playerDownName, playerRightName, playerUpName, playerLeftName, textNumPlayersNumber, textSended, textTimeNumber;
    public GameObject playerDownNameImage, playerRightNameImage, playerUpNameImage, playerLeftNameImage;
    public GameObject emojiInstantiatorDown, emojiInstantiatorRight, emojiInstantiatorUp, emojiInstantiatorLeft;
    public GameObject[] emojis;
    public GameObject puntuations, textNumPlayers;
    public Text[] textNamesPuntuationPlayers, puntuationPlayers, textNamesPuntuationPlayersOptions, puntuationPlayersOptions;
    private Text[] passedTextPlayers, textPlayersName;
    private GameObject[] passedTextPlayersImages;
    private Vector3[] emojisInstantiators;
    public GameObject panelEmojis;

    public GameObject buttonToBegin;
    public GameObject textForWaiting, onePlayerDisconnected;

    public Color red, orange, green, yellow;

    public GameObject buttonNextGame, buttonPassTurn, buttonPlayCards, buttonEmojis;

    public AudioClip cardSound, passSound;

    public GameObject buttonOptions;

    private int throwNumber;
    public int remainingTime;

    [SyncVar]
    private bool available;

    private bool startedGame;

    private int timeNumber;

    private AudioSource asource;

    // Use this for initialization
    void Start() {
        numPlayer = GameObject.FindGameObjectsWithTag("Player").Length - 1;

        asource = gameObject.GetComponent<AudioSource>();
        startedGame = false;
        numPlayers = 1;

        if (isServer) buttonToBegin.SetActive(true);
        else textForWaiting.SetActive(true);

        textNumPlayers.gameObject.SetActive(true);
        buttonPassTurn.GetComponent<Button>().interactable = false;
        buttonPlayCards.GetComponent<Button>().interactable = false;

        StartCoroutine(FillTextNumPlayers());
    }

    IEnumerator SameNumberOfPlayers()
    {
        for (;;)
        {
            if (numPlayers != GameObject.FindGameObjectsWithTag("Player").Length)
            {
                RpcShowTextNumPlayersDifferent();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    [ClientRpc]
    public void RpcShowTextNumPlayersDifferent()
    {
        onePlayerDisconnected.SetActive(true);
        Invoke("ExitGame", 3f);
    }

    IEnumerator FillTextNumPlayers()
    {
        for (; ; )
        {
            textNumPlayersNumber.GetComponent<Text>().text = "" + GameObject.FindGameObjectsWithTag("Player").Length;
            
            if (isServer)
            {
                if (GameObject.FindGameObjectsWithTag("Player").Length > 1) buttonToBegin.GetComponent<Button>().interactable = true;
                else buttonToBegin.GetComponent<Button>().interactable = false;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void ButtonToBeginPressed()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1 && !startedGame)
        {
            //Debug.Log("BUTTON TO BEGIN PRESSED");
            BeginGame();
            startedGame = true;
            StartCoroutine(SameNumberOfPlayers());
                
            RpcDisableTextNumPlayersAndWaitinText();
        }
    }

    public void Initialize()
    {
        distanceLastCard = 50;
        indexCards = 0;
        indexLastCards = 0;
        playersHavePassedTurn = new bool[4];
        passedTextPlayers = new Text[4];
        passedTextPlayersImages = new GameObject[4];
        textPlayersName = new Text[4];
        emojisInstantiators = new Vector3[4];
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

        throwNumber = 0;
    }

    [ClientRpc]
    private void RpcInitialize()
    {
        distanceLastCard = 50;
        indexCards = 0;
        indexLastCards = 0;
        playersHavePassedTurn = new bool[4];
        passedTextPlayers = new Text[4];
        passedTextPlayersImages = new GameObject[4];
        textPlayersName = new Text[4];
        nameOfPlayers = new String[4];
        emojisInstantiators = new Vector3[4];
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
    private void RpcDisableTextNumPlayersAndWaitinText()
    {
        StopCoroutine(FillTextNumPlayers());
        startedGame = true;
        buttonToBegin.SetActive(false);
        textNumPlayers.SetActive(false);
        textForWaiting.gameObject.SetActive(false);
    }

    private void BeginGame()
    {
        int index;
        GameObject aux;

        Initialize();
        RpcInitialize();

        RpcSetEmojisButtonEnabled();
        RpcEliminateNotPossibleCube();

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

        if (!startedGame) RpcSetPuntuationTo0(Player_control.getplayersName());

        RpcDealCards(numCardsForPlayer, numPlayers);

        RpcChangePlayerTurn(playerTurn, playersHaveFinished);

        RpcRefreshButtonsInteraction(playerTurn);

        timeNumber = remainingTime;
        RpcSetTime(timeNumber);
        StartCoroutine(ReduceTime(throwNumber));
    }

    [ClientRpc]
    private void RpcEliminateNotPossibleCube()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<NetworkIdentity>().isLocalPlayer) player.GetComponent<Player>().EliminateNotPossibleCubes();
        }
    }

    [ClientRpc]
    private void RpcSetEmojisButtonEnabled()
    {
        buttonEmojis.GetComponent<Button>().interactable = true;
    }

    [ClientRpc]
    private void RpcSendNameOfPlayers(String[] nameOfPlayers)
    {
        for (int i = 0; i < 4; i++)
        {
            this.nameOfPlayers[i] = nameOfPlayers[i];
            textNamesPuntuationPlayers[i].text = nameOfPlayers[i];
            textNamesPuntuationPlayersOptions[i].text = nameOfPlayers[i];
        }
    }

    [ClientRpc]
    private void RpcSetPuntuationTo0(String[] nameOfPlayers)
    {
        for (int i = 0; i < 4; i++)
        {
            if (nameOfPlayers[i].Length != 0) puntuationPlayersOptions[i].text = 0.ToString();
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
            playerCards[i].transform.position = new Vector3((i * 0.6f) - ((numCardsForPlayer / 2) * 0.6f), -4, 50 - i);
        }
    }

    private void positionOthersCards()
    {
        passedTextPlayers[numPlayer] = passedTextDown;
        passedTextPlayersImages[numPlayer] = passedTextDownImage;
        playerDownName.text = nameOfPlayers[numPlayer];
        playerDownNameImage.SetActive(true);
        textPlayersName[numPlayer] = playerDownName;
        emojisInstantiators[numPlayer] = emojiInstantiatorDown.transform.position;
        if (numPlayers == 2)
        {
            passedTextPlayers[(numPlayer + 1) % numPlayers] = passedTextUp;
            passedTextPlayersImages[(numPlayer + 1) % numPlayers] = passedTextUpImage;
            playerUpName.text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            textPlayersName[(numPlayer + 1) % numPlayers] = playerUpName;
            emojisInstantiators[(numPlayer + 1) % numPlayers] = emojiInstantiatorUp.transform.position;
            playerUpNameImage.SetActive(true);
            for (int i = 0; i < numCardsForPlayer; i++)
            {
                othersCards[(numPlayer + 1) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.position = new Vector3((i * 0.35f) - ((numCardsForPlayer / 2) * 0.35f), 4, 50 - i);
            }
        } else if (numPlayers == 3)
        {
            passedTextPlayers[(numPlayer + 1) % numPlayers] = passedTextRight;
            passedTextPlayersImages[(numPlayer + 1) % numPlayers] = passedTextRightImage;
            playerRightName.text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            textPlayersName[(numPlayer + 1) % numPlayers] = playerRightName;
            emojisInstantiators[(numPlayer + 1) % numPlayers] = emojiInstantiatorRight.transform.position;
            passedTextPlayers[(numPlayer + 2) % numPlayers] = passedTextUp;
            passedTextPlayersImages[(numPlayer + 2) % numPlayers] = passedTextUpImage;
            playerUpName.text = nameOfPlayers[(numPlayer + 2) % numPlayers];
            textPlayersName[(numPlayer + 2) % numPlayers] = playerUpName;
            emojisInstantiators[(numPlayer + 2) % numPlayers] = emojiInstantiatorUp.transform.position;
            playerRightNameImage.SetActive(true);
            playerUpNameImage.SetActive(true);
            for (int i = 0; i < numCardsForPlayer; i++)
            {
                othersCards[(numPlayer + 1) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.position = new Vector3(7.7f, (i * 0.35f) - ((numCardsForPlayer / 2) * 0.35f), 50 - i);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.Rotate(new Vector3(0, 0, 90));


                othersCards[(numPlayer + 2) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 2) % numPlayers, i].transform.position = new Vector3((i * 0.35f) - ((numCardsForPlayer / 2) * 0.35f), 4, 50 - i);
            }
        }
        else
        {
            passedTextPlayers[(numPlayer + 1) % numPlayers] = passedTextRight;
            passedTextPlayersImages[(numPlayer + 1) % numPlayers] = passedTextRightImage;
            playerRightName.text = nameOfPlayers[(numPlayer + 1) % numPlayers];
            textPlayersName[(numPlayer + 1) % numPlayers] = playerRightName;
            emojisInstantiators[(numPlayer + 1) % numPlayers] = emojiInstantiatorRight.transform.position;
            passedTextPlayers[(numPlayer + 2) % numPlayers] = passedTextUp;
            passedTextPlayersImages[(numPlayer + 2) % numPlayers] = passedTextUpImage;
            playerUpName.text = nameOfPlayers[(numPlayer + 2) % numPlayers];
            textPlayersName[(numPlayer + 2) % numPlayers] = playerUpName;
            emojisInstantiators[(numPlayer + 2) % numPlayers] = emojiInstantiatorUp.transform.position;
            passedTextPlayers[(numPlayer + 3) % numPlayers] = passedTextLeft;
            passedTextPlayersImages[(numPlayer + 3) % numPlayers] = passedTextLeftImage;
            playerLeftName.text = nameOfPlayers[(numPlayer + 3) % numPlayers];
            textPlayersName[(numPlayer + 3) % numPlayers] = playerLeftName;
            emojisInstantiators[(numPlayer + 3) % numPlayers] = emojiInstantiatorLeft.transform.position;
            playerRightNameImage.SetActive(true);
            playerUpNameImage.SetActive(true);
            playerLeftNameImage.SetActive(true);
            for (int i = 0; i < numCardsForPlayer; i++)
            {
                othersCards[(numPlayer + 1) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.position = new Vector3(7.7f, (i * 0.35f) - ((numCardsForPlayer / 2) * 0.35f), 50 - i);
                othersCards[(numPlayer + 1) % numPlayers, i].transform.Rotate(new Vector3(0, 0, 90));

                othersCards[(numPlayer + 2) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 2) % numPlayers, i].transform.position = new Vector3((i * 0.35f) - ((numCardsForPlayer / 2) * 0.35f), 4, 50 - i);

                othersCards[(numPlayer + 3) % numPlayers, i] = Instantiate(reversCard, transform.position, reversCard.transform.rotation);
                othersCards[(numPlayer + 3) % numPlayers, i].transform.position = new Vector3(-7.7f, (i * 0.35f) - ((numCardsForPlayer / 2) * 0.35f), 50 - i);
                othersCards[(numPlayer + 3) % numPlayers, i].transform.Rotate(new Vector3(0, 0, -90));
            }
        }
    }

    public bool IsCardPosible(int cardNumber)
    {
        if (lastGame[0] == -1) return true;
        else if (cardNumber == 14) return true;

        if (!IsCardPosibleByNumber(cardNumber, lastGame[1])) return false;

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
        RpcCardSound();

        if (int.Parse(cards[0].tag) == 14)
        {
            RpcReverseLastCards(indexLastCards);
        }

        indexLastCards = 0;
        for (int i = 0; i < cardsNumber; i++)
        {
            RpcSetCardPosition(cards[i], i, cardsNumber, distanceLastCard);
            distanceLastCard -= 0.5f;
            RpcSetCardVisible(cards[i]);

            RpcAddLastCards(cards[i], indexLastCards);
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

        RpcEliminateOtherCards(cardsNumber, playerTurn);

        //Debug.Log(numPlayerCards);
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
                StopCoroutine(ReduceTime(0));
                RpcShowPuntuation(Player_control.playersPuntuation);
            }
            else
            {
                position++;

                available = false;

                changeTurn(numberPlayersPassed);
            }
        }
        else
        {
            available = false;

            changeTurn(numberPlayersPassed);
        }
    }

    [ClientRpc]
    public void RpcSetCardPosition(GameObject card, int i, int cardsNumber, float distanceLastCard)
    {
        card.transform.position = new Vector3((i * 0.4f) - (cardsNumber / (2f / 0.4f)), i * (-0.1f) + 0.69f, distanceLastCard);
        card.transform.Rotate(0, 0, i * 20);
    }

    [ClientRpc]
    public void RpcAddLastCards(GameObject card, int indexLastCards)
    {
        lastCards[indexLastCards] = card;
        this.indexLastCards = indexLastCards+1;
    }

    [ClientRpc]
    public void RpcReverseLastCards(int indexLastCards)
    {
        for (int i = 0; i < indexLastCards; i++)
        {
            if (int.Parse(lastCards[i].gameObject.tag) != 14)
            {
                lastCards[i].transform.Rotate(new Vector3(180, 0, 0));
            }
        }
        this.indexLastCards = 0;
    }

    [ClientRpc]
    public void RpcCardSound()
    {
        asource.clip = cardSound;
        asource.volume = 0.9f;
        asource.Play();
    }

    [ClientRpc]
    public void RpcPassedSound()
    {
        asource.clip = passSound;
        asource.volume = 0.6f;
        asource.Play();
    }

    [ClientRpc]
    public void RpcAlertSound()
    {
        GameObject.Find("Alerta").GetComponent<AudioSource>().Play();
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
            puntuationPlayersOptions[i].text = "" + playersPuntuation[i];
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
        //Debug.Log(numPlayer + "DESTROY ALL OTHER CARDS");
        int currentIndex;
        for (int i = 0; i < numPlayers; i++)
        {
            if (indexOthersCards[i] % 2 == 0) currentIndex = indexOthersCards[i] / 2;
            else currentIndex = indexOthersCards[i] / 2 + 1;

            for (int j = indexOthersCards[i]; j < numCardsForPlayer; j++)
            {
                //Debug.Log(othersCards[i, currentIndex] + " - " + currentIndex + " - " + j);
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
                //Debug.Log("IS ONLY ONE PLAYER 1" + numPlayer);
                DestroyAllOthersCards();
            }

            buttonNextGame.SetActive(true);
        }
        else
        {
            if (IsLastPlayer(playersHaveFinished))
            {
                //Debug.Log("IS LAST PLAYER" + numPlayer);
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
                //Debug.Log("IS ONLY ONE PLAYER 2" + numPlayer);
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
        if (lastGame[0] == -1 && timeNumber == 0)
        {
            lastPlayerGame = playerTurn;
            playerDisponible = false;
        }

        if (playerTurn == lastPlayerGame) playerDisponible = false;
        //Debug.Log("PLAYER DISPONIBLE: " + playerDisponible);
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

            IEnumerator changePlayerTurnName = ChangePlayerTurn(2f);
            StartCoroutine(changePlayerTurnName);
        }
        else
        {
            IEnumerator changePlayerTurnName = ChangePlayerTurn(0.3f);
            StartCoroutine(changePlayerTurnName);
        }

        throwNumber += 1;
    }

    [ClientRpc]
    private void RpcSkipPlayer(int numPlayer)
    {
        passedTextPlayers[numPlayer].text ="SKIPPED";
        passedTextPlayers[numPlayer].color = orange;
        passedTextPlayersImages[numPlayer].SetActive(true);

        IEnumerator disableSkippedPlayer = DisableSkippedPlayer(2f, numPlayer);
        StartCoroutine(disableSkippedPlayer);
    }

    private IEnumerator DisableSkippedPlayer(float time, int numPlayer)
    {
        yield return new WaitForSeconds(time);
        passedTextPlayers[numPlayer].text = "PASSED";
        passedTextPlayers[numPlayer].color = red;
        passedTextPlayersImages[numPlayer].SetActive(false);
    }

    private IEnumerator ChangePlayerTurn(float time)
    {
        yield return new WaitForSeconds(time);
        RpcChangePlayerTurn(playerTurn, playersHaveFinished);
        RpcNotPossibleCards(playerTurn, lastGame[1]);
        available = true;

        timeNumber = remainingTime;
        textTimeNumber.GetComponent<Text>().text = remainingTime.ToString();
        RpcSetTime(remainingTime);
        StartCoroutine(ReduceTime(throwNumber));

        RpcRefreshButtonsInteraction(playerTurn);
    }

    [ClientRpc]
    private void RpcNotPossibleCards(int playerTurn, int cardsNumber)
    {
        if (playerTurn == numPlayer)
        {
            foreach (GameObject card in playerCards)
            {
                if (!IsCardPosible(int.Parse(card.tag)))
                {
                    Instantiate(notPossibleCube, card.transform.position, card.transform.rotation);
                }
            }
        }
    }

    public bool IsCardPosibleByNumber(int cardNumber, int cardsNumber)
    {
        int count = 0;
        foreach (GameObject card in playerCards)
        {
            if (int.Parse(card.tag) == cardNumber) count++;
            else if ((cardNumber == 10 || cardNumber == 11) && (int.Parse(card.tag) == 10 || int.Parse(card.tag) == 11)) count++;
        }
        if (count >= cardsNumber) return true;
        else return false;
    }


    [ClientRpc]
    private void RpcRefreshButtonsInteraction(int playerTurn)
    {
        if (playerTurn == numPlayer)
        {
            buttonPassTurn.GetComponent<Button>().interactable = true;
            buttonPlayCards.GetComponent<Button>().interactable = true;
            Handheld.Vibrate();
        }
        else
        {
            buttonPassTurn.GetComponent<Button>().interactable = false;
            buttonPlayCards.GetComponent<Button>().interactable = false;
        }
    }


    private IEnumerator ResetTable()
    {
        yield return new WaitForSeconds(2f);
        RpcReverseLastCards(indexLastCards);

        for (int i = 0; i < numPlayers; i++)
        {
            playersHavePassedTurn[i] = false;
        }
        RpcDisablePassedText();
    }

    [ClientRpc]
    private void RpcDisablePassedText()
    {
        for (int i=0; i<4; i++)
        {
            if (passedTextPlayers[i] != null)
            {
                passedTextPlayersImages[i].SetActive(false);
            }
        }
    }

    [ClientRpc]
    private void RpcChangePlayerTurn(int playerTurn, bool[] playersHaveFinished)
    {
        for (int i = 0; i < 4; i++)
        {
            if (textPlayersName[i] != null)
            {
                if (i != playerTurn || IsOnlyOnePlayer(playersHaveFinished)) textPlayersName[i].color = Color.black;
                else textPlayersName[i].color = yellow;
            }
        }
    }

    public void ButtonPassTurn()
    {
        //Debug.Log("Button pass turn");
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
        //Debug.Log("Button next game");
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
        RpcPassedSound();
        changeTurn(0);
    }

    [ClientRpc]
    private void RpcPlayerHasPassed(int numPlayer)
    {
        //Debug.Log("AQUIII" + numPlayer);
        passedTextPlayersImages[numPlayer].SetActive(true);
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
            bool found = false;
            for (int i = 0; i < numPlayerCards; i++)
            {
                //Debug.Log(i+" - "+ cardsChoosed[indexCard]+" - "+ playerCards[i]);
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
            /*for (int i = 0; i < numPlayerCards; i++)
            {
                Debug.Log(playerCards[i].name);
            }*/
        }
        //Debug.Log("Length player cards: "+playerCards.Length+" "+ numPlayerCards);
    }

    public void AddPlayerPrepared(int numPlayer)
    {
        bool beginNextGame = true;
        Player_control.playersPreparedForNextGame[numPlayer] = true;
        for (int i = 0; i < numPlayers; i++)
        {
            //Debug.Log(i+"-"+Player_control.playersPreparedForNextGame[i]);
            if (!Player_control.playersPreparedForNextGame[i]) beginNextGame = false;
        }
        if (beginNextGame)
        {
            for (int i = 0; i < 54; i++)
            {
                NetworkServer.Destroy(cardsSpawned[i]);
                //Destroy(cardsSpawned[i]);
            }
            BeginGame();
        }
    }

    public void PlaySelectedCards()
    {
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<Player>().PlaySelectedCards();
        }
    }

    IEnumerator ReduceTime(int throwNumber)
    {
        for (;;)
        {
            yield return new WaitForSeconds(1);

            if (throwNumber != this.throwNumber || !available) break;
            if (timeNumber > 0)
            {
                timeNumber -= 1;
                RpcSetTime(timeNumber);
                if (timeNumber == 6 || timeNumber == 4 || timeNumber == 2 || timeNumber == 1) RpcAlertSound();
            }
            else if (timeNumber == 0)
            {
                RpcRemainingTimeOut();
                break;
            }
        }
    }

    [ClientRpc]
    private void RpcSetTime(int timeNumber)
    {
        textTimeNumber.GetComponent<Text>().text = timeNumber.ToString();

        if (timeNumber == remainingTime) textTimeNumber.GetComponent<Text>().color = green;
        if (timeNumber == 15) textTimeNumber.GetComponent<Text>().color = orange;
        else if (timeNumber == 6) textTimeNumber.GetComponent<Text>().color = red;
    }

    [ClientRpc]
    private void RpcRemainingTimeOut()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<Player>().PassTurn(numPlayer, playerTurn);
        }
    }

    public void ButtonEmojiPressed()
    {
        if (!panelEmojis.activeSelf) panelEmojis.SetActive(true);
        else panelEmojis.SetActive(false);
    }

    public void EmojiSelected(int numEmoji)
    {
        if (numPlayers > 1)
        {
            panelEmojis.SetActive(false);
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                player.GetComponent<Player>().EmojiSelected(numPlayer, numEmoji);
            }
        }
    }

    [ClientRpc]
    public void RpcThrowEmoji(int numPlayer, int numEmoji)
    {
        Instantiate(emojis[numEmoji], emojisInstantiators[numPlayer], Quaternion.identity);
    }

    public void ShowOptions()
    {
        if (buttonOptions.activeSelf) buttonOptions.SetActive(false);
        else buttonOptions.SetActive(true);
    }

    public void ExitGame()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManagerController>().Stop();
    }
}
