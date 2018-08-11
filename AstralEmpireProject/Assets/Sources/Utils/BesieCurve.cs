using UnityEngine;
using System.Collections;

public class BesieCurve {
    private const int defaultPointsInCurve = 7;

    // Curve controls
    public float Lenght { get { return fullLenght; } }

    public readonly Vector3[] PathPoints;
    private float[] pathLenghts;
    private float[] pathDistances;
    private float fullLenght = 0;

    public Vector3 GetPositionOnCurve(float distance) {
        if (distance <= 0) {
            return PathPoints[0];
        }
        if (distance >= fullLenght) {
            return PathPoints[PathPoints.Length - 1];
        }
        for (int i = 0; i < PathPoints.Length - 1; i++) {
            if (distance > pathDistances[i] && distance <= pathDistances[i + 1]) {
                float normalizedLocalDistance = (distance - pathDistances[i]) / pathLenghts[i];
                return Utils.Lerp(PathPoints[i], PathPoints[i + 1], normalizedLocalDistance);
            }
        }
        return PathPoints[PathPoints.Length - 1];
    }

    public BesieCurve(Vector3[] controlPoints, int pointsInCurveTurn = defaultPointsInCurve) {
        PathPoints = GeneratePath(controlPoints, pointsInCurveTurn);
        FindPointDistancesParams();
    }

    private Vector3[] GeneratePath(Vector3[] controlPoints, int pointsBetweenControlPoints = defaultPointsInCurve) {
        Vector3[] middleControlPoints = new Vector3[controlPoints.Length - 1];
        middleControlPoints[0] = controlPoints[0]; // first point
        middleControlPoints[middleControlPoints.Length - 1] = controlPoints[controlPoints.Length - 1]; // last point
        for (int i = 0; i < middleControlPoints.Length; i++) {
            middleControlPoints[i] = (controlPoints[i] + controlPoints[i + 1]) * 0.5f;
        }
        var pathPoints = new Vector3[(middleControlPoints.Length - 1) * pointsBetweenControlPoints + 3];
        pathPoints[0] = controlPoints[0];
        for (int i = 0; i < middleControlPoints.Length - 1; i++) {
            for (int segment = 0; segment < pointsBetweenControlPoints; segment++) {
                pathPoints[i * pointsBetweenControlPoints + 1 + segment] = GetBesiePoint(
                    middleControlPoints[i],
                    middleControlPoints[i + 1],
                    controlPoints[i + 1],
                    (float)segment / (float)(pointsBetweenControlPoints));
            }
        }
        pathPoints[pathPoints.Length - 2] = middleControlPoints[middleControlPoints.Length - 1];
        pathPoints[pathPoints.Length - 1] = controlPoints[controlPoints.Length - 1];
        return pathPoints;
    }

    private void FindPointDistancesParams() {
        pathLenghts = new float[PathPoints.Length];
        pathDistances = new float[PathPoints.Length];
        fullLenght = 0;
        for (int i = 0; i < PathPoints.Length - 1; i++) {
            pathLenghts[i] = (PathPoints[i + 1] - PathPoints[i]).magnitude;
            pathDistances[i] = fullLenght;
            fullLenght += pathLenghts[i];
        }
        pathDistances[pathDistances.Length - 1] = fullLenght;
    }

    public static Vector3 GetBesiePoint(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint, float t) {
        Vector3 startControlLine = (startPoint + (controlPoint - startPoint) * t);
        Vector3 controlEndLine = (controlPoint + (endPoint - controlPoint) * t);
        return (startControlLine + (controlEndLine - startControlLine) * t);
    }
}