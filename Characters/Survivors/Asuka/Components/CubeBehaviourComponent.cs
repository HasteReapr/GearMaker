using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using RoR2.Projectile;
using R2API;

namespace AsukaMod.Survivors.Asuka.Components
{
    internal class CubeBehaviourComponent : NetworkBehaviour
    {
        public enum CubeState : int
        {
            None = 0,
            Speed = 1,
            Slow = 2,
            Light = 3,
            Heavy = 4,
            Attract = 5,
            Repulse = 6
        }
        public void Awake()
        {

        }

        public void OnEnter()
        {

        }

        public void FixedUpdate()
        {

        }

        public void SetState(CubeState state)
        {

        }

        //We use this to edit the speed of the cube, used by the Time Stretch Staves
        private void SpeedBehaviour()
        {

        }

        //We use this to make the cubes bouncy and either go up or down. Used by gravity filter staves.
        private void GravityBehaviour()
        {

        }

        //We use this to make the cubes attract to the staff. Used by Gravity Rod Shooting
        private void MagneticBehaviour()
        {

        }

        // We use this to make the cubes homing, usesd by Repulsive Rod Shooting
        private void RepulsiveBehaviour()
        {

        }
    }
}
