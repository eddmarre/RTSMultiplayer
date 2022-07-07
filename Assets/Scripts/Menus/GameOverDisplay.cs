using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDispalyParent;
    [SerializeField] private TextMeshProUGUI winnerNameText;
    private void Start()
    {
        GameOverHandler.ClientOnGameOver += GameOverHandler_ClientOnGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= GameOverHandler_ClientOnGameOver;
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }
    private void GameOverHandler_ClientOnGameOver(string winner)
    {
        winnerNameText.text = $"{winner} has won!";
        
        gameOverDispalyParent.SetActive(true);
    }
}
