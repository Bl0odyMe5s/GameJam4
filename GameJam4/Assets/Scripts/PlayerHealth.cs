using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using Prototype.NetworkLobby;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 100;

    private Player player;
    int health;

    private Text healthText;

    public static GameObject winTextObj;
    public static int amountOfMarines;

    void Awake()
    {
        player = GetComponent<Player>();
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<Text>();
        }

        if (winTextObj == null)
        {
            winTextObj = GameObject.FindGameObjectWithTag("WinText");
            winTextObj.SetActive(false);
        }

        if(isServer)
        {
            RpcSetHealth(health);
        }
    }

    [ServerCallback]
    void OnEnable()
    {
        health = maxHealth;
    }

    [Server]
    public bool TakeDamage(int damage)
    {
        bool died = false;

        if (health <= 0)
            return died;
        health -= damage;
        died = health <= 0;

        if (gameObject.CompareTag("Alien") && died)
        {
            RpcAlienKilled();
            StartCoroutine(ReturnToLobby(4f));
        }

        if(gameObject.CompareTag("Player") && died)
        {
            amountOfMarines--;

            if(amountOfMarines <= 0)
            {
                // Marines dead
                RpcMarinesKilled();
                StartCoroutine(ReturnToLobby(4f));
            }
        }

        RpcSetHealth(health);

        return died;
    }

    [ClientRpc]
    void RpcSetHealth(int currentHealth)
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if(player != null)
                player.Die();
        }

        health = currentHealth;

        if(healthText != null)
            healthText.text = "Health: " + currentHealth;
    }

    [Server]
    private IEnumerator ReturnToLobby(float timeToSwitch)
    {
        yield return new WaitForSeconds(timeToSwitch);
        LobbyManager.s_Singleton.ServerReturnToLobby();
    }

    [ClientRpc]
    void RpcAlienKilled()
    {
        winTextObj.GetComponent<Text>().text = "Marines win!";
        winTextObj.SetActive(true);

        if (healthText != null)
            healthText.gameObject.SetActive(false);
    }

    [ClientRpc]
    void RpcMarinesKilled()
    {
        winTextObj.GetComponent<Text>().text = "Alien wins!";
        winTextObj.SetActive(true);

        if (healthText != null)
            healthText.gameObject.SetActive(false);
    }
}