using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class PlClimbing : State
    {
        public PlClimbing(PlayerController playerController) : base(playerController) { }

        public override void NextStateCheck()
        {

        }

        public override void Jump()
        {
            ReleaseGrab();
            PlayerController.JumpAbility.Jump();
            PlayerController.JumpAbility.JumpCharge = 0;
        }

        public override void ToggleGrab()
        {
            ReleaseGrab();
        }

        public override void ReleaseGrab()
        {
            PlayerController.ClimbAbility.ReleaseGrab();
        }

        public override void Move(Vector3 inputDir)
        {
            PlayerController.ClimbAbility.ClimbMovement(inputDir);
        }

        public override void PhysicsUpdate()
        {
            StateMaster.PhysicsGravity(PlayerController);
        }
    }
}
