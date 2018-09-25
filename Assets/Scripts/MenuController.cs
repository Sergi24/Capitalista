using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class MenuController : MonoBehaviour {

    public GameObject inputField;
    public GameObject networkManagerGO;
    public GameObject textMatchesName, textMatchesSize;
    public GameObject[] joins;
    public GameObject textNoRoomsCreated;

    private NetworkID networkId;
    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = networkManagerGO.GetComponent<NetworkManager>();
        inputField.GetComponent<InputField>().text = Player_control.playerLocalName;

        networkManager.StartMatchMaker();

        StartCoroutine(WriteMatchTable());
    }

    public void NameIntroduced()
    {
        if (inputField.GetComponent<InputField>().text.Length > 0)
        {
            Player_control.playerLocalName = inputField.GetComponent<InputField>().text;
            //networkManager.GetComponent<NetworkManagerHUD>().enabled = true;
            Debug.Log(Player_control.playerLocalName);
        }
    }

    public void CreateMatch()
    {
        networkManager.matchMaker.CreateMatch(Player_control.playerLocalName, 4, true, "", "", "", 0, 0, OnMatchCreate);
    }

    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (!success) Debug.Log("Ha anat malament el create");
        else
        {
            NetworkManager.singleton.StartHost(matchInfo);
        }
    }

    public void JoinMatch()
    {
        networkManager.matchMaker.JoinMatch(networkId, "", "", "", 0, 0, OnMatchJoined);
    }

    public void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (!success) Debug.Log("Ha anat malament el join");
    }

    IEnumerator WriteMatchTable()
    {
        for (;;)
        {
            networkManager.matchMaker.ListMatches(0, 3, "", true, 0, 0, OnMatchList);
            yield return new WaitForSeconds(1);
        }
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        if (!success) Debug.Log("Ha anat malament");
        else
        {
            string names = "";
            string capacity = "";

            int count = 0;
            if (matchList != null)
            {
                foreach (MatchInfoSnapshot match in matchList)
                {
                    joins[count].SetActive(true);
                    count++;

                    names += match.name + "\n\n";
                    capacity += match.currentSize + "/" + match.maxSize + "\n\n";
                }
            }

            for(int i=count; i<4; i++)
            {
                joins[count].SetActive(false);
            }
            if (count == 0) textNoRoomsCreated.SetActive(true);
            else textNoRoomsCreated.SetActive(false);

            textMatchesName.GetComponent<Text>().text = names;
            textMatchesSize.GetComponent<Text>().text = capacity;
        }
    }
}
