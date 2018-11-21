using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : Prototype.NetworkLobby.LobbyManager {

    private static CustomNetworkManager _instance;
    public static CustomNetworkManager instance
    {
        get
        {
            return _instance;
        }
    }

    public int playerCount;
    public static string playerName = "NoName";

    private GameStateManager gameStateManager;
    public GameObject alienPrefab;
    
    void Awake()
    {
        if (FindObjectsOfType<CustomNetworkManager>().Length > 1)
        {
            DestroyImmediate(gameObject);
            return;
        }

        if(_instance == null || _instance != this)
        {
            DestroyImmediate(_instance);
        }

        _instance = this;

        gameStateManager = GetComponent<GameStateManager>();
    }

	public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        playerCount++;
        Debug.Log("Player with IP: " + conn.address + " connected, total players: " + playerCount);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        playerCount--;
        Debug.Log("Player with IP: " + conn.address + " disconnected, total players: " + playerCount);
    }

    public void StartAsClient(string ipAdress)
    {
        networkAddress = ipAdress;
        StartClient();
    }

    public void StartHosting()
    {
        CustomNetworkManager.instance.StartHost();
    }

    public void StopHosting()
    {
        CustomNetworkManager.instance.StopHost();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if(!gameStateManager.hasAlienSelected)
        {
            GameObject player = Instantiate(alienPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            gameStateManager.hasAlienSelected = true;
        }
        else
        {
            Transform spawnPos = GetStartPosition();
            GameObject player = Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
    }
}
