using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class PlJump : MonoBehaviour
    {
        private PlayerController _playerController;

        [SerializeField] private float _jumpForce = 4;
        public float JumpForce => _jumpForce;

        [HideInInspector] public float JumpCharge;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public void Jump()
        {
            float verticalComponent = _jumpForce;
            _playerController.SetPhysicsVelocity(new Vector3(_playerController.PhysicsVelocity.x, verticalComponent, _playerController.PhysicsVelocity.z));
        }
    }
}
