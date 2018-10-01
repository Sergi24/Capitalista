using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Networking.Match;

public class MenuController : MonoBehaviour {

    public GameObject textMatchesName, textMatchesSize;
    public GameObject textNoRoomsCreated;

    public GameObject inputField;
    public GameObject[] joins;
    public GameObject buttonCreateRoom;

    private void Start()
    {
        inputField.GetComponent<InputField>().text = Player_control.playerLocalName;

        StartCoroutine(ListMatches());
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
    }

    public void CreateMatch()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManagerController>().CreateMatch();
    }

    public void JoinMatch(int room)
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManagerController>().JoinMatch(room);
    }

    public IEnumerator ListMatches()
    {
        for (; ; )
        {
            GameObject.Find("NetworkManager").GetComponent<NetworkManagerController>().ListMatches();
            yield return new WaitForSeconds(1);
        }
    }

    public void WriteRoomsTable(List<MatchInfoSnapshot> matchList)
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
