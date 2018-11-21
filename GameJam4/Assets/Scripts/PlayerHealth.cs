using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 100;

    private Player player;
    int health;

    private Text healthText;

    void Awake()
    {
        player = GetComponent<Player>();
    }

    void Start()
    {
        if (isLocalPlayer)
            healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<Text>();
    }

    [ServerCallback]
    void OnEnable()
    {
        health = maxHealth;
        RpcSetHealth(health);
    }

    [Server]
    public bool TakeDamage(int damage)
    {
        bool died = false;

        if (health <= 0)
            return died;

        health -= damage;
        died = health <= 0;

        RpcSetHealth(health);

        return died;
    }

    [ClientRpc]
    void RpcSetHealth(int currentHealth)
    {
        if (currentHealth <= 0)
            player.Die();

        health = currentHealth;

        healthText.text = "Health: " + currentHealth;
    }
}