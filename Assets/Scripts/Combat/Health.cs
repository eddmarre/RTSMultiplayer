using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action<int, int> ClientOnHealthUpdated;

    public event Action ServerOnDie;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += UnitBase_ServerOnPlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= UnitBase_ServerOnPlayerDie;
    }

    [Server]
    private void UnitBase_ServerOnPlayerDie(int connectionID)
    {
        if(connectionToClient.connectionId!=connectionID) return;
        
        DealDamage(currentHealth);
    }
    [Server]
    public void DealDamage(int damgeAmount)
    {
        if (currentHealth == 0) return;

        currentHealth = Mathf.Max(currentHealth - damgeAmount, 0);

        if (currentHealth != 0) return;


        ServerOnDie?.Invoke();
    }

    #endregion


    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}