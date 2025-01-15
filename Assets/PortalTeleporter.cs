using System;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    public GameObject view;
    [Header("Settings")]
    public float peripheralVision = 0.75f;

    private NonEuclideanGroup neg;
    private bool playerIsOverlapping;

    private void Start()
    {
        neg = transform.parent.parent.GetComponent<NonEuclideanGroup>();
    }

    private void Update()
    {
        Transform cam = PlayerController.playerCamera.transform;
        float camDot = Vector3.Dot(transform.up, cam.transform.forward);
        float posDot = Vector3.Dot(transform.up, cam.transform.position - transform.position);
        float dot = Vector3.Dot(transform.up, PlayerController.GetVelocity()) * 1000;

        if (camDot > peripheralVision || posDot < 0)
            view.SetActive(true);

        if (!playerIsOverlapping)
            return;

        if (neg.LastTeleport < 0.25f)
        {
            view.SetActive(true);
            return;
        }

        if (view.activeSelf == false)
            goto stop;

        if (dot > 0 && camDot < peripheralVision)
        {
            view.SetActive(false);
            goto stop;
        }

        if (camDot > 0.5f)
            goto stop;

        neg.Teleport();

        stop:
        playerIsOverlapping = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        playerIsOverlapping = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        playerIsOverlapping = false;
    }
}
