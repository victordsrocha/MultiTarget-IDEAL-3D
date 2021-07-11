using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        BumpForward,
        Release,
        Unchanged
    }

    private enum FocusObjectType
    {
        Food,
        Poison,
        None
    }

    private enum FocusClosenessStatus
    {
        Closer,
        Distant,
        Poison,
        Food,
        None
    }

    private enum FocusEye
    {
        Left,
        Right,
        Both,
        None
    }

    public BeakTrigger beakTrigger;
    public BodyCollider bodyCollider;

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

    private bool forward;

    // Focus
    private FocusObjectType _previousFocusObjectType; // atualizar previous focus para conseguir utilizar o disappear
    private FocusObjectType _focusObjectType;
    private VisionStateStatus _focusObjectStatus;
    private bool _focusChange;
    private FocusClosenessStatus _focusClosenessStatus;
    private FocusEye _focusEye;
    private Transform _focusTransform;
    private float _focusLastDistance;
    private float _distanceThreshold;

    public string Result { get; private set; }

    private void Start()
    {
        leftEye = new Eye(leftFOV);
        rightEye = new Eye(rightFOV);
        wallEye = new EyeWall(edgeFOV);


        _focusObjectType = FocusObjectType.None;
        _focusObjectStatus = VisionStateStatus.Unchanged;
        _focusChange = false;
        _focusClosenessStatus = FocusClosenessStatus.None;
        _focusEye = FocusEye.None;
        _focusTransform = null;
        _focusLastDistance = float.PositiveInfinity;
        _distanceThreshold = 0.28f;
    }

    public void ObservationResult(bool isForward)
    {
        _previousFocusObjectType = _focusObjectType;
        forward = isForward;

        UpdateVisionState();
        UpdateLeftEye();
        UpdateRightEye();
        IsFoodOrPoisonReached();
        UpdateWallEye();

        UpdateFocus();

        var observationString = ",";

        observationString += FocusResultString();

        //observationString += ResultFromStatus(leftEye.LastFoodStatus);
        //observationString += ResultFromStatus(rightEye.LastFoodStatus);
        //observationString += ResultFromStatus(leftEye.LastPoisonStatus);
        //observationString += ResultFromStatus(rightEye.LastPoisonStatus);

        observationString += ResultFromStatus(wallEye.LastWallStatus);
        //observationString += ClosestLeftTarget();
        //observationString += ClosestRightTarget();

        Result = observationString;
    }

    string FocusResultString()
    {
        string result = string.Empty;

        result += _focusObjectType switch
        {
            FocusObjectType.Food => "f",
            FocusObjectType.Poison => "p",
            FocusObjectType.None => "-",
            _ => throw new ArgumentOutOfRangeException()
        };

        switch (_focusObjectStatus)
        {
            case VisionStateStatus.Appear:
                result += "a";
                break;
            case VisionStateStatus.Closer:
                result += "c";
                break;
            case VisionStateStatus.Further:
                result += "f";
                break;
            case VisionStateStatus.Reached:
                result += "r";
                break;
            case VisionStateStatus.Unchanged:
                result += "-";
                break;
        }

        if (_focusChange)
        {
            result += _previousFocusObjectType switch
            {
                FocusObjectType.Food => "f",
                FocusObjectType.Poison => "p",
                _ => "*"
            };
        }
        else
        {
            result += "-";
        }

        result += _focusClosenessStatus switch
        {
            FocusClosenessStatus.Closer => "c",
            FocusClosenessStatus.Distant => "d",
            FocusClosenessStatus.None => "-",
            FocusClosenessStatus.Poison => "p",
            FocusClosenessStatus.Food => "f",
            _ => throw new ArgumentOutOfRangeException()
        };

        result += _focusEye switch
        {
            FocusEye.Left => "l",
            FocusEye.Right => "r",
            FocusEye.Both => "b",
            FocusEye.None => "-",
            _ => throw new ArgumentOutOfRangeException()
        };

        return result;
    }

    void UpdateFocus()
    {
        float leftFood = leftEye.LastFoodDistance;
        float leftPoison = leftEye.LastPoisonDistance;
        float rightFood = rightEye.LastFoodDistance;
        float rightPoison = rightEye.LastPoisonDistance;


        if (leftEye.LastFoodStatus == VisionStateStatus.Reached ||
            rightEye.LastFoodStatus == VisionStateStatus.Reached)
        {
            _focusObjectType = FocusObjectType.Food;
            _focusChange = true;
            _focusTransform = null;
            _focusObjectStatus = VisionStateStatus.Reached;
            _focusLastDistance = float.PositiveInfinity;
            _focusClosenessStatus = FocusClosenessStatus.Food;
            _focusEye = FocusEye.None;
            return;
        }
        else if (leftEye.LastPoisonStatus == VisionStateStatus.Reached ||
                 rightEye.LastPoisonStatus == VisionStateStatus.Reached)
        {
            _focusObjectType = FocusObjectType.Poison;
            _focusChange = true;
            _focusTransform = null;
            _focusObjectStatus = VisionStateStatus.Reached;
            _focusLastDistance = float.PositiveInfinity;
            _focusClosenessStatus = FocusClosenessStatus.Poison;
            _focusEye = FocusEye.None;
            return;
        }

        if (leftFood <= leftPoison && leftFood <= rightFood && leftFood <= rightPoison)
        {
            _focusObjectType = FocusObjectType.Food;
            UpdateFocusTransition(leftFOV.closestFood);
            _focusTransform = leftFOV.closestFood;

            _focusObjectStatus = UpdateFocusObjectStatus(true, leftEye.LastFoodDistance);
            _focusLastDistance = leftEye.LastFoodDistance;

            UpdateFocusCloseness(_focusLastDistance, true);

            _focusEye = leftFOV.closestFood == rightFOV.closestFood ? FocusEye.Both : FocusEye.Left;
        }
        else if (rightFood <= rightPoison && rightFood <= leftPoison)
        {
            _focusObjectType = FocusObjectType.Food;
            UpdateFocusTransition(rightFOV.closestFood);
            _focusTransform = rightFOV.closestFood;

            _focusObjectStatus = UpdateFocusObjectStatus(true, rightEye.LastFoodDistance);
            _focusLastDistance = rightEye.LastFoodDistance;

            UpdateFocusCloseness(_focusLastDistance, true);

            _focusEye = leftFOV.closestFood == rightFOV.closestFood ? FocusEye.Both : FocusEye.Right;
        }
        else if (leftPoison <= rightPoison)
        {
            _focusObjectType = FocusObjectType.Poison;
            UpdateFocusTransition(leftFOV.closestPoison);
            _focusTransform = leftFOV.closestPoison;

            _focusObjectStatus = UpdateFocusObjectStatus(true, leftEye.LastPoisonDistance);
            _focusLastDistance = leftEye.LastPoisonDistance;

            UpdateFocusCloseness(_focusLastDistance, true);

            _focusEye = leftFOV.closestPoison == rightFOV.closestPoison ? FocusEye.Both : FocusEye.Left;
        }
        else if (rightPoison < rightFood)
        {
            _focusObjectType = FocusObjectType.Poison;
            UpdateFocusTransition(rightFOV.closestPoison);
            _focusTransform = rightFOV.closestPoison;

            _focusObjectStatus = UpdateFocusObjectStatus(true, rightEye.LastPoisonDistance);
            _focusLastDistance = rightEye.LastPoisonDistance;

            UpdateFocusCloseness(_focusLastDistance, true);

            _focusEye = leftFOV.closestPoison == rightFOV.closestPoison ? FocusEye.Both : FocusEye.Right;
        }
        else
        {
            _focusObjectType = FocusObjectType.None;
            UpdateFocusTransition(null);
            _focusTransform = null;

            _focusObjectStatus = UpdateFocusObjectStatus(false, float.PositiveInfinity);
            _focusLastDistance = float.PositiveInfinity;

            // TODO incluir Disappear

            UpdateFocusCloseness(_focusLastDistance, false);
            _focusEye = FocusEye.None;
        }
    }

    VisionStateStatus UpdateFocusObjectStatus(bool targetTrue, float newDistance)
    {
        if (_focusChange && targetTrue)
        {
            return VisionStateStatus.Appear;
        }

        if (targetTrue)
        {
            if (_focusLastDistance > newDistance)
            {
                return VisionStateStatus.Closer;
            }
            else if (_focusLastDistance < newDistance)
            {
                return VisionStateStatus.Further;
            }

            return VisionStateStatus.Unchanged;
        }
        else
        {
            return VisionStateStatus.Unchanged;
        }
    }

    void UpdateFocusTransition(Transform newFocusTransform)
    {
        _focusChange = _focusTransform != newFocusTransform;
    }

    void UpdateFocusCloseness(float focusDistance, bool targetTrue)
    {
        if (targetTrue && focusDistance < _distanceThreshold)
        {
            _focusClosenessStatus = FocusClosenessStatus.Closer;
        }
        else if (targetTrue)
        {
            _focusClosenessStatus = FocusClosenessStatus.Distant;
        }
        else
        {
            _focusClosenessStatus = FocusClosenessStatus.None;
        }
    }


    char ClosestLeftTarget()
    {
        if (leftEye.LastFoodDistance < leftEye.LastPoisonDistance)
        {
            return 'f';
        }
        else if (leftEye.LastFoodDistance > leftEye.LastPoisonDistance)
        {
            return 'p';
        }

        return 'n';
    }

    char ClosestRightTarget()
    {
        if (rightEye.LastFoodDistance < rightEye.LastPoisonDistance)
        {
            return 'f';
        }
        else if (rightEye.LastFoodDistance > rightEye.LastPoisonDistance)
        {
            return 'p';
        }

        return 'n';
    }

    private void UpdateVisionState()
    {
        leftFOV.FindVisibleTargets();
        leftFOV.UpdateClosestFood();
        leftFOV.UpdateClosestPoison();

        rightFOV.FindVisibleTargets();
        rightFOV.UpdateClosestFood();
        rightFOV.UpdateClosestPoison();

        edgeFOV.FindVisibleTargets();
        edgeFOV.FindVisibleTargetsMinimumDistance();

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
                //DEBUG!!!
                if (leftEye.LastFoodStatus == VisionStateStatus.Appear)
                {
                    Debug.Log("ERRROOOOOO");
                }

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
        if (isRightFoodVisible)
        {
            if (rightEye.IsSeeingAnyFood)
            {
                if (rightFoodDstNormalized < rightEye.LastFoodDistance)
                {
                    rightEye.LastFoodStatus = VisionStateStatus.Closer;
                }
                else if (rightFoodDstNormalized > rightEye.LastFoodDistance)
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

        if (isRightPoisonVisible)
        {
            if (rightEye.IsSeeingAnyPoison)
            {
                if (rightPoisonDstNormalized < rightEye.LastPoisonDistance)
                {
                    rightEye.LastPoisonStatus = VisionStateStatus.Closer;
                }
                else if (rightPoisonDstNormalized > rightEye.LastPoisonDistance)
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

    void IsFoodOrPoisonReached()
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
            if (bodyCollider.isColliding)
            {
                if (forward)
                {
                    wallEye.LastWallStatus = VisionStateStatus.BumpForward;
                    wallEye.IsBumping = true;
                }
                else
                {
                    wallEye.LastWallStatus = VisionStateStatus.Bump;
                    wallEye.IsBumping = true;
                }
            }
            else if (wallEye.IsBumping)
            {
                wallEye.IsBumping = false;
                // wallEye.LastWallStatus = VisionStateStatus.Release;
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
            case VisionStateStatus.BumpForward:
                return 'B';
            case VisionStateStatus.Reached:
                return 'r';
            case VisionStateStatus.Unchanged:
                return '-';
            case VisionStateStatus.Release:
                return 'l';
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
        public bool IsBumping;

        public EyeWall(FieldOfViewAdaptedToEdge fieldOfViewAdaptedToEdge)
        {
            _fieldOfViewAdaptedToEdge = fieldOfViewAdaptedToEdge;
            LastWallStatus = VisionStateStatus.Unchanged;
            LastWallDistance = float.PositiveInfinity;
            IsSeeingWall = false;
            IsBumping = false;
        }
    }
}