using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
        [SerializeField] private UnitSelectionHandler unitSelectionHandler;
        [SerializeField] private LayerMask layerMask;


        private void Start()
        {
            GameOverHandler.ClientOnGameOver+= GameOverHandler_ClientOnGameOver;
        }

        private void OnDestroy()
        {
            GameOverHandler.ClientOnGameOver -= GameOverHandler_ClientOnGameOver;
        }

        private void GameOverHandler_ClientOnGameOver(string winner)
        {
            enabled = false;
        }
        

        private void Update()
        {
            if (!Mouse.current.rightButton.wasPressedThisFrame) return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;


            if (hit.collider.TryGetComponent(out Targetable targetable))
            {
                if (targetable.hasAuthority)
                {
                    TryMove(hit.point);
                    return;
                }
                TryTarget(targetable);
                return;
            }
            
            TryMove(hit.point);
        }

        private void TryMove(Vector3 position)
        {
            foreach (Unit unit in unitSelectionHandler.SelectedUnits)
            {
                unit.GetUnitMovement().CmdMove(position);
            }
        }
        
        private void TryTarget(Targetable targetable)
        {
            foreach (Unit unit in unitSelectionHandler.SelectedUnits)
            {
                unit.GetTargeter().CmdSetTarget(targetable.gameObject);
            }
        }
}
