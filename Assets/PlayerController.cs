using UnityEngine;
public enum MovementType { SLIDING, RUNNING, DASHING, JUMPING, FALLING }
public enum JumpType { NONE, DOUBLE, AIRTIME }

public class PlayerController : MonoBehaviour
{
    public static Camera playerCamera;
    public static Transform playerTransform;
    public static Vector3 GetVelocity() { return _singleton.velocity; }

    public static PlayerController _singleton;

    [Header("Setup")]
    public CharacterController CharacterController;
    public Transform CameraRig;
    public Transform CameraPitch;
    public Camera cameraObject;

    [Header("Settings")]
    public float RunSpeed = 100;
    public JumpType jumpType = JumpType.NONE;
    private MovementType movementType = MovementType.RUNNING;

    [Space()]
    public float JumpPower = 6f;
    public float GroundPoundPower = 20f;
    public float DashPower = 30f;
    public float SlidePower = 30f;
    public float AirControll = .5f;
    public float AirTimeMultiplier = .5f;

    [Header("Debug")]
    public Vector3 velocity;
    public float targetHeight = 1.8f;
    private float height = 1.8f;

    private float lastGrounded;
    private float lastJumpInput = 1;
    private float lastSlideInput = 1;
    private float lastDashInput = 1;

    private int jumpCount;
    
    void Start()
    {
        _singleton = this;
        playerCamera = cameraObject;
        playerTransform = transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        ProcessMovement();
        CharacterController.Move(velocity * Time.fixedDeltaTime);

        SetHeight(Mathf.Lerp(height, targetHeight, Mathf.Pow(Time.fixedDeltaTime, 0.5f)));
    }

    void Update()
    {
        ProcessInputs();
        ProcessCameraMovement();

        // for debugging purposes
        // <<<<
        if (Input.GetKey(KeyCode.LeftAlt))
            Application.targetFrameRate = 5;
        else
            Application.targetFrameRate = -1;
        /// <<<<
    }

    ////////// Movement methods

    private void ProcessInputs()
    {
        if (lastDashInput > 1 && Input.GetKeyDown(KeyMap.Dash))
            Dash();

        if (Input.GetKeyDown(KeyMap.Slide) && lastSlideInput > 1)
            lastSlideInput = 0;

        if (Input.GetKeyDown(KeyMap.Jump))
            lastJumpInput = 0;

        if (lastJumpInput < 0.1 && lastGrounded < 0.1)
            Jump();
    }

    private void ProcessMovement()
    {
        Vector2 input = GetMovementInput();
        Vector3 movement = input.x * CameraRig.right + input.y * CameraRig.forward;
        movement *= RunSpeed * Time.fixedDeltaTime;

        lastDashInput += Time.fixedDeltaTime;
        lastSlideInput += Time.fixedDeltaTime;
        lastJumpInput += Time.fixedDeltaTime;

        if (lastGrounded <= 0 && movementType == MovementType.RUNNING)
            ProcessGroundMovement(movement);
        else
            ProcessAirMovement(movement);

        if (IsGrounded())
            lastGrounded = 0;
        else
            lastGrounded += Time.fixedDeltaTime;

        if(movementType != MovementType.SLIDING)
        {
            targetHeight = 1.8f;
            if (lastGrounded == 0 && lastDashInput > 0.25f)
                movementType = MovementType.RUNNING;
        }
        else if(lastSlideInput > 0.8f)
            movementType = MovementType.RUNNING;
    }

    private void ProcessAirMovement(Vector3 movement)
    {
        movement *= AirControll;
        if (lastDashInput > 0.1)
            movement += GetGravitationForce();
        velocity += movement;

        FrictionForce(Mathf.Pow(Time.fixedDeltaTime, 1.1f - AirControll));

        if (movementType == MovementType.SLIDING)
            return;

        if (lastDashInput > 0.25f && Vector3.Dot(Physics.gravity, velocity) > 0)
            movementType = MovementType.FALLING;

        if (lastSlideInput < 0.1)
        {
            if (GetDistanceToGround() < 0.5f)
                Slide();
            else
                GroundPound();
        }
    }

