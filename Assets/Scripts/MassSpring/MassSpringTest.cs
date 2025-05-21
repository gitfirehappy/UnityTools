using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MassSpringTest : MonoBehaviour
{
    #region members
    class Spring
    {
        public readonly Transform pointA;
        public readonly Transform pointB;
        public float length;

        public Spring(Transform pointA, Transform pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    [SerializeField] [Range(0, 2)] float elasticity = 0.75f;//弹性系数
    [SerializeField] [Range(0, 0.999f)] float drag = 0.1f;//阻尼
    [SerializeField] [Range(0.1f, 5f)] float unit = 1f;//质点间距
    [SerializeField] [Range(1, 5)] int bendDistance = 2;//抗弯折距
    [SerializeField] int size = 30;//网格大小
    [SerializeField] bool isCloth;//是否是布料
    [SerializeField] bool useGravity;

    readonly List<Spring>allSprings = new List<Spring>();
    readonly List<Transform>allpoints = new List<Transform>();
    readonly Dictionary<Transform,Vector3>lastPositions = new Dictionary<Transform,Vector3>();

    #endregion

    //初始化
    private void Start()
    {
        if (!isCloth) 
        {
            Transform[] points = new Transform[size];
            for(int i = 0; i < size; i++)
            {
                Vector3 position = new Vector3(i*unit,0,0);
                GameObject point = new GameObject();
                allpoints.Add(point.transform);
                lastPositions[point.transform] = position;
                point.transform.position = position;
                points[i] = point.transform;
            }
            for(int i = 0;i < size-1; i++)
            {
                allSprings.Add(new Spring(points[i], points[i+1]));
            }
        }
        else
        {
            Transform[,] points = new Transform[size,size];
            for(int i = 0; i < size; i++)
                for(int j = 0; j < size; j++)
                {
                    float x = j*unit;
                    float y = i*unit;
                    Vector3 position = new Vector3(x,y,0);
                    GameObject point = new GameObject();
                    allpoints.Add(point.transform);
                    lastPositions[point.transform] = position;
                    point.transform.position = position;
                    points[i,j] = point.transform;
                }

                //添加弹簧
            for(int u = 0;u < size; u++)
            {
                for (int v = 0;v < size; v++)
                {
                    //横纵结构
                    if(v+1<size)
                        allSprings.Add(new Spring(points[u,v], points[u,v+1]));
                    if(u+1<size)
                        allSprings.Add(new Spring(points[u,v], points[u+1,v]));
                    //斜向结构
                    if(v+1<size&&u+1<size)
                        allSprings.Add(new Spring(points[u, v], points[u+1,v+1]));
                    if(v+1<size&&u-1>=0)
                        allSprings.Add(new Spring(points[u, v], points[u-1, v+1]));
                    //抗弯折
                    if (v + bendDistance < size)
                        allSprings.Add(new Spring(points[u, v], points[u, v + bendDistance]));
                    if (u + bendDistance < size)
                        allSprings.Add(new Spring(points[u, v], points[u + bendDistance, v]));
                }
            }

        }

        foreach (Spring spring in allSprings)
        {
            spring.length = Vector3.Distance(spring.pointA.position,spring.pointB.position);
        }
    }

    //编辑器交互
    float GetMinusDrag(Transform transform)
    {
        return transform == Selection.activeTransform ? 0 : 1 - drag;
    }

    private void Update()
    {
        //简化的Verlet积分法
        foreach(Transform point in allpoints)
        {
            Vector3 position = point.position;
            point.position += (position - lastPositions[point]) * GetMinusDrag(point);
            lastPositions[point] = position;
        }

        if (useGravity)
        {
            foreach (Transform point in allpoints)
            {
                const float Gravity = 0.2f * 0.2f * 0.981f;
                point.position += new Vector3(0, -Gravity, 0) * GetMinusDrag(point);
            }
        }
        foreach (Spring spring in allSprings)
        {
            Transform pointA = spring.pointA;
            Transform pointB = spring.pointB;

            Vector3 positionA = pointA.position;
            Vector3 positionB = pointB.position;
            Vector3 vector = positionA - positionB;

            if(vector == Vector3.zero) vector = new Vector3(0, float.Epsilon, 0);
            //胡克定律
            float distance = vector.magnitude;//取模
            float tendency = distance - spring.length;
            Vector3 direction = vector / distance;
            Vector3 move = elasticity * tendency * direction;

            float moveWeight;//权重
            if (!isCloth && drag > 0.99f)
            {
                moveWeight = 1.0f;
            }
            else
            {
                float oneMinusDragA = GetMinusDrag(pointA);
                float oneMinusDragB = GetMinusDrag(pointB);
                moveWeight = Mathf.Approximately(oneMinusDragB + oneMinusDragA, 0.0f) ? 0.5f : oneMinusDragB / (oneMinusDragB + oneMinusDragA);//防止分母过小
            }
            pointB.position += moveWeight *move;
            pointA.position += (1-moveWeight)*-move;
        }
    }
    void OnDrawGizmos()
    {
        if (allSprings != null)
            foreach (Spring spring in allSprings)
            {
                Gizmos.DrawLine(spring.pointA.position, spring.pointB.position);
            }
    }

}
