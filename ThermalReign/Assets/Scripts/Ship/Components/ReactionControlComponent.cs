using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ship
{
    [RequireComponent(typeof(Rigidbody))]
    public class ReactionControlComponent : ShipComponent
    {
        private Vector3 _strafe;
        private Vector3 _rotate;
        private Rigidbody _rigidbody;
        [SerializeField] private bool useInputSystem = false;
        [SerializeField] private List<ReactionControlThruster> reactionControlThrusters;
        public bool directionalDampening = true;
        public bool angularDampening = true;

        private void OnValidate()
        {
            if (!(componentData is ReactionControlComponentData))
            {
                componentData = null;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            reactionControlThrusters = GetComponentsInChildren<ReactionControlThruster>().ToList();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            ReactionControlComponentData data = componentData as ReactionControlComponentData;
            
            Vector3 forceVector = _strafe * data!.directionalForce;
            Vector3 torqueVector = _rotate * data!.angularForce;

            if (directionalDampening) forceVector = DampenVelocity(transform.InverseTransformDirection(_rigidbody.velocity), forceVector, data!.directionalForce);
            if (angularDampening) torqueVector = DampenVelocity(transform.InverseTransformDirection(_rigidbody.angularVelocity), torqueVector, data!.angularForce);
            var force = CalcForce(forceVector, torqueVector, data);
            ActivateParticleEmitters(force.force, force.torque);
            _rigidbody.AddRelativeForce(force.force);
            _rigidbody.AddRelativeTorque(force.torque);
        }

        private (Vector3 force, Vector3 torque) CalcForce(Vector3 force, Vector3 torque, ReactionControlComponentData data)
        {
            float power = ConvertForceToPower(force, torque, data);
            if (power > CurrentMaxPower)
            {
                var adjusted = AdjustForce(force, torque, power, data);
                force = adjusted.newForce;
                torque = adjusted.newTorgue;
                power = adjusted.newPower;
            }
            PowerUsage = power;
            return (force, torque);
        }

        private (Vector3 newForce, Vector3 newTorgue, float newPower) AdjustForce(Vector3 currentForce, Vector3 currentTorque, float currentPower, ReactionControlComponentData data)
        {
            var percentage = CurrentMaxPower / currentPower;
            currentForce = currentForce * percentage;
            currentTorque = currentTorque * percentage;
            return (currentForce, currentTorque, ConvertForceToPower(currentForce, currentTorque, data));
        }
            
        float ConvertForceToPower(Vector3 force, Vector3 torque, ReactionControlComponentData data)
        {
            return force.magnitude * data!.directionalForceToPowerRatio + torque.magnitude * data!.angularForceToPowerRatio;
        }
        
        private Vector3 DampenVelocity(Vector3 localVelocity, Vector3 forceVector, float force)
        {
            forceVector.x = forceVector.x == 0 ? -(Mathf.Max(Mathf.Min(localVelocity.x, 1), -1) * force) : forceVector.x;
            forceVector.y = forceVector.y == 0 ? -(Mathf.Max(Mathf.Min(localVelocity.y, 1), -1) * force) : forceVector.y;
            forceVector.z = forceVector.z == 0 ? -(Mathf.Max(Mathf.Min(localVelocity.z, 1), -1) * force) : forceVector.z;
            return forceVector;
        }

        private void ActivateParticleEmitters(Vector3 force, Vector3 torque)
        {
            foreach (var thruster in reactionControlThrusters)
            {
                thruster.ReactToThrustVectors(force, torque);
            }
        }
        
        public void Strafe(Vector3 input)
        {
            _strafe = new Vector3(Mathf.Min(input.x, 1f), Mathf.Min(input.y, 1f), Mathf.Min(input.z, 1f));
        }

        public void Rotate(Vector3 input)
        {
            _rotate = new Vector3(Mathf.Min(input.x, 1f), Mathf.Min(input.y, 1f), Mathf.Min(input.z, 1f));
        }
        
        private void OnStrafe(InputValue value)
        {
            if(useInputSystem) Strafe(value.Get<Vector3>());
        }

        private void OnRotate(InputValue value)
        {
            if(useInputSystem) Rotate(value.Get<Vector3>());
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawRay(transform.position, transform.forward);
        }
    }
}

