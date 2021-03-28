using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Primitives;
using Roy_T.AStar.Paths;
using System;
using Grid = Roy_T.AStar.Grids.Grid;
using Roy_T.AStar.Graphs;

public class Main : MonoBehaviour
{
    private Path path;

    // Start is called before the first frame update
    void Start()
    {
        // Create directed graph with node a and b, and a one-way direction from a to b
        var nodeA = new Node(Position.Zero);
        var nodeB = new Node(new Position(10, 10));

        var traversalVelocity = Velocity.FromKilometersPerHour(100);
        nodeA.Connect(nodeB, traversalVelocity);

        var pathFinder = new PathFinder();
        path = pathFinder.FindPath(nodeA, nodeB, maximumVelocity: traversalVelocity);

        Debug.Log($"type: {path.Type}, distance: {path.Distance}, duration {path.Duration}");
        // prints: "type: Complete, distance: 14.14m, duration 0.51s"

        // Use path.Edges to get the actual path
        //TraversePath(path.Edges);
        //if (path != null && path.Edges != null)
        //{
        //    drawPath(path.Edges);
        //}
    }


    private LineRenderer curLine;//当前实例化出来的线

    private int posIndex;//当前线的Positions数组下标

    private Vector2 mouseEndPos;//鼠标结束的位置

    //线的参数
    public Color startColor = Color.black;//线开始的颜色
    public Color endColor = Color.black;//线结束的颜色
    public float width = 0.1f;//线宽度
    public int vertices = 90;//顶点数

    /// <summary>
    /// 初始化线
    /// </summary>
    private void InitLine()
    {
        posIndex = 0;

        curLine.startColor = startColor;
        curLine.endColor = endColor;
        curLine.startWidth = width;
        curLine.endWidth = width;
        curLine.numCapVertices = vertices;
        curLine.numCornerVertices = vertices;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            curLine = new GameObject("line").AddComponent<LineRenderer>();
            curLine.material = new Material(Shader.Find("Sprites/Default"));
            InitLine();
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 mouseStartPos = GetMousePosInWorldSpace();
            //优化性能
            if (Vector2.Distance(mouseStartPos, mouseEndPos) >= 0.1f)
            {
                AddPositions(mouseStartPos);
            }
        }
    }

    /// <summary>
    /// 将线段添加到当前线的Positions数组中
    /// </summary>
    private void AddPositions(Vector2 pos)
    {
        curLine.positionCount = ++posIndex;

        curLine.SetPosition(posIndex - 1, pos);

        mouseEndPos = pos;
    }

    /// <summary>
    /// 得到鼠标在世界空间下的坐标
    /// </summary>
    /// <returns></returns>
    private Vector2 GetMousePosInWorldSpace()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        return worldPos;
    }
}
