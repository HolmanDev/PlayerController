using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class PlWalking : State
    {
        public PlWalking(PlayerController playerController) : base(playerController) { }

        public override void NextStateCheck()
        {
            if (!PlayerController.IsGrounded)
            {
                // !! Set state to falling
            }
        }

        public override void Jump()
        {
            PlayerController.JumpAbility.Jump();
            PlayerController.JumpAbility.JumpCharge = 0;
        }

        public override void Grab()
        {
            PlayerController.ClimbAbility.Grab();
        }

        public override void ToggleGrab()
        {
            Grab();
        }

        public override void Move(Vector3 inputDir)
        {
            PlayerController.MoveAbility.InputMovement(inputDir);
        }

        public override void PhysicsUpdate()
        {
            StateMaster.PhysicsGravity(PlayerController);
            StateMaster.PhysicsVelocityBounceSlide(PlayerController);
        }
    }
}
