using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HolmanPlayerController
{
    public class StateMaster : MonoBehaviour
    {
        // Physics
        /// <summary>
        /// Adds gravity.
        /// </summary>
        public static void PhysicsGravity(PlayerController playerController)
        {
            Vector3 g = new Vector3(0, -9.81f, 0);
            playerController.AddPhysicsVelocity(g * Time.deltaTime);
        }

        /// <summary>
        /// Adds general velocity-based movement, bouncing and sliding.
        /// </summary>
        public static void PhysicsVelocityBounceSlide(PlayerController playerController)
        {
            playerController.CharacterController.Move(playerController.CombinedVelocity * Time.deltaTime);
        }
    }
}
