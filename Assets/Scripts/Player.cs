using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ObjectController;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("General")]
    public new PhotonView photonView;
    public Rigidbody2D Rigid;
    public SpriteRenderer Sprite;
    public GameObject PlayerCamera;
    public Text PlayerNameText;
    public GameObject Projectile_Prefab;
    public GameObject SpecialProjectile_Prefab;
    public GameObject AimingBox;
    public GameObject SpecialAimingBox;
    public Transform firePoint;

    [Space]

    [Header("Floats and Ints")]
    public float runSpeed = 8f;
    public float groundFriction = 20f;
    public float jumpForce = 3f;

    public float gravity = -25f;
    public float airStrafe = 5f;
    public float wallCling = 0f;

    public static float attackSpeed = 2f;

    [HideInInspector] public bool DisableInput = false;
    [HideInInspector] public bool canAttack = true;

    private float normalizedHorizontalSpeed = 0;
    private GeneralController _object;
    private Vector3 _velocity;
    private Vector3 latestPosition; // For Syncing

    protected MoveStick moveStick;
    protected AttackStick attackStick;
    protected SpecialStick specialStick;
	protected Joybutton jumpButton;

    [HideInInspector] public float attackTimer;
    [HideInInspector] public Image specialMeter;
    [HideInInspector] public GameObject SpecialCanvas;
    [HideInInspector] public GameObject SpecialMeterCanvas;

    protected PlayerCameraController cameraController;
    protected float cameraOffset = 1f;

    private bool AimingLastFrame = false;
    private bool AimingSpecialLastFrame = false;
    private Vector2 AimingLocation;

    private void Awake()
    {
        SetUpData_LOCAL_NONLOCAL();
    }

    private void SetUpData_LOCAL_NONLOCAL()
    {
        if (photonView.IsMine)
        {
            PlayerCamera.transform.SetParent(null, false);
            cameraController = PlayerCamera.GetComponent<PlayerCameraController>();
            cameraController.m_XOffset = cameraOffset;

            _object = GetComponent<GeneralController>();
            moveStick = FindObjectOfType<MoveStick>();
            attackStick = FindObjectOfType<AttackStick>();
            specialStick = FindObjectOfType<SpecialStick>();
		    jumpButton = FindObjectOfType<Joybutton>();

            SpecialCanvas = GameObject.Find("Canvas - Special");
            SpecialMeterCanvas = GameObject.Find("Canvas - SpecialMeter");
            specialMeter = GameObject.Find("Special Meter").GetComponent<Image>();
            SpecialCanvas.SetActive(false);

            PlayerNameText.text = PhotonNetwork.LocalPlayer.NickName;
            PlayerNameText.color = Color.green;
        }
        else
        {
            PlayerCamera.SetActive(false);

            PlayerNameText.text = photonView.Owner.NickName;
            PlayerNameText.color = Color.red;
        }
    }

    private void Update()
    {
        if (photonView.IsMine && DisableInput == false)
        {
            CheckInput();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, latestPosition, Time.deltaTime * runSpeed);
        }
    }

    private void CheckInput()
    {
        #region Gravity

        if(_object.isGrounded) _velocity.y = 0;
        _velocity.y += gravity * Time.deltaTime;

        #endregion

        #region Horizontal Movement

        normalizedHorizontalSpeed = moveStick.Horizontal;

        if (normalizedHorizontalSpeed > 0 || Input.GetKey(KeyCode.RightArrow)) // Right
        {
            if (normalizedHorizontalSpeed < .33) normalizedHorizontalSpeed = .5f;
            else normalizedHorizontalSpeed = 1;
            photonView.RPC("FlipSprite_RIGHT", RpcTarget.AllBuffered);
            cameraController.m_XOffset = cameraOffset;
        }
        else if (normalizedHorizontalSpeed < 0 || Input.GetKey(KeyCode.LeftArrow)) // Left
        {
            if (normalizedHorizontalSpeed > -.33) normalizedHorizontalSpeed = -.5f;
            else normalizedHorizontalSpeed = -1;
            photonView.RPC("FlipSprite_LEFT", RpcTarget.AllBuffered);
            cameraController.m_XOffset = -cameraOffset;
        }

        // Apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
        var smoothedMovementFactor = _object.isGrounded ? groundFriction : airStrafe;
        _velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor );

        // Platform Dropping
        if(_object.isGrounded && Input.GetKey(KeyCode.DownArrow))
        {
            _velocity.y *= 3f;
            _object.ignoreOneWayPlatformsThisFrame = true;
        }

        #endregion

        #region Vertical Movement

        if(_object.isGrounded && (jumpButton.pressed || Input.GetKey(KeyCode.Space))) //Input.GetKeyDown( KeyCode.UpArrow ) )
        {
            _velocity.y = Mathf.Sqrt( 2f * jumpForce * -gravity );
            //_animator.Play( Animator.StringToHash( "Jump" ) );
        }

        if(_object.collisionState.right)
        {
            _velocity.y = wallCling; 
            if (jumpButton.pressed || Input.GetKey(KeyCode.Space)) //Input.GetKeyDown(KeyCode.UpArrow))
            {
                _velocity.y = Mathf.Sqrt(2f * jumpForce * -gravity);
                _velocity.x = -Mathf.Sqrt(2f * jumpForce * -gravity);
                //_animator.Play( Animator.StringToHash( "Jump" ) );
            }
        }

        if(_object.collisionState.left)
        {
            _velocity.y = wallCling; 
            if (jumpButton.pressed || Input.GetKey(KeyCode.Space)) //Input.GetKeyDown(KeyCode.UpArrow))
            {
                _velocity.y = Mathf.Sqrt(2f * jumpForce * -gravity);
                _velocity.x = Mathf.Sqrt(2f * jumpForce * -gravity);
                //_animator.Play( Animator.StringToHash( "Jump" ) );
            }
        }

        #endregion

        // Apply calculated velocity to player
        _object.move(_velocity * Time.deltaTime);
        _velocity = _object.velocity; // Update calculated object velocity

        #region Attacking

        // Normal Attack
        if (attackTimer < 0) 
            canAttack = true;
        else
        {
            canAttack = false;
            attackTimer -= Time.deltaTime;
        }

        if (attackStick.IsPressed) 
        {
            AimingLocation = attackStick.Direction;
            if (Mathf.Abs(AimingLocation.x) >= .08 || Mathf.Abs(AimingLocation.y) >= .08)
            {            
                AimingLastFrame = true;
                AimingBox.SetActive(true);
                Aim(AimingLocation); 
            }
            else
            {
                AimingLastFrame = false;
                AimingBox.SetActive(false);
            }
        }
        else if (AimingLastFrame && (Mathf.Abs(AimingLocation.x) >= .08 || Mathf.Abs(AimingLocation.y) >= .08)) 
        {
            AimingBox.SetActive(false);
            if (canAttack) Attack(AimingLocation);
            AimingLastFrame = false;
        }

        // Special Attack
        if (specialStick.IsPressed)
        {
            AimingLocation = specialStick.Direction;
            if (Mathf.Abs(AimingLocation.x) >= .08 || Mathf.Abs(AimingLocation.y) >= .08)
            {
                AimingSpecialLastFrame = true;
                SpecialAimingBox.SetActive(true);
                AimSpecial(AimingLocation);
            }
            else
            {
                AimingSpecialLastFrame = false;
                SpecialAimingBox.SetActive(false);
            }
        }
        else if (AimingSpecialLastFrame && (Mathf.Abs(AimingLocation.x) >= .08 || Mathf.Abs(AimingLocation.y) >= .08)) 
        {
            SpecialAimingBox.SetActive(false);
            SpecialAttack(AimingLocation);
            AimingSpecialLastFrame = false;
        }

        #endregion
    }

    [PunRPC]
    private void FlipSprite_RIGHT() { Sprite.flipX = false; }

    [PunRPC]
    private void FlipSprite_LEFT() { Sprite.flipX = true; }

    private void Aim(Vector2 location)
    {
        var normal = new Vector2(1, 0);
        double angleInRadians = Math.Atan2(location.y, location.x) - Math.Atan2(normal.y, normal.x);
        float angle = (float) (angleInRadians * (180.0 / Math.PI));

        if (canAttack) AimingBox.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .2f);
        else AimingBox.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, .1f);

        AimingBox.transform.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void AimSpecial(Vector2 location)
    {
        var normal = new Vector2(1, 0);
        double angleInRadians = Math.Atan2(location.y, location.x) - Math.Atan2(normal.y, normal.x);
        float angle = (float) (angleInRadians * (180.0 / Math.PI));

        SpecialAimingBox.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .2f);

        AimingBox.transform.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void Attack(Vector2 location)
    {        
        GameObject obj = PhotonNetwork.Instantiate(Projectile_Prefab.name, new Vector2(firePoint.transform.position.x, firePoint.transform.position.y), Quaternion.identity, 0);
        obj.GetComponent<BallScript>().ParentObject = this.gameObject;
        obj.GetComponent<PhotonView>().RPC("SetDirection", RpcTarget.AllBuffered, location);   
        attackTimer = attackSpeed;
    }

    private void SpecialAttack(Vector2 location)
    {   
        GameObject obj = PhotonNetwork.Instantiate(SpecialProjectile_Prefab.name, new Vector2(firePoint.transform.position.x, firePoint.transform.position.y), Quaternion.identity, 0);
        obj.GetComponent<BallScript>().ParentObject = this.gameObject;
        obj.GetComponent<PhotonView>().RPC("SetDirection", RpcTarget.AllBuffered, location);  
        UpdateSpecialMeter(0);
    }

    public void UpdateSpecialMeter(float amount)
    {
        if (amount == 0) specialMeter.fillAmount = 0f;
        else specialMeter.fillAmount += amount/100f;

        if (specialMeter.fillAmount >= 1)
        {
            SpecialMeterCanvas.SetActive(false);
            SpecialCanvas.SetActive(true);
        }
        else
        {
            SpecialMeterCanvas.SetActive(true);
            SpecialCanvas.SetActive(false);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (photonView.IsMine)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            latestPosition = (Vector3) stream.ReceiveNext();
        }
    }
}