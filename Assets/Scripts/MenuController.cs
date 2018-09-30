using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    public GameObject inputField;
    public GameObject textMatchesName, textMatchesSize;
    public GameObject[] joins;
    public GameObject textNoRoomsCreated;
    public GameObject buttonCreateRoom;

    private NetworkManager networkManager;
    private long[] networksIds = new long[4];

    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        inputField.GetComponent<InputField>().text = Player_control.playerLocalName;

        networkManager.StartMatchMaker();

        StartCoroutine(WriteMatchTable());
    }

    private void Update()
    {
        bool activar = false;

        if (inputField.GetComponent<InputField>().text.Length == 0) activar = false;
        else activar = true;
        
        foreach(GameObject join in joins)
        {
            join.GetComponent<Button>().interactable = activar;
        }
        buttonCreateRoom.GetComponent<Button>().interactable = activar;
    }

    public void NameIntroduced()
    {
         Player_control.playerLocalName = inputField.GetComponent<InputField>().text;
         //Debug.Log(Player_control.playerLocalName);
    }

    public void CreateMatch()
    {
        NameIntroduced();
        networkManager.matchMaker.CreateMatch(Player_control.playerLocalName, 4, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
    }

    public void JoinMatch(int room)
    {
        NameIntroduced();
        networkManager.matchMaker.JoinMatch((NetworkID)networksIds[room], "", "", "", 0, 0, networkManager.OnMatchJoined);
    }

    IEnumerator WriteMatchTable()
    {
        for (;;)
        {
            networkManager.matchMaker.ListMatches(0, 4, "", true, 0, 0, OnMatchList);
            yield return new WaitForSeconds(1);
        }
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        networkManager.OnMatchList(success, extendedInfo, matchList);
        if (success)
        {
            if (SceneManager.GetActiveScene().name.Equals("Menu"))
            {
                string names = "";
                string capacity = "";

                int count = 0;
                if (matchList != null)
                {
                    foreach (MatchInfoSnapshot match in matchList)
                    {
                        joins[count].SetActive(true);
                        networksIds[count] = (long)match.networkId;
                        count++;

                        names += match.name + "\n\n";
                        capacity += match.currentSize + "/" + match.maxSize + "\n\n";

                    }
                }

                for (int i = count; i < 4; i++)
                {
                    joins[i].SetActive(false);
                }
                if (count == 0) textNoRoomsCreated.SetActive(true);
                else textNoRoomsCreated.SetActive(false);

                textMatchesName.GetComponent<Text>().text = names;
                textMatchesSize.GetComponent<Text>().text = capacity;
            }
        }
    }
}
