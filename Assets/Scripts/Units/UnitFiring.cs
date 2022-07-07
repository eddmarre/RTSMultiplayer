using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float roatationSpeed = 20f;

    private float lastFireTime;


    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        
        if(target==null) return;
        
        if (!CanFireAtTarget()) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, roatationSpeed * Time.deltaTime);

        if (Time.time > 1 / fireRate + lastFireTime)
        {
            Quaternion projectileRotation =
                Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);

            GameObject projectileGO = Instantiate(projectilePrefab, projectileSpawnPoint.position,
                projectileRotation);
            NetworkServer.Spawn(projectileGO, connectionToClient);
            lastFireTime = Time.time;
        }
    }


    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <=
               fireRange * fireRange;
    }
}