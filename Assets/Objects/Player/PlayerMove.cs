﻿using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game
{
    public class PlayerMove : MonoBehaviour
    {
        public SpeedData speed = new SpeedData(5f);
        [Serializable]
        public struct SpeedData
        {
            [SerializeField]
            float value;
            public float Value { get { return value; } }

            [SerializeField]
            AnimationCurve distanceMultiplier;
            public AnimationCurve DistanceMultipler { get { return distanceMultiplier; } }

            public float Evaluate(float distance)
            {
                return value * distanceMultiplier.Evaluate(distance);
            }

            public SpeedData(float value)
            {
                this.value = value;

                distanceMultiplier = new AnimationCurve()
                {
                    keys = new Keyframe[] { new Keyframe(0f, 0.2f), new Keyframe(2f, 1f) },
                    postWrapMode = WrapMode.Clamp,
                    preWrapMode = WrapMode.Clamp
                };
            }
        }

        Player player;

        public NavMeshAgent NavAgent { get { return player.NavAgent; } }
        public Vector3 Destination { get { return NavAgent.destination; } }

        public Animator Animator { get { return player.Animator; } }

        public void Init(Player player)
        {
            this.player = player;
        }

        #region Distance
        void CalculateDistances()
        {
            
        }

        protected Vector3 lastCommandPosition;
        public Vector3 LastCommandPosition { get { return lastCommandPosition; } }

        protected float totalDistance;
        public float TotalDistance { get { return totalDistance; } }

        protected float distanceLeft;
        public float DistanceLeft { get { return distanceLeft; } }

        public float DistanceTraveled { get { return TotalDistance - DistanceLeft; } }

        public float DistanceRate { get { return DistanceTraveled / totalDistance; } }
        #endregion

        public void To(Vector3 target)
        {
            if (Vector3.Distance(target, Destination) <= 0.1 + 0.05f)
                return;

            lastCommandPosition = player.FeetPosition;
            totalDistance = distanceLeft = Vector3.Distance(lastCommandPosition, target);

            if (IsProcessing)
                NavAgent.SetDestination(target);
            else
                coroutine = StartCoroutine(Procedure(target));
        }

        Coroutine coroutine;
        public bool IsProcessing { get { return coroutine != null; } }
        IEnumerator Procedure(Vector3 destination)
        {
            NavAgent.SetDestination(destination);
            NavAgent.isStopped = false;

            while (true)
            {
                distanceLeft = Vector3.Distance(player.FeetPosition, Destination);

                NavAgent.speed = speed.Evaluate(DistanceLeft);

                if (DistanceLeft <= NavAgent.stoppingDistance)
                    break;

                yield return new WaitForEndOfFrame();
            }

            NavAgent.isStopped = true;
            coroutine = null;
        }

        public void Stop()
        {
            if (IsProcessing)
            {
                NavAgent.isStopped = true;
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        void Update()
        {
            Animator.SetFloat("Speed", NavAgent.velocity.magnitude);
        }
    }
}