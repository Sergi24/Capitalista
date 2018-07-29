using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MenuController : MonoBehaviour {

    public GameObject inputField;
    public GameObject networkManager;

    private void Start()
    {
        inputField.GetComponent<InputField>().text = Player_control.playerLocalName;
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetKeyDown(KeyCode.Return))
        {
            Player_control.playerLocalName = inputField.GetComponent<InputField>().text;
            networkManager.GetComponent<NetworkManagerHUD>().enabled = true;
            Debug.Log(Player_control.playerLocalName);
        }
	}
}
