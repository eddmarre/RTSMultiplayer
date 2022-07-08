using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI[] playersNameTexts = new TextMeshProUGUI[4];

    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += RTSPlayer_AuthorityOnPartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += RTSPlayer_ClientOnInfoUpdated;
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= RTSPlayer_AuthorityOnPartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= RTSPlayer_ClientOnInfoUpdated;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    private void RTSPlayer_AuthorityOnPartyOwnerStateUpdated(bool value)
    {
        startGameButton.gameObject.SetActive(value);
    }

    private void RTSPlayer_ClientOnInfoUpdated()
    {
        List<RTSPlayer> players = ((RTSNetworkManager) NetworkManager.singleton).players;

        for (int i = 0; i < players.Count; i++)
        {
            playersNameTexts[i].text = players[i].GetDisplayName();
        }

        for (int i = players.Count; i < playersNameTexts.Length; i++)
        {
            playersNameTexts[i].text = "Waiting for Players...";
        }

        startGameButton.interactable = players.Count >= 2;
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }

    public void LeaveLobby()
    {
        //host
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }
}