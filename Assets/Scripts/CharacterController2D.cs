using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class CharacterController2D : MonoBehaviour
{
	private GameManager_ _manager; // Make sure GameManager is set up in the scene

    #region PLAYER_MOVEMENT
	// ========= MOVEMENT ========== //
	[Header("Movement Settings")]
	public bool EnableMovement = true;

	private float _moveInput = 0;
    [ConditionalHide("EnableMovement", true)]
	[Tooltip("The speed at which the player moves horitontally")]
	[Range(0, 3)] public float MoveSpeed;

	[ConditionalHide("EnableMovement", true)]
	[Tooltip("The acceleration of the player (speed smoothing)")]
	[Range(0, 0.3f)] public float MovementSmoothing = .05f;

	// ========= GROUND & AIR STATE ========== //
	[Header("Ground & Air check")]

	[ConditionalHide("EnableMovement", true)]
	[Tooltip("Can the player move whilst in the air?")]
	public bool AirControl = true;

	[ConditionalHide("EnableMovement", true)]
	[Tooltip("Which object layer will the player collide with?")]
	public LayerMask ObstacleLayer;

	[ConditionalHide("EnableMovement", true)]
	[Tooltip("The feet position of the player, where raycast will detect collision for ground.")]
	public Transform GroundCheckPosition;

	[ConditionalHide("EnableMovement", true)]
	[Tooltip("The head position of the player, where raycast will detect collision for ceil.")]
	public Transform CeilingCheckPosition;

	[ConditionalHide("EnableMovement", true)]
	[Tooltip("The radius of raycast checking, the smaller the more accurate.")]
	public float ObstacleCheckRadius = .2f;

	private bool _isGrounded;
	private bool _hitCeiling;
	private Rigidbody2D _rb;
	private bool _isFacingRight = true;
	private Vector3 _velocityVector = Vector3.zero;
	#endregion

	#region PLAYER_JUMPING
	// ========= JUMPING ========== //
	[Header("Jumping")]
	public bool EnableJumping = true;

	[ConditionalHide("EnableJumping", true)]
	[Tooltip("The magnitude of the force to propel the player upward.")]
	[Range(10f, 100f)] public float JumpForce = 20f;

	[ConditionalHide("EnableJumping", true)]
	[Tooltip("After this amount of time, the player will have to descend.")]
	public float MaxJumpTime = 1f;

	[ConditionalHide("EnableJumping", true)]
	[Tooltip("After jumping this height, the player will have to descend.")]
	public float MaxJumpHeight = 3f;

	private float _jumpTimeCounter = 0f;
	private float _oldJumpHeight;
	private bool _isJumping = false;
    #endregion

    #region WALL_CLIMBING

    // ========= WALL CLIMBING ========== //
    [Header("Wall climbing")]
	public bool EnableWallClimbing = true;

	[ConditionalHide("EnableWallClimbing", true)]
	[Tooltip("The radius of wall checking, the smaller the more accurate.")]
	public float WallCheckRadius = .2f;

	[ConditionalHide("EnableWallClimbing", true)]
	[Tooltip("Side position of the player, where raycast will detect whether you have touched the wall")]
	public Transform WallCheckPosition;

	[ConditionalHide("EnableWallClimbing", true)]
	[Tooltip("The down sliding speed while you are grabbing onto walls")]
	public float WallSlidingSpeed;

	[ConditionalHide("EnableWallClimbing", true)]
	[Tooltip("After this time, the player will start descending.")]
	public float MaxWallJumpTime;

	[ConditionalHide("EnableWallClimbing", true)]
	[Tooltip("Horizontal force applied to jumping against walls")]
	public float WallForceJumpX;

	[ConditionalHide("EnableWallClimbing", true)]
	[Tooltip("Vertical force applied to jumping against walls")]
	public float WallForceJumpY;

	[ConditionalHide("EnableWallClimbing", true)]
	[Tooltip("Which object layer will define 'wall'? (May be same as Obstacle layer)")]
	public LayerMask WallLayer;

	private bool _isTouchingFront;
	private bool _wallSliding;
	private bool _wallJumping;
	private bool _canSlide = true;

    #endregion

    #region DASHING
    // ========= DASHING ========== //
    [Header("Dashing")]

	public bool EnableDashing = true;

	[ConditionalHide("EnableDashing", true)]
	[Tooltip("The force to propel the player in the direction he is moving")]
	public float DashPower;
	[ConditionalHide("EnableDashing", true)]
	[Tooltip("In this amount of time after dashing, the player cannot continue dashing")]
	public float DashDelay;
	[ConditionalHide("EnableDashing", true)]
	[Tooltip("The shadows effect of the player, should be a GameObject containing child transforms with each player shadow")]
	public GameObject DashEffect;

	private bool _canDash = true;
	#endregion

	#region LADDER_CLIMBING
	// ========= LADDER CLIMBING ========== //
	[Header("Ladder Climbing")]

	public bool EnableLadderClimbing = true;

	[ConditionalHide("EnableLadderClimbing", true)]
	[Tooltip("The vertical speed of the player whilst on ladder objects")]
	public float ClimbingSpeed;
	[ConditionalHide("EnableLadderClimbing", true)]
	[Tooltip("Which object layer will define 'Ladder'")]
	public LayerMask LadderLayer;

	private bool _isClimbing;
	private float _moveLadder;
    #endregion

    #region PARTICLE_EFFECT
    // ========= PARTICLE EFFECT ========== //
    [Header("Particle Effect")]
	[Tooltip("Particle spawned while moving or jumping")]
	public ParticleSystem MoveParticleFeet;
    #endregion

    private void Start()
    {
		_rb = GetComponent<Rigidbody2D>();
		_manager = GameManager_.Instance;
	}

    private void Update()
    {
		if (!_manager.IsRunningGame) return;
		if (EnableJumping) Jump();
		if (EnableWallClimbing) WallClimb();
		if (EnableDashing) Dash();
	}

	private void FixedUpdate()
	{
		if (!_manager.IsRunningGame) return;

		GroundCheck();

		if (EnableMovement) Move();
		if (EnableWallClimbing) ClimbLadder();
	}

	#region MOVE
	public void Move()
	{
		_moveInput = Input.GetAxisRaw("Horizontal") * MoveSpeed;
		if (_isGrounded || (AirControl && !_wallSliding))
		{
			Vector3 targetVelocity = new Vector2(_moveInput * 10f, _rb.velocity.y);
			_rb.velocity = Vector3.SmoothDamp(_rb.velocity, targetVelocity, ref _velocityVector, MovementSmoothing);

			if ((_moveInput > 0 && !_isFacingRight) || (_moveInput < 0 && _isFacingRight))
			{
				MoveParticleFeet.Play();
				_isFacingRight = !_isFacingRight;

				Vector3 characterScale = transform.localScale;
				characterScale.x = _isFacingRight ? 1 : -1;
				transform.localScale = characterScale;
			}
		}
	}
	#endregion

	#region JUMP

	private void Jump()
    {
		if (_isGrounded) _oldJumpHeight = transform.position.y;
		if ((_isGrounded || _wallSliding) && Input.GetKeyDown(KeyCode.Space))
		{
			if (_wallSliding)
			{
				StartCoroutine(WallJump());
			}
			_isJumping = true;
			_jumpTimeCounter = MaxJumpTime;
			ApplyJump(JumpForce);
		}

		if (Input.GetKey(KeyCode.Space) && _isJumping)
		{
			if (_jumpTimeCounter > 0 && transform.position.y - _oldJumpHeight <= MaxJumpHeight)
			{
				ApplyJump(JumpForce);
				_jumpTimeCounter -= Time.deltaTime;
			}
			else
			{
				_isJumping = false;
			}
		}

		if (Input.GetKeyUp(KeyCode.Space) || _hitCeiling) _isJumping = false;
	}

	public void ApplyJump(float force)
	{
		MoveParticleFeet.Play();
		_rb.velocity = new Vector2(_rb.velocity.x, force);
	}
	IEnumerator TransitionToGroundless()
	{
		yield return new WaitForSeconds(0.1f);
		_isGrounded = false;
	}
	#endregion

	#region WALL_CLIMB
	private void WallClimb()
    {
		if (_isTouchingFront && !_isGrounded && _moveInput != 0 && _canSlide && !_isClimbing)
		{
			_wallSliding = true;
		}
		else _wallSliding = false;

		if (_wallSliding)
		{
			_rb.velocity = new Vector2(_rb.velocity.x, Mathf.Clamp(_rb.velocity.y, -WallSlidingSpeed, float.MaxValue));
		}
	}
	IEnumerator WallJump()
	{
		_canSlide = false;
		if ((_moveInput > 0 && _isFacingRight) || (_moveInput < 0 && !_isFacingRight))
			_rb.velocity = new Vector2(WallForceJumpX * -_moveInput, WallForceJumpY);
		yield return new WaitForSeconds(MaxWallJumpTime);
		_canSlide = true;
	}
	#endregion

	#region DASH
	private void Dash()
    {
		if (Input.GetKeyDown(KeyCode.Z) && (_isGrounded || (AirControl && !_wallSliding)) && _canDash && !_isClimbing)
		{
			StartCoroutine(DashProcess());
		}
	}
	IEnumerator DashProcess()
	{
		DashEffect.SetActive(true);
		_canDash = false;
		_rb.velocity = new Vector2(DashPower * transform.localScale.x, _rb.velocity.y);
		MoveParticleFeet.Play();
		yield return new WaitForSeconds(0.1f);
		DashEffect.SetActive(false);
		yield return new WaitForSeconds(DashDelay);
		_canDash = true;
	}
	#endregion

	#region CLIMB_LADDER
	private void ClimbLadder()
	{
		RaycastHit2D hitInfo = Physics2D.Raycast(GroundCheckPosition.position, Vector2.down, 0.5f, LadderLayer);
		if (hitInfo.collider != null)
		{
			_canSlide = false;
			_wallSliding = false;
			// print("Not null");
			if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
			{
				_isClimbing = true;
			}
		}
		else { _isClimbing = false; _canSlide = true; }

		if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.3f) _isClimbing = false;

		if (_isClimbing)
		{
			_moveLadder = Input.GetAxisRaw("Vertical");
			_rb.velocity = new Vector2(_rb.velocity.x, _moveLadder * ClimbingSpeed);
			_rb.gravityScale = 0;
		}
	}
    #endregion

    #region GROUND_CHECK
	private void GroundCheck()
    {
		_hitCeiling = Physics2D.OverlapCircle(CeilingCheckPosition.position, ObstacleCheckRadius, ObstacleLayer);
		_isTouchingFront = Physics2D.OverlapCircle(WallCheckPosition.position, WallCheckRadius, WallLayer);

		bool wasGrounded = _isGrounded;
		bool groundCheck = Physics2D.OverlapCircle(GroundCheckPosition.position, ObstacleCheckRadius, ObstacleLayer);
		if (groundCheck)
		{
			_isGrounded = true;
			if (!wasGrounded)
				MoveParticleFeet.Play();
		}
		else StartCoroutine(TransitionToGroundless());
	}
    #endregion
}