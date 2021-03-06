﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Modified from code written by Sebastian Lague
//Original Source: https://github.com/SebLague/Field-of-View/blob/master/Episode%2003/FieldOfView.cs

public class CastLight : MonoBehaviour {

    public float viewRadius; //the distance the light is cast
    [Range(0, 360)]
    public float viewAngle; //the number of degrees the light goes out; set to 360 for all directions

    public LayerMask targetMask; //the objects that detect being in the light
    public LayerMask obstacleMask; //the objects that block light

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>(); //the list of objects that are in the light
    private List<RaycastHit2D> hitsToCheck = new List<RaycastHit2D>();
    private List<GameObject> hitsChecked = new List<GameObject>();

    public float meshResolution; //increasing this improves corner recognition, but slows performance
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public float maskCutawayDst = 0f; //the amount of the edge of obstacles that is shown; values other than 0 cause strange ground shadows

    public MeshFilter viewMeshFilter;
    public Vector3 positionOffset = Vector3.zero;
    Mesh viewMesh; //the light polygon created by the script

    /**
     * Creates the mesh object that represents the light
     */
    void Start() {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine("FindTargetsWithDelay", .2f);
    }

    /**
     * Periodically identifies objects that are in the light
     */
    IEnumerator FindTargetsWithDelay(float delay) {
        while (true) {
            yield return new WaitForSeconds(delay);
            visibleTargets.Clear();
            foreach (RaycastHit2D hit in hitsToCheck) {
                CheckHit(hit);
            }
            hitsToCheck.Clear();
            hitsChecked.Clear();
        }
    }

    /**
     * Draws light after everything else has run
     */
    void LateUpdate() {
        DrawFieldOfView();
    }

    /**
     * Identifies objects that are in the light
     */
    void CheckHit(RaycastHit2D hit) {
        Transform target = hit.transform;
        if (hitsChecked.Contains(target.gameObject)) return; //don't check the same object twice in one frame

        if (isActiveAndEnabled) {
            hitsChecked.Add(target.gameObject);
            visibleTargets.Add(target);

            //IMPORTANT: Put code here to get objects to do something when in the light
            Mirror mirror = target.GetComponent<Mirror>();
            if (mirror && mirror.gameObject != this.gameObject) //prevents mirrors from keeping themselves active
                mirror.Activate(this); //turns mirror light on

            GrowingPlant vine = target.GetComponent<GrowingPlant>();
            if (vine)
                vine.Grow();

            ShadowPlayerObject shadowPlayer = target.GetComponent<ShadowPlayerObject>();
            if (shadowPlayer)
                shadowPlayer.Die(); //game over

            PlayerController player = target.GetComponent<PlayerController>();
            if (player && player.lightOrShadow == PlayerController.PlayerType.Shadow)
                player.Die(); //game over
        }
    }

    /**
     * Draws a polygon representing the light using raycasts to find the edges of objects that block light
     */
    void DrawFieldOfView() {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++) {
            float angle = 360 - transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0) {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    //adds points to the border of the mesh
                    if (edge.pointA != Vector3.zero) {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero) {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        //constructs the light's polygonal mesh
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.up * maskCutawayDst;

            if (i < vertexCount - 2) {
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

    /**
     * Identifies the edges of objects that block light
     */
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++) {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    /**
     * Uses raycasts to identify the objects that block light
     */
    ViewCastInfo ViewCast(float globalAngle) {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + positionOffset, dir, viewRadius, obstacleMask | targetMask);

        foreach (RaycastHit2D hit in hits) {
            if (hit.transform == this.transform) continue;
            if (hit && (targetMask == (targetMask | (1 << hit.collider.gameObject.layer)))) hitsToCheck.Add(hit);
            if (hit && (obstacleMask == (obstacleMask | (1 << hit.collider.gameObject.layer)))) return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        return new ViewCastInfo(false, transform.position + positionOffset + dir * viewRadius, viewRadius, globalAngle);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if (!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    public struct ViewCastInfo {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
