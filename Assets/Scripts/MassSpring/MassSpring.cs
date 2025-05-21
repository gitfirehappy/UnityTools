using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MassSpring : MonoBehaviour
{
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

    [SerializeField][Range(0, 2)] float elasticity = 0.75f;
    [SerializeField][Range(0, 0.999f)] float drag = 0.01f;
    [SerializeField] int size = 30;
    [SerializeField] int unit = 1;
    [SerializeField] bool ropeOrCloth;
    [SerializeField] bool useGravity;

    readonly List<Spring> allSprings = new List<Spring>();
    readonly List<Transform> allPoints = new List<Transform>();
    readonly Dictionary<Transform, Vector3> lastPositions = new Dictionary<Transform, Vector3>();

    void Start()
    {
        if (ropeOrCloth) //����
        {
            //����ʵ�
            Transform[] points = new Transform[size];
            for (int i = 0; i < size; i++)
            {
                Vector3 position = new Vector3(i * unit, 0, 0);
                GameObject point = new GameObject();
                allPoints.Add(point.transform);
                lastPositions[point.transform] = position;
                point.transform.position = position;
                points[i] = point.transform;
            }

            for (int i = 0; i < size - 1; i++)
            {
                allSprings.Add(new Spring(points[i], points[i + 1]));
            }
        }
        else //����
        {
            //����ʵ�
            Transform[,] points = new Transform[size, size];
            for (int i = 0, y = 0; i < size; i++, y += unit)
                for (int j = 0, x = 0; j < size; j++, x += unit)
                {
                    Vector3 position = new Vector3(x, y, 0);
                    GameObject point = new GameObject();
                    allPoints.Add(point.transform);
                    lastPositions[point.transform] = position;
                    point.transform.position = position;
                    points[i, j] = point.transform;
                }

            //��ӵ���
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    //���ݽṹ
                    if (x + 1 < size)
                        allSprings.Add(new Spring(points[y, x], points[y, x + 1]));
                    if (y + 1 < size)
                        allSprings.Add(new Spring(points[y, x], points[y + 1, x]));
                    //б��ṹ
                    if (x + 1 < size && y + 1 < size) //����
                        allSprings.Add(new Spring(points[y, x], points[y + 1, x + 1]));
                    if (x + 1 < size && y - 1 >= 0) //����
                        allSprings.Add(new Spring(points[y, x], points[y - 1, x + 1]));
                    //�����۵���
                    if (x + 2 < size)
                        allSprings.Add(new Spring(points[y, x], points[y, x + 2]));
                    if (y + 2 < size)
                        allSprings.Add(new Spring(points[y, x], points[y + 2, x]));
                }
        }


        foreach (Spring spring in allSprings)
        {
            spring.length = Vector3.Distance(spring.pointA.position, spring.pointB.position);
        }
    }

    float GetOneMinusDrag(Transform transform)
    {
        return transform == Selection.activeTransform ? 0 : 1 - drag;
    }

    void Update()
    {
        foreach (Transform point in allPoints)
        {
            //Verlet���ַ�������ȥ�˶Լ��ٶȵļ��㣬��ȫ����λ�ƿ����ʵ㣩
            //����ֱ�ӻ���λ���޸ĺͼ��㣬������ÿ�������㶼���Զ����������໥���ã���˵��ɽ��Դ�������
            //����ֱ���޸�λ��ʵ���ٶȸ��ģ��ʵ���ֱ㲻�ܷ���ĩβ���£�����Ჽ�����Ρ�
            Vector3 position = point.position;
            point.position += (position - lastPositions[point]) * GetOneMinusDrag(point);
            lastPositions[point] = position;
        }

        if (useGravity)
            foreach (Transform point in allPoints)
            {
                const float Gravity = 0.02f * 0.02f * -9.81f;
                point.position += new Vector3(0, Gravity, 0) * GetOneMinusDrag(point);
            }

        foreach (Spring spring in allSprings)
        {
            Transform pointA = spring.pointA;
            Transform pointB = spring.pointB;

            //ʹ���ʵ�����˵�ǰ�ٶȺ������λ����Ϣ�����������������ʼ��ֱ��ʩ����λ���ϣ����������ư���ʽŷ���ļ��㷽���������ģ���ȶ��ԣ�
            //��Ϊ���ܿ��ǵ��뵱ǰ���������໥���ã��Ӷ���ʵ��һ���ĵ�������������ö���ʵ�ֵ���������
            //����������Ҳ��ȱ�㣬����ʹ�ʵ㲻����ѭ�����غ㣨�ͷŵĵ��������������仯�������ǻ��ھ���Ķ�ֵ�����ڵ���ʵ�����޷��ص���ͬ�ĸ߶ȣ�������ʵ�������ķ�ʽû���������⡣
            Vector3 positionA = pointA.position;
            Vector3 positionB = pointB.position;
            Vector3 vector = positionA - positionB;
            // ��������ע��ʹ�õ��ʵ�λ���ǵ�ǰ֡��δ������ٶȻ���������λ�ã�Ҳ����˵������������Ӱ�죬ֻ����˲ʱ������㣬��Ȼ������ȶ�����Ҫ�����䱸�������ܣ�������ѭ�����غ㶨�ɣ������õ�����ʵ��
            // Vector3 vector = particleA.lastPosition - particleB.lastPosition;
            if (vector == Vector3.zero) vector = new Vector3(0, float.Epsilon, 0);
            float distance = vector.magnitude;
            //���˶��ɣ�����=����ϵ��*����*����
            float tendency = distance - spring.length; //���루��Ϊ������ʹ����ȡ����
            Vector3 direction = vector / distance; //����
            //����ϵ��ʵ�����������޵ģ�����ǰ�ʵ����Լ��λ�þ���Ϊa������߲��ܳ���2a����Ϊ������ÿ�ε�������Լ�����λ��ֻ��Խ��ԽԶ��
            //�ʴ˴��ĵ���ϵ���ǻ���λ�Ƶģ���ΧΪ[0,2]������ľ��������ķ�Χ���ƣ���˴˴��õ��ġ�������ʵ����λ�ơ�
            //������ϵ��Ϊ1ʱ���õ���λ�ƺ󶥵������Լ���㣬��Ϊ�����������˵��ɵľ���Ҫ�����λ�ơ�
            //��������Լ���㣬����ԶҲ���ܳ������Լ����1�����룬��Ϊ����1��ÿ�ε���ֻ��Խ��ԽԶ�����յ��ɽ�������
            //������������ۼ��ٶȣ����ٶ�Ҳ�����λ�ƣ����������Ե�������������������ʽŷ�����ַ����µ���������������ϵ�����ܳ���1��
            Vector3 move = elasticity * tendency * direction;

            float moveWeight;
            if (ropeOrCloth && drag > 0.99f)
            {
                //ģ�����
                moveWeight = 1;
            }
            else
            {
                //���ɻ�ʹ�����ʵ�һ��λ��������Լ��Ҫ��Ҫ���������ʵ�������ͬ�����䲻ͬ��λ��Ȩ�أ�
                //�Ա�������Ӱ����Ҳ����ȷ�ﵽԼ��λ�ã���Ȼ����Ҫ�Ļ���Ϊ�˷�ֹ�̶��㱻��ק��
                float oneMinusDragB = GetOneMinusDrag(pointB);
                float oneMinusDragA = GetOneMinusDrag(pointA);
                moveWeight = Mathf.Approximately(oneMinusDragB + oneMinusDragA, 0.0f) ? 0.5f : oneMinusDragB / (oneMinusDragB + oneMinusDragA);
            }
            pointB.position += moveWeight * move;
            pointA.position += (1 - moveWeight) * -move;
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
