using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

public class NetworkManagerController : MonoBehaviour
{
    private long[] networksIds = new long[4];

    private void Start()
    {
        gameObject.GetComponent<NetworkManager>().StartMatchMaker();
    }

    public void CreateMatch()
    {
        gameObject.GetComponent<NetworkManager>().matchMaker.CreateMatch(Player_control.playerLocalName, 4, true, "", "", "", 0, 0, gameObject.GetComponent<NetworkManager>().OnMatchCreate);
    }

    public void JoinMatch(int room)
    {
        gameObject.GetComponent<NetworkManager>().matchMaker.JoinMatch((NetworkID)networksIds[room], "", "", "", 0, 0, gameObject.GetComponent<NetworkManager>().OnMatchJoined);
    }

    public void ListMatches()
    {
        if (gameObject.GetComponent<NetworkManager>().matchMaker == null)
        {
            gameObject.GetComponent<NetworkManager>().StartMatchMaker();
        }
        gameObject.GetComponent<NetworkManager>().matchMaker.ListMatches(0, 4, "", true, 0, 0, OnMatchList);
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        gameObject.GetComponent<NetworkManager>().OnMatchList(success, extendedInfo, matchList);
        
        if (success)
        {
            if (SceneManager.GetActiveScene().name.Equals("Menu"))
            {
                int count = 0;
                foreach (MatchInfoSnapshot match in matchList)
                {
                    networksIds[count] = (long)match.networkId;
                    count++;
                }
                GameObject.Find("MenuController").GetComponent<MenuController>().WriteRoomsTable(matchList);
            }
        }
    }

    public void Stop()
    {
        gameObject.GetComponent<NetworkManager>().StopHost();
        gameObject.GetComponent<NetworkManager>().StartMatchMaker();
    }
}