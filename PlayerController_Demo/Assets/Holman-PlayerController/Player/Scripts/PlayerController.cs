using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class PlayerController : MonoBehaviour
    {
        protected State _state;
        public State State => _state;

        public InputHandler InputHandler { get; private set; }
        public CharacterController CharacterController { get; private set; }
        [SerializeField] private LayerMask _groundMask = default;
        public LayerMask GroundMask => _groundMask;
        [SerializeField] private Transform _feet = default;
        public Transform Feet => _feet;
        [Tooltip("Maximum distance from feet to ground")]
        [SerializeField] private float _groundDetectionDistance = 0.2f;
        public float GroundDetectionDistance => _groundDetectionDistance;

        [SerializeField] private float _pushForce = 1.0f;
        public float PushForce => _pushForce;
        // Platform code is heavily inspired by SharpCoder
        // Link: https://sharpcoderblog.com/blog/unity-3d-character-controller-moving-platform-support
        private Transform SelectedPlatform;
        private Vector3 _localPlayerPlatformPoint;
        private Vector3 _globalPlayerPlatformPoint;

        //  Environment interaction
        [SerializeField] private float _maxAngle = 45f;
        public float MaxAngle => _maxAngle;
        [SerializeField] private float _wallAngle = 89f;
        public float WallAngle => _wallAngle;
        [SerializeField] private float _bounceSpeed = 0.5f;
        public float BounceSpeed => _bounceSpeed;
        [SerializeField] private float _slideSpeed = 2f;
        public float SlideSpeed => _slideSpeed;
        [SerializeField] private float _minimumStepOffset = 0.1f;
        [SerializeField] private float _maximumStepOffset = 0.3f;
        public bool IsGrounded { get; private set; }
        public bool IsOnStableGround { get; private set; }
        public bool IsOnWall { get; private set; }
        public bool IsOnSteepSlope { get; private set; }
        public bool IsOnMildSlope { get; private set; }
        public bool HasContactWithSteepSlope { get; private set; }
        public Vector3 SlopeNormal { get; set; }
        public Vector3 CollisionPoint { get; private set; }

        // Velocities. Be careful when modyfing these since they're core to the physics system
        private Vector3 _physicsVelocity;
        public Vector3 PhysicsVelocity => _physicsVelocity;
        private Vector3 _moveVelocity;
        public Vector3 MoveVelocity => _moveVelocity;
        private Vector3 _bounceVelocity;
        public Vector3 BounceVelocity => _bounceVelocity;
        private Vector3 _slideVelocity;
        public Vector3 SlideVelocity => _slideVelocity;
        public Vector3 CombinedVelocity => PhysicsVelocity + MoveVelocity + BounceVelocity + SlideVelocity;

        // Abilities
        public PlJump JumpAbility { get; private set; }
        public PlMove MoveAbility { get; private set; }
        public PlClimb ClimbAbility { get; private set; }

        public static bool debug = true; // Enables debug graphics in the scene view

        public void SetState(State state)
        {
            _state = state;
        }

        private void Awake()
        {
            InputHandler = GetComponent<InputHandler>();
            CharacterController = GetComponent<CharacterController>();
            _state = new PlWalking(this);

            // Find attached abilities
            JumpAbility = GetComponent<PlJump>();
            if (JumpAbility == null)
            {
                Debug.LogError("Jump ability not found. Add a PlJump component to the PlayerController.");
            }
            MoveAbility = GetComponent<PlMove>();
            if (MoveAbility == null)
            {
                Debug.LogError("Move ability not found. Add a PlMove component to the PlayerController.");
            }
            ClimbAbility = GetComponent<PlClimb>();
            if(ClimbAbility == null)
            {
                Debug.LogError("Climb ability not found. Add a PlClimb component to the PlayerController.");
            }
        }

        private void Update()
        {
            Cleanup();

            // Push the player towards the ground (this isn't gravity though!). Needed because of a quirk with Unity's character controllers.
            if (IsGrounded && PhysicsVelocity.y < 0)
            {
                _physicsVelocity.y = -0.1f;
            }

            RaycastHit feetCast = FeetCast();
            if(feetCast.collider == null)
            {
                SelectedPlatform = null;
            } else if(feetCast.collider.transform != SelectedPlatform)
            {
                SelectedPlatform = null;
            }

            if(SelectedPlatform != null)
            {
                Vector3 newGlobalPoint = SelectedPlatform.TransformPoint(_localPlayerPlatformPoint);
                Vector3 moveDirection = newGlobalPoint - _globalPlayerPlatformPoint;
                if (moveDirection.magnitude > 0.001f)
                {
                    CharacterController.Move(moveDirection);
                }
                if(SelectedPlatform)
                {
                    UpdateMovingPlatform();
                }
            }

            State.Move(InputHandler.InputDir);
            State.PhysicsUpdate();
        }

        private void UpdateMovingPlatform()
        {
            _globalPlayerPlatformPoint = transform.position;
            _localPlayerPlatformPoint = SelectedPlatform.InverseTransformPoint(transform.position);
        }

        /// <summary>
        /// Execute some checks and cleanup outdated states.
        /// </summary>
        private void Cleanup()
        {
            IsGrounded = GroundCheck();
            IsOnStableGround = StableGroundCheck();
            UpdateSlopeLimit();
            TryRemoveLedgeBounce();
            TryRemoveSlopeSlide();
            UpdateStepOffset();

            State.NextStateCheck();
        }

        /// <summary>
        /// Update the slope limit. This decreases jitter when moving near ledges.
        /// </summary>
        private void UpdateSlopeLimit()
        {
            if (IsOnStableGround || IsOnSteepSlope)
            {
                CharacterController.slopeLimit = MaxAngle;
            }
            else
            {
                CharacterController.slopeLimit = 90f;
            }
        }

        /// <summary>
        /// Update the step offset. This prevents the player from sliding up steep slopes.
        /// </summary>
        private void UpdateStepOffset()
        {
            if (HasContactWithSteepSlope)
            {
                if (IsOnWall)
                {
                    CharacterController.stepOffset = 0f;
                }
                else
                {
                    CharacterController.stepOffset = _minimumStepOffset;
                }
            }
            else
            {
                CharacterController.stepOffset = _maximumStepOffset;
            }
        }

        // Physics
        public void SetPhysicsVelocity(Vector3 velocity)
        {
            _physicsVelocity = velocity;
        }

        public void AddPhysicsVelocity(Vector3 velocity)
        {
            _physicsVelocity += velocity;
        }

        public void RemovePhysicsVelocity(Vector3 velocity)
        {
            _physicsVelocity -= velocity;
        }

        // Move
        public void SetMoveVelocity(Vector3 velocity)
        {
            _moveVelocity = velocity;
        }

        public void AddMoveVelocity(Vector3 velocity)
        {
            _moveVelocity += velocity;
        }

        public void RemoveMoveVelocity(Vector3 velocity)
        {
            _moveVelocity -= velocity;
        }

        /// <summary>
        /// Shortcut method for a common calculation. Does a call to CheckSphere around the player's lower part 
        /// that goes 0.01f unit outside its collider's skin.
        /// </summary>
        private bool LowerPartTouchingGroundCheck()
        {
            return Physics.CheckSphere(transform.position + Vector3.down * 0.5f, CharacterController.skinWidth + CharacterController.radius + 0.01f, GroundMask);
        }

        /// <summary>
        /// Shortcut method for a common calculation. Casts a ray from the feet down.
        /// </summary>
        public RaycastHit FeetCast()
        {
            RaycastHit feetCastHit;
            Physics.Raycast(Feet.position, Vector3.down, out feetCastHit, GroundDetectionDistance, GroundMask);
            return feetCastHit;
        }

        /// <summary>
        /// Remove ledge bounce if freefalling.
        /// </summary>
        private void TryRemoveLedgeBounce()
        {
            if (!LowerPartTouchingGroundCheck())
            {
                _bounceVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Remove slope slide if not on steep slope.
        /// </summary>
        private void TryRemoveSlopeSlide()
        {
            if (!IsOnSteepSlope)
            {
                _slideVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Returns true if the player is grounded in any way (includes climbing).
        /// </summary>
        private bool GroundCheck()
        {
            bool isGrounded;

            // True if on regular ground
            isGrounded = Physics.CheckSphere(Feet.position, GroundDetectionDistance, GroundMask);

            // True if on slope
            if (LowerPartTouchingGroundCheck() && !IsOnWall)
            {
                isGrounded = true;
            }

            // True if grabbing something
            if (ClimbAbility.Grabbing)
            {
                isGrounded = true;
            }

            return isGrounded;
        }

        /// <summary>
        /// Returns true if the player is standing on stable ground, e.g when on regular ground 
        /// but returns false when on steep slopes.
        /// </summary>
        public bool StableGroundCheck()
        {
            bool isOnStableGround;

            // True if on regular ground
            isOnStableGround = Physics.CheckSphere(Feet.position, GroundDetectionDistance, GroundMask);

            // True if on mild slope
            if (LowerPartTouchingGroundCheck() && IsOnMildSlope) // Fix offset
            {
                isOnStableGround = true;
            }

            // False if on steep slope or wall
            if (IsOnSteepSlope || IsOnWall)
            {
                isOnStableGround = false;
            }

            return isOnStableGround;
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if(hit.transform.TryGetComponent(out Rigidbody hitBody))
            {
                if (hit.moveDirection.y > -0.3f)
                {
                    Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
                    hitBody.velocity = (pushDir * PushForce) / hitBody.mass;
                }
            }

            if(hit.moveDirection.y < -0.9f && hit.normal.y > 0.41f)
            {
                if (SelectedPlatform != hit.collider.transform)
                {
                    SelectedPlatform = hit.collider.transform;
                    UpdateMovingPlatform();
                }
            } else
            {
                SelectedPlatform = null;
            }

            CollisionPoint = hit.point;

            // Store the result of a raycast down from the feet
            RaycastHit feetCastHit = FeetCast();

            // Calculate the angle of the surface hit by the raycast
            LayerMask layer = LayerMask.GetMask(LayerMask.LayerToName(hit.gameObject.layer));
            SlopeNormal = GetRaycastNormal(Feet.position, hit.point - Feet.position, layer);
            float slopeAngle = Mathf.Round(10f * Vector3.Angle(SlopeNormal, Vector3.up)) / 10f;

            // Calculate the angle of the surface that the player currently collides with
            float hitSurfaceSlopeAngle = Mathf.Round(10f * Vector3.Angle(hit.normal, Vector3.up)) / 10f;

            // Interpret the slope and describe it
            IsOnMildSlope = OnMildSlopeCheck(slopeAngle);
            IsOnSteepSlope = OnSteepSlopeCheck(slopeAngle);
            HasContactWithSteepSlope = OnSteepSlopeCheck(hitSurfaceSlopeAngle);
            IsOnWall = OnWallCheck(slopeAngle);

            // Determine the direction that the player would slide if they stood on the surface
            float angle = Vector3.Angle(Vector3.up, SlopeNormal) + 90;
            Vector3 perpendicularRotationAxis = Vector3.Cross(Vector3.up, SlopeNormal);
            Vector3 direction = Vector3.up;
            // Rotate {direction} {angle} degrees around {perpendicularRotationAxis}
            Vector3 slideDirection = Quaternion.AngleAxis(angle, perpendicularRotationAxis) * direction;

            // Calculate if the player is moving towards the point of collision.
            Vector3 worldMoveDirection = transform.TransformDirection(new Vector3(InputHandler.InputDir.x, 0, InputHandler.InputDir.y));
            bool isMovingTowardsPoint = Vector3.Dot(worldMoveDirection, hit.point - Feet.position) > 0;

            if (slopeAngle > MaxAngle && slopeAngle < 90f)
            {
                TrySlopeSlide(hit, feetCastHit, slideDirection);
            }
            else
            {
                TryLedgeBounce(hit, feetCastHit, isMovingTowardsPoint);
            }
        }

        /// <summary>
        /// Push the player away from the ledge if it's not directly above it.
        /// </summary>
        private void TryLedgeBounce(ControllerColliderHit hit, RaycastHit feetCastHit, bool isMovingTowardsPoint)
        {
            if (feetCastHit.collider == null && hit.point.y < transform.position.y - 0.5f && (!isMovingTowardsPoint || IsOnSteepSlope))
            {
                _bounceVelocity += (Feet.position - hit.point).normalized * BounceSpeed;
            }
            else
            {
                _bounceVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Slide player down slope.
        /// </summary>
        private void TrySlopeSlide(ControllerColliderHit hit, RaycastHit feetCastHit, Vector3 slideDirection)
        {
            if (feetCastHit.collider == null && hit.point.y < transform.position.y - 0.5f && !IsOnMildSlope)
            {
                _slideVelocity = Vector3.Lerp(SlideVelocity, slideDirection * SlideSpeed, 0.1f * Time.deltaTime);
            }
            else
            {
                _slideVelocity = Vector3.zero;
            }
        }

        private bool OnWallCheck(float slopeAngle)
        {
            return slopeAngle >= WallAngle;
        }

        private bool OnSteepSlopeCheck(float slopeAngle)
        {
            return slopeAngle > MaxAngle && slopeAngle < WallAngle;
        }

        private bool OnMildSlopeCheck(float slopeAngle)
        {
            return slopeAngle > 0.1f && slopeAngle < MaxAngle;
        }

        /// <summary>
        /// Get exact normal of a surface.
        /// </summary>
        public Vector3 GetRaycastNormal(Vector3 origin, Vector3 dir, LayerMask mask)
        {
            RaycastHit hit;
            Physics.Raycast(origin, dir, out hit, 10f, mask);

            if (hit.collider != null)
            {
                return hit.normal;
            }
            else
            {
                return Vector3.up;
            }
        }
    }
}
