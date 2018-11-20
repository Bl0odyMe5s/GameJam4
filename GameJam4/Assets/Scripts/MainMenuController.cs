using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button stopConnectingButton;

    public void StartHosting()
    {
        CustomNetworkManager.instance.StartHosting();
    }

    public void StartClient()
    {
        CustomNetworkManager.instance.StartClient();
        stopConnectingButton.gameObject.SetActive(true);
    }

    public void SetNewIpAdress(string ip)
    {
        CustomNetworkManager.instance.networkAddress = ip;
    }

    public void StopHosting()
    {
        CustomNetworkManager.instance.StopHosting();
        stopConnectingButton.gameObject.SetActive(false);
    }

    public void SetPlayerName(string name)
    {
        CustomNetworkManager.playerName = name;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
