using UnityEngine;

public class NonEuclideanGroup : MonoBehaviour
{
    [Header("Setup")]
    public Transform Portal1;
    public Transform Portal2;

    public PortalTeleporter Teleporter1;
    public PortalTeleporter Teleporter2;

    public Transform Camera;

    [Header("Settings")]

    public float LastTeleport;
    public float EnableDistance = 25;


    private bool active = false;
    private bool switched = false;
    private PlayerController player;


    private void Start()
    {
        player = PlayerController._singleton;
    }

    void Update()
    {
        LastTeleport += Time.deltaTime;
        SwitchToClosest();

        if (!active)
            return;

        EnableClosest();

        Transform playerCam = PlayerController.playerCamera.transform;
        Vector3 newPos = TransformPoint(playerCam.position);
        Vector3 newRot = TransformEuler(playerCam.eulerAngles);

        Camera.position = newPos;
        Camera.eulerAngles = newRot;
    }

    public void SwitchToClosest()
    {
        Vector3 PlayerCameraPosition = PlayerController.playerCamera.transform.position;
        float d1 = Vector3.Distance(Portal1.position, PlayerCameraPosition);
        float d2 = Vector3.Distance(Portal2.position, PlayerCameraPosition);

        SetActive(d1 < EnableDistance || d2 < EnableDistance);

        switched = d1 > d2;
    }

    public void EnableClosest()
    {
        if (switched)
        {
            Portal1.gameObject.SetActive(false);
            Portal2.gameObject.SetActive(true);
        }
        else
        {
            Portal1.gameObject.SetActive(true);
            Portal2.gameObject.SetActive(false);
        }
    }

    private void SetActive(bool newValue)
    {
        if (active == newValue)
            return;
        else
            Camera.gameObject.SetActive(newValue);
        active = newValue;
    }

    public void Teleport()
    {
        Transform playerTransform = player.transform;
        Transform camTransform = player.cameraObject.transform;

        Vector3 newPos = TransformPoint(playerTransform.position);
        Vector3 newRot = TransformEuler(camTransform.eulerAngles);
        Quaternion rotation = Quaternion.Euler(newRot - camTransform.eulerAngles);

        player.velocity = rotation * player.velocity;
        player.SetPosition(newPos);
        player.SetCameraRotation(newRot + new Vector3(0, 180, 0));

        switched = !switched;
        LastTeleport = 0;
    }

    Vector3 TransformPoint(Vector3 point)
    {
        Transform current = switched ? Portal2 : Portal1;
        Transform other = switched ? Portal1 : Portal2;
        Vector3 localPos = current.InverseTransformPoint(point);

        return other.TransformPoint(localPos);
    }

    Vector3 TransformEuler(Vector3 euler)
    {
        Transform current = switched ? Portal2 : Portal1;
        Transform other = switched ? Portal1 : Portal2;
        Vector3 localRot = euler - current.eulerAngles;

        return localRot + other.eulerAngles;
    }
}
