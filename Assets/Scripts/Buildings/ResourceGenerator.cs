using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private int resourcesPerInterval = 10;
    [SerializeField] private float interval = 2f;


    private float _timer;
    private RTSPlayer _player;

    [ServerCallback]
    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            _player.SetResource(_player.GetResources() + resourcesPerInterval);
            _timer += interval;
        }
    }

    public override void OnStartServer()
    {
        _timer = interval;
        _player = connectionToClient.identity.GetComponent<RTSPlayer>();

        health.ServerOnDie += Health_ServerOnDie;
        GameOverHandler.ServerOnGameOver += GameOverHandler_ServerOnGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= Health_ServerOnDie;
        GameOverHandler.ServerOnGameOver -= GameOverHandler_ServerOnGameOver;
    }

    private void Health_ServerOnDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void GameOverHandler_ServerOnGameOver()
    {
        enabled = false;
    }
}