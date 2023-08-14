using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 申明和定义一个节点类，用于创建节点、进行坐标转换、并设置属性。
public class Node
{
	// 定义布尔型walkable用于判断网格化的节点是否可以walk
	public bool walkable;
	// node中心的世界坐标位置
	public Vector3 worldPosition;
	// 在Grid二维坐标中的位置
	public int gridX;
	public int gridY;

	public int hCost;
	public int gCost;
	public int vcost;
	// 父节点 也就是路径中该节点的上一个节点
	public Node parent = null;
	public int time = 0;

	//Node方法构造函数
	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
	{
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	}

	public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}
}

/* A star算法中的每一个特定状态 */
public class State
{
	public Node node; // node position
	public int time = 0;  // time step

	public State(Node _node, int _time)
	{
		node = _node;
		time = _time;
	}
}

// “冲突”类:包括agent编号,Node(walkable,worldPos,gridX,gridY)，第几次
public class Conflict
{
	public Node node;
	public int time = 0;
	public List<int> agents;

	public Conflict(List<int> _agents, Node _node, int _time)
	{
		node = _node;
		agents = new List<int>(_agents);
		time = _time;
	}
}

/* Constraint tree node */
public class CTNode
{
	public List<State>[] cstr; // constraints
	public int cost;
	public List<List<Node>> soln = new List<List<Node>>(); // solution

	public CTNode(List<State>[] _constraints, List<List<Node>> _solution, int _cost)
	{
		cstr = new List<State>[_constraints.Length];
		for (int i = 0; i < cstr.Length; i++)
		{
			cstr[i] = new List<State>(_constraints[i]);
		}

		foreach (List<Node> path in _solution)
			soln.Add(new List<Node>(path));

		cost = _cost;
	}


	public int GetConstraintsCount()
	{
		int count = 0;
		for (int i = 0; i < cstr.Length; i++)
		{
			count += cstr[i].Count;
		}
		return count;
	}
}