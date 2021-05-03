using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observation : MonoBehaviour
{
    private enum VisionStateStatus
    {
        Appear,
        Disappear,
        Closer,
        Further,
        Reached,
        Bump,
        Unchanged
    }

    public BeakTrigger beakTrigger;
    private BodyCollider _bodyCollider;

    public FieldOfView leftFOV;
    public FieldOfView rightFOV;
    public FieldOfViewAdaptedToEdge edgeFOV;

    public bool isRightFoodVisible;
    public Transform rightFood;
    public float rightFoodDstNormalized;

    public bool isRightPoisonVisible;
    public Transform rightPoison;
    public float rightPoisonDstNormalized;

    public bool isLeftFoodVisible;
    public Transform leftFood;
    public float leftFoodDstNormalized;

    public bool isLeftPoisonVisible;
    public Transform leftPoison;
    public float leftPoisonDstNormalized;

    public bool isWallVisible;
    public Vector3 closestWallPoint;
    public float wallDstNormalized;

    private Eye leftEye;
    private Eye rightEye;
    private EyeWall wallEye;

    private void Start()
    {
        leftEye = new Eye(leftFOV);
        rightEye = new Eye(rightFOV);
        wallEye = new EyeWall(edgeFOV);

        _bodyCollider = GetComponent<BodyCollider>();
    }

    public string ObservationString()
    {
        UpdateVisionState();
        UpdateLeftEye();
        UpdateRightEye();
        IsFoodReached();
        UpdateWallEye();

        var observationString = ",";
        observationString += ResultFromStatus(leftEye.LastFoodStatus);
        observationString += ResultFromStatus(rightEye.LastFoodStatus);
        observationString += ",";
        observationString += ResultFromStatus(leftEye.LastPoisonStatus);
        observationString += ResultFromStatus(rightEye.LastPoisonStatus);
        observationString += ",";
        observationString += ResultFromStatus(wallEye.LastWallStatus);
        observationString += ",";
        observationString += ClosestLeftTarget();
        observationString += ClosestRightTarget();

        return observationString;
    }

    char ClosestLeftTarget()
    {
        if (leftEye.LastFoodDistance<leftEye.LastPoisonDistance)
        {
            return 'f';
        }
        else if (leftEye.LastFoodDistance>leftEye.LastPoisonDistance)
        {
            return 'p';
        }

        return 'n';
    }
    
    char ClosestRightTarget()
    {
        if (rightEye.LastFoodDistance<rightEye.LastPoisonDistance)
        {
            return 'f';
        }
        else if (rightEye.LastFoodDistance>rightEye.LastPoisonDistance)
        {
            return 'p';
        }

        return 'n';
    }

    private void UpdateVisionState()
    {
        isRightFoodVisible = rightFOV.isFoodVisible;
        rightFood = rightFOV.closestFood;
        rightFoodDstNormalized = rightFOV.closestFoodDstNormalized;

        isRightPoisonVisible = rightFOV.isPoisonVisible;
        rightPoison = rightFOV.closestPoison;
        rightPoisonDstNormalized = rightFOV.closestPoisonDstNormalized;

        isLeftFoodVisible = leftFOV.isFoodVisible;
        leftFood = leftFOV.closestFood;
        leftFoodDstNormalized = leftFOV.closestFoodDstNormalized;

        isLeftPoisonVisible = leftFOV.isPoisonVisible;
        leftPoison = leftFOV.closestPoison;
        leftPoisonDstNormalized = leftFOV.closestPoisonDstNormalized;

        isWallVisible = edgeFOV.isTargetVisible;
        closestWallPoint = edgeFOV.targetPosition;
        wallDstNormalized = edgeFOV.closestTargetDstNormalized;
    }

    private void UpdateLeftEye()
    {
        if (isLeftFoodVisible)
        {
            if (leftEye.IsSeeingAnyFood)
            {
                if (leftFoodDstNormalized < leftEye.LastFoodDistance)
                {
                    leftEye.LastFoodStatus = VisionStateStatus.Closer;
                }
                else if (leftFoodDstNormalized > leftEye.LastFoodDistance)
                {
                    leftEye.LastFoodStatus = VisionStateStatus.Further;
                }
                else
                {
                    leftEye.LastFoodStatus = VisionStateStatus.Unchanged;
                }
            }
            else
            {
                leftEye.LastFoodStatus = VisionStateStatus.Appear;
            }
        }
        else
        {
            if (leftEye.IsSeeingAnyFood)
            {
                leftEye.LastFoodStatus = VisionStateStatus.Disappear;
            }
            else
            {
                leftEye.LastFoodStatus = VisionStateStatus.Unchanged;
            }
        }

        if (isLeftPoisonVisible)
        {
            if (leftEye.IsSeeingAnyPoison)
            {
                if (leftPoisonDstNormalized < leftEye.LastPoisonDistance)
                {
                    leftEye.LastPoisonStatus = VisionStateStatus.Closer;
                }
                else if (leftPoisonDstNormalized > leftEye.LastPoisonDistance)
                {
                    leftEye.LastPoisonStatus = VisionStateStatus.Further;
                }
                else
                {
                    leftEye.LastPoisonStatus = VisionStateStatus.Unchanged;
                }
            }
            else
            {
                leftEye.LastPoisonStatus = VisionStateStatus.Appear;
            }
        }
        else
        {
            if (leftEye.IsSeeingAnyPoison)
            {
                leftEye.LastPoisonStatus = VisionStateStatus.Disappear;
            }
            else
            {
                leftEye.LastPoisonStatus = VisionStateStatus.Unchanged;
            }
        }

        leftEye.LastFoodDistance = leftFoodDstNormalized;
        leftEye.LastPoisonDistance = leftPoisonDstNormalized;
        leftEye.IsSeeingAnyFood = isLeftFoodVisible;
        leftEye.IsSeeingAnyPoison = isLeftPoisonVisible;
    }

    private void UpdateRightEye()
    {
        if (isLeftFoodVisible)
        {
            if (rightEye.IsSeeingAnyFood)
            {
                if (leftFoodDstNormalized < rightEye.LastFoodDistance)
                {
                    rightEye.LastFoodStatus = VisionStateStatus.Closer;
                }
                else if (leftFoodDstNormalized > rightEye.LastFoodDistance)
                {
                    rightEye.LastFoodStatus = VisionStateStatus.Further;
                }
                else
                {
                    rightEye.LastFoodStatus = VisionStateStatus.Unchanged;
                }
            }
            else
            {
                rightEye.LastFoodStatus = VisionStateStatus.Appear;
            }
        }
        else
        {
            if (rightEye.IsSeeingAnyFood)
            {
                rightEye.LastFoodStatus = VisionStateStatus.Disappear;
            }
            else
            {
                rightEye.LastFoodStatus = VisionStateStatus.Unchanged;
            }
        }

        if (isLeftPoisonVisible)
        {
            if (rightEye.IsSeeingAnyPoison)
            {
                if (leftPoisonDstNormalized < rightEye.LastPoisonDistance)
                {
                    rightEye.LastPoisonStatus = VisionStateStatus.Closer;
                }
                else if (leftPoisonDstNormalized > rightEye.LastPoisonDistance)
                {
                    rightEye.LastPoisonStatus = VisionStateStatus.Further;
                }
                else
                {
                    rightEye.LastPoisonStatus = VisionStateStatus.Unchanged;
                }
            }
            else
            {
                rightEye.LastPoisonStatus = VisionStateStatus.Appear;
            }
        }
        else
        {
            if (rightEye.IsSeeingAnyPoison)
            {
                rightEye.LastPoisonStatus = VisionStateStatus.Disappear;
            }
            else
            {
                rightEye.LastPoisonStatus = VisionStateStatus.Unchanged;
            }
        }

        rightEye.LastFoodDistance = rightFoodDstNormalized;
        rightEye.LastPoisonDistance = rightPoisonDstNormalized;
        rightEye.IsSeeingAnyFood = isRightFoodVisible;
        rightEye.IsSeeingAnyPoison = isRightPoisonVisible;
    }

    void IsFoodReached()
    {
        if (beakTrigger.lastFruit != 0)
        {
            if (beakTrigger.lastFruit == 1)
            {
                leftEye.LastFoodStatus = VisionStateStatus.Reached;
                rightEye.LastFoodStatus = VisionStateStatus.Reached;
            }
            else
            {
                leftEye.LastPoisonStatus = VisionStateStatus.Reached;
                rightEye.LastPoisonStatus = VisionStateStatus.Reached;
            }

            beakTrigger.lastFruit = 0;
        }
    }

    private void UpdateWallEye()
    {
        if (isWallVisible)
        {
            if (wallEye.IsSeeingWall)
            {
                if (wallDstNormalized < wallEye.LastWallDistance)
                {
                    wallEye.LastWallStatus = VisionStateStatus.Closer;
                }
                else if (wallDstNormalized > wallEye.LastWallDistance)
                {
                    wallEye.LastWallStatus = VisionStateStatus.Further;
                }
                else
                {
                    wallEye.LastWallStatus = VisionStateStatus.Unchanged;
                }
            }
            else
            {
                wallEye.LastWallStatus = VisionStateStatus.Appear;
            }
        }
        else
        {
            if (wallEye.IsSeeingWall)
            {
                wallEye.LastWallStatus = VisionStateStatus.Disappear;
            }
            else
            {
                wallEye.LastWallStatus = VisionStateStatus.Unchanged;
            }
        }

        //bump?
        if (wallEye.IsSeeingWall)
        {
            if (_bodyCollider.isColliding)
            {
                wallEye.LastWallStatus = VisionStateStatus.Bump;
            }
        }

        wallEye.LastWallDistance = wallDstNormalized;
        wallEye.IsSeeingWall = isWallVisible;
    }

    private char ResultFromStatus(VisionStateStatus status)
    {
        switch (status)
        {
            case VisionStateStatus.Appear:
                return 'a';
            case VisionStateStatus.Disappear:
                return 'd';
            case VisionStateStatus.Closer:
                return 'c';
            case VisionStateStatus.Further:
                return 'f';
            case VisionStateStatus.Bump:
                return 'b';
            case VisionStateStatus.Reached:
                return 'r';
            case VisionStateStatus.Unchanged:
                return 'u';
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private struct Eye
    {
        private FieldOfView _fieldOfView;
        public VisionStateStatus LastFoodStatus;
        public VisionStateStatus LastPoisonStatus;
        public float LastFoodDistance;
        public float LastPoisonDistance;
        public bool IsSeeingAnyFood;
        public bool IsSeeingAnyPoison;

        public Eye(FieldOfView fieldOfView)
        {
            _fieldOfView = fieldOfView;
            LastFoodStatus = VisionStateStatus.Unchanged;
            LastPoisonStatus = VisionStateStatus.Unchanged;
            LastFoodDistance = float.PositiveInfinity;
            LastPoisonDistance = float.PositiveInfinity;
            IsSeeingAnyFood = false;
            IsSeeingAnyPoison = false;
        }
    }

    private struct EyeWall
    {
        private FieldOfViewAdaptedToEdge _fieldOfViewAdaptedToEdge;
        public VisionStateStatus LastWallStatus;
        public float LastWallDistance;
        public bool IsSeeingWall;

        public EyeWall(FieldOfViewAdaptedToEdge fieldOfViewAdaptedToEdge)
        {
            _fieldOfViewAdaptedToEdge = fieldOfViewAdaptedToEdge;
            LastWallStatus = VisionStateStatus.Unchanged;
            LastWallDistance = float.PositiveInfinity;
            IsSeeingWall = false;
        }
    }
}