    private void ProcessGroundMovement(Vector3 movement)
    {
        velocity += movement;

        FrictionForce(Mathf.Pow(Time.fixedDeltaTime, 0.5f));

        if(movementType == MovementType.RUNNING)
        {
            jumpCount = 0;
            if (lastSlideInput < 0.1f)
                Slide();
        }
    }

    private Vector3 GetGravitationForce()
    {
        if (jumpType == JumpType.AIRTIME && Input.GetKey(KeyMap.Jump))
            return Physics.gravity * Time.fixedDeltaTime * AirTimeMultiplier;

        if (jumpType == JumpType.DOUBLE && jumpCount < 1 && Input.GetKeyDown(KeyMap.Jump))
        {
            Jump();
        }

        return Physics.gravity * Time.fixedDeltaTime;
    }

    private void ProcessCameraMovement()
    {
        float xMovement = Input.GetAxis("Mouse X") * Settings.MouseSensitivity;
        float yMovement = Input.GetAxis("Mouse Y") * Settings.MouseSensitivity;

        RotateCamera(new Vector3(-yMovement, xMovement, 0));
    }

    ////////// Private methods

    private void SetHeight(float newHeight)
    {
        height = newHeight;

        CharacterController.height = height;
        CameraRig.localPosition = new Vector3(0, height / 2 + 0.7f, 0);
    }

    private void FrictionForce(float friction)
    {
        Vector3 XZ = new Vector3(velocity.x, 0, velocity.z);
        XZ = Vector3.Lerp(XZ, Vector3.zero, friction);

        velocity = new Vector3(XZ.x, velocity.y, XZ.z);
    }

    public bool IsGrounded()
    {
        if (GetDistanceToGround() < 0.1f)
            return true;
        return CharacterController.isGrounded;

        // Courtesy of Aleksandar Karaychev
        Application.Quit();
    }

    private void Slide()
    {
        movementType = MovementType.SLIDING;

        Vector3 camVector = cameraObject.transform.forward;
        Vector3 direction = new Vector3(camVector.x, 0, camVector.z).normalized;
        velocity = direction * SlidePower;

        targetHeight = 1;
    }

    private void Jump()
    {
        if (lastDashInput < 0.25)
            return;
        velocity.y = JumpPower;
        movementType = MovementType.JUMPING;
        lastJumpInput = 1;
        jumpCount++;
    }

    private void GroundPound()
    {
        velocity = new Vector3(0, -GroundPoundPower, 0);
        lastSlideInput += 50;
    }

    private void Dash()
    {
        lastDashInput = 0;

        Vector3 camVector = cameraObject.transform.forward;
        Vector3 direction = new Vector3(camVector.x, 0, camVector.z).normalized;
        velocity = direction * DashPower;
        movementType = MovementType.DASHING;
    }

    private Vector2 GetMovementInput()
    {
        Vector2 result = new Vector2();

        if (Input.GetKey(KeyMap.MoveForward))
            result.y += 1;
        if (Input.GetKey(KeyMap.MoveLeft))
            result.x -= 1;
        if (Input.GetKey(KeyMap.MoveBackward))
            result.y -= 1;
        if (Input.GetKey(KeyMap.MoveRight))
            result.x += 1;

        return result.normalized;
    }

    private float GetDistanceToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up * -1, out hit))
            return hit.distance;
        return float.MaxValue;
    }

    ////////// Public methods
    public void RotateCamera(Vector3 euler)
    {
        CameraPitch.localEulerAngles += new Vector3(euler.x, 0, 0);
        CameraRig.localEulerAngles += new Vector3(0, euler.y, 0);
        cameraObject.transform.localEulerAngles += new Vector3(0, 0, euler.z);
    }

    public void SetCameraRotation(Vector3 euler)
    {
        CameraPitch.localEulerAngles = new Vector3(euler.x, 0, 0);
        CameraRig.localEulerAngles = new Vector3(0, euler.y, 0);
    }

    public void SetPosition(Vector3 pos)
    {
        CharacterController.enabled = false;
        transform.position = pos;
        CharacterController.enabled = true;
    }
}
