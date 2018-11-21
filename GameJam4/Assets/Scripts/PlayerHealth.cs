using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 100;

    private Player player;
    int health;

    void Awake()
    {
        player = GetComponent<Player>();
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

        RpcTakeDamage(health);

        return died;
    }

    [ClientRpc]
    void RpcTakeDamage(int currentHealth)
    {
        if (currentHealth <= 0)
            player.Die();

        health = currentHealth;
    }
}