using UnityEngine;

public class NonEuclideanGroup : MonoBehaviour
{
    [Header("Setup")]
    public Vector3 testvar;
    public Transform Portal1;
    public Transform Portal2;

    public PortalTeleporter Teleporter1;
    public PortalTeleporter Teleporter2;

    public new Camera camera;

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

        Transform current = switched ? Portal2 : Portal1;
        Transform other = switched ? Portal1 : Portal2;
        Transform playerCamera = player.cameraObject.transform;
        Transform portalCamera = camera.transform;

        Vector3 relativePosition = current.InverseTransformPoint(playerCamera.position);
        Quaternion relativeRotation = Quaternion.Inverse(current.rotation) * playerCamera.rotation;

        Vector3 portalCameraPosition = other.TransformPoint(relativePosition);
        Quaternion portalCameraRotation = other.rotation * relativeRotation;

        portalCamera.position = portalCameraPosition;
        portalCamera.rotation = portalCameraRotation;
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
            camera.gameObject.SetActive(newValue);
        active = newValue;
    }

    public void Teleport()
    {
        Transform current = switched ? Portal2 : Portal1;
        Transform other = switched ? Portal1 : Portal2;

        player.SetCameraRotation(camera.transform.eulerAngles + new Vector3(0, 180, 0));
        player.SetPosition(camera.transform.position - player.CameraRig.localPosition);

        Vector3 localVelocity = current.InverseTransformDirection(player.velocity);
        Vector3 newVelocity = other.TransformDirection(localVelocity);

        newVelocity.y *= 0.5f;

        player.velocity = newVelocity;
    }
}
