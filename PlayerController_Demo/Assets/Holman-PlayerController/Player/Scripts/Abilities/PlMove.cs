using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class PlMove : MonoBehaviour
    {
        private PlayerController _playerController;

        [SerializeField] private float _walkSpeed = 5;
        public float WalkSpeed => _walkSpeed;
        [SerializeField] private float _runSpeed = 10;
        public float RunSpeed => _runSpeed;

        private float _speed;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        /// <summary>
        /// Set the player's speed to running speed.
        /// </summary>
        public void SetRunSpeed()
        {
            _speed = _runSpeed;
        }

        /// <summary>
        /// Set the player's speed to walking speed.
        /// </summary>
        public void SetWalkSpeed()
        {
            _speed = _walkSpeed;
        }

        // Simple input movement.
        public void InputMovement(Vector3 inputDir)
        {
            // Move smoothly down smooth slopes
            Vector3 direction = transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y)).normalized;
            Vector3 normal = _playerController.SlopeNormal;

            // Allow the player to smoothly walk up and down mild slopes
            if (_playerController.IsOnMildSlope)
            {
                direction = Quaternion.FromToRotation(transform.up, normal) * direction;
            }

            // Allow the player to smoothly walk down steep slopes, but not up them
            if (_playerController.IsOnSteepSlope)
            {
                //direction = Quaternion.FromToRotation(transform.up, normal) * direction;
                //direction.y = Mathf.Min(direction.y, 0);
            }

            /* The following code should be needed to prevent some quirkyness, but for some reason the problem it aims to solve never appears.
            If you have any problems with extremely steep (ca 85+ degree) slopes, try uncommenting this.*/
            /*if(_playerController.IsOnWall || _playerController.IsOnSteepSlope)
            {
                Vector3 collisionDirection = transform.position - _playerController.CollisionPoint;
                collisionDirection = transform.InverseTransformDirection(collisionDirection);
                collisionDirection.y = 0;
                collisionDirection.Normalize();
                float alignment = Vector3.Dot(direction, collisionDirection);
                direction -= Mathf.Max(alignment, 0) * collisionDirection;
            }*/

            _playerController.SetMoveVelocity(direction * _speed);
        }
    }
}
