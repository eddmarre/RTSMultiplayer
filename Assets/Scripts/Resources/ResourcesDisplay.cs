using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
        [SerializeField] private TextMeshProUGUI resourcesText;

        private RTSPlayer _player;

        private void Update()
        {
                if (_player == null)
                {
                        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

                        if (_player != null)
                        {
                                Player_ClientOnResourcesUpdated(_player.GetResources());
                                
                                _player.ClientOnResourcesUpdated += Player_ClientOnResourcesUpdated;
                        }
                }
        }

        private void OnDestroy()
        {
                _player.ClientOnResourcesUpdated -= Player_ClientOnResourcesUpdated;
        }

        private void Player_ClientOnResourcesUpdated(int newResourceAmount)
        {
                resourcesText.text = $"Resources: {newResourceAmount}";
        }
}
