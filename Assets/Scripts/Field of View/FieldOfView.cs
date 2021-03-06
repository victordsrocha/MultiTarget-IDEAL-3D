// Adapted from: Field of view visualisation - Sebastian Lague - https://youtu.be/rQG9aUWarwE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gizmos = Popcron.Gizmos;

public class FieldOfView : MonoBehaviour
{
    public bool isFoodVisible;
    public Vector3 foodPosition;

    public bool isPoisonVisible;
    public Vector3 poisonPosition;


    public float viewRadius;
    [Range(0, 360)] public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    public Transform closestFood;
    public float closestFoodDst;
    public float closestFoodDstNormalized;

    public Transform closestPoison;
    public float closestPoisonDst;
    public float closestPoisonDstNormalized;

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine("FindTargetsWithDelay", .02f);
    }


    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
            UpdateClosestFood();
            UpdateClosestPoison();
        }
    }

    void LateUpdate()
    {
        DrawFieldOfView();
    }

    public void FindVisibleTargets()
    {
        visibleTargets.Clear();

        int resolution = 10;
        float stepAngleSize = viewAngle / resolution;
        float angle = transform.eulerAngles.y - viewAngle / 2;

        for (int i = 0; i <= resolution; i++)
        {
            Vector3 dir = DirFromAngle(angle + stepAngleSize * i, true);

            if (Physics.Raycast(transform.position, dir, out var hit,
                viewRadius, targetMask))
            {
                Debug.DrawRay(transform.position, dir * hit.distance, Color.yellow, 0.02f);

                if (!visibleTargets.Contains(hit.transform))
                {
                    visibleTargets.Add(hit.transform);
                }
            }
            else
            {
                Debug.DrawRay(transform.position, dir * viewRadius, Color.blue, 0.02f);
            }
        }
    }


    public void UpdateClosestFood()
    {
        closestFood = null;
        closestFoodDst = float.PositiveInfinity;
        closestFoodDstNormalized = float.PositiveInfinity;
        isFoodVisible = false;

        foreach (var visibleTarget in visibleTargets)
        {
            if (visibleTarget.CompareTag("Food"))
            {
                Vector3 targetInThePlane =
                    new Vector3(visibleTarget.position.x, transform.position.y, visibleTarget.position.z);
                float visibleTargetDst = Vector3.Distance(transform.position, targetInThePlane);
                if (visibleTargetDst < closestFoodDst)
                {
                    isFoodVisible = true;
                    closestFoodDst = visibleTargetDst;
                    closestFoodDstNormalized = closestFoodDst / viewRadius;

                    closestFood = visibleTarget;
                }
            }
        }


        if (!isFoodVisible) return;
        if (closestFood is { }) foodPosition = closestFood.position;
    }

    public void UpdateClosestPoison()
    {
        closestPoison = null;
        closestPoisonDst = float.PositiveInfinity;
        closestPoisonDstNormalized = float.PositiveInfinity;
        isPoisonVisible = false;

        foreach (var visibleTarget in visibleTargets)
        {
            if (visibleTarget.CompareTag("Poison"))
            {
                Vector3 targetInThePlane =
                    new Vector3(visibleTarget.position.x, transform.position.y, visibleTarget.position.z);
                float visibleTargetDst = Vector3.Distance(transform.position, targetInThePlane);
                if (visibleTargetDst < closestPoisonDst)
                {
                    isPoisonVisible = true;
                    closestPoisonDst = visibleTargetDst;
                    closestPoisonDstNormalized = closestPoisonDst / viewRadius;

                    closestPoison = visibleTarget;
                }
            }
        }


        if (!isPoisonVisible) return;
        if (closestPoison is { }) foodPosition = closestPoison.position;
    }


    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit ||
                    (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }

                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }


    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }


    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void AdjustViewAngle(float newViewAngle)
    {
        viewAngle = newViewAngle;
    }
}