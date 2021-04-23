using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    // This class is like a template for all other states. That is why they all inherit of it.
    public abstract class State
    {
        protected PlayerController PlayerController;
        protected InputHandler.KeyBindings KeyBindings;

        public State(PlayerController playerController)
        {
            PlayerController = playerController;
            KeyBindings = PlayerController.InputHandler.KeyBindingSettings;
        }

        // Everything below can be overwritten in individual states. This is just a template with some default values.

        #region actions
        // General
        public virtual void NextStateCheck() { }

        // Movement
        public virtual void Move(Vector3 inputDir) { }
        public virtual void RunSpeed() { PlayerController.MoveAbility.SetRunSpeed(); }
        public virtual void WalkSpeed() { PlayerController.MoveAbility.SetWalkSpeed(); }

        // Jumping
        public virtual void Jump() { }

        // Grabbing
        public virtual void ToggleGrab() { }
        public virtual void Grab() { }
        public virtual void ReleaseGrab() { }

        // Physics
        public abstract void PhysicsUpdate();
        #endregion

        #region checks
        // Movement
        public virtual bool WalkSpeedCheck()
        {
            return !Input.GetKey(KeyBindings.Run);
        }

        public virtual bool RunSpeedCheck()
        {
            return Input.GetKey(KeyBindings.Run);
        }

        // Jumping
        public virtual bool JumpCheck()
        {
            // Magic
            RaycastHit feetCastHit = PlayerController.FeetCast();
            PlayerController.SlopeNormal = PlayerController.GetRaycastNormal(
                PlayerController.Feet.position, feetCastHit.point - PlayerController.Feet.position, PlayerController.GroundMask);
            bool foundGroundDirectlyBelow = feetCastHit.collider != null;
            bool isClimbing = PlayerController.State.GetType() == typeof(PlClimbing);
            return Input.GetKeyDown(KeyCode.Space) && (PlayerController.IsOnStableGround || foundGroundDirectlyBelow || isClimbing);
        }

        // Grabbing
        public virtual bool ToggleGrabCheck()
        {
            return Input.GetKeyDown(KeyBindings.Grab);
        }
        #endregion
    }
}
