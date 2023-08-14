using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class AStar
{
	// 需要手动更改运行模式，哪个置1表示哪个模式激活
	public const int MODE_VH = 0; // VCOST启发模式，VCost是从源节点到该特定节点的路径中的总冲突数
	public const int MODE_N = 1;  // 一般模式
	

	public static int Mode = MODE_VH;

	static int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		return dstX + dstY;
		/*
		if (dstX > dstY)
			return 16 * dstY + 12 * (dstX - dstY);
		return 16 * dstX + 12 * (dstY - dstX);
		*/
	}

	/* 从节点列表中寻找最小代价的node，从两种启发式算法种进行选择
	 * get min cost node from list of nodes */

	/* 这里，H-Cost是用于优化搜索的启发式成本，一个节点的G-Cost是从源节点移动到该节点的实际成本，
	 * F-Cost= H-Cost + G-Cost。使用启发式成本（H-Cost）可以提供基于名称的搜索或最佳优先搜索。
	 * H-Cost = 当前节点与目标点的几何距离
	 * G-Cost = 当前节点与起始点的步数 */
	static Node getMinCostNode(List<Node> nodes)
	{
		Node node = nodes[0];//nodes[0]就是startNode
		for (int i = 1; i < nodes.Count; i++)
		{
			if (nodes[i].fCost < node.fCost)
				node = nodes[i];

			//Mode == MODE_VH   Mode == MODE_N
			if (true)
			{
				/* 原始的启发式算法H-cost */
				if (nodes[i].fCost == node.fCost)
					if (nodes[i].hCost < node.hCost)
						node = nodes[i];
			}
		}

		return node;
	}

	static Node focalList(List<Node> nodes)
	{
		float OmegaforECBS = 1.2f;
		Node node = nodes[0];//nodes[0]就是startNode
		List<Node> FocalList = new List<Node>();

		for (int i = 1; i < nodes.Count; i++)
		{
			if (nodes[i].fCost <= OmegaforECBS * node.fCost)
				FocalList.Add(nodes[i]);
		}

		for (int i = 1; i < FocalList.Count; i++)
		{
			/* 基于V-cost的新启发式算法 */
			if (FocalList[i].fCost == node.fCost)
				if (FocalList[i].vcost < node.vcost)
					node = FocalList[i];
		}


		return node;

	}
	

	/* 节点的VCost是从源节点到该特定节点的路径中的总冲突数 */
	static int GetVCost(Node n, List<List<Node>> soln, int curAgent)
	{
		if (soln == null || curAgent < 0) return 0;
		int vcost = 0;
		for (int i = 0; i < soln.Count; i++)
		{
			if (i != curAgent)
			{
				// 检查所有其他代理的冲突
				List<Node> path = soln[i];
				if (n.time < path.Count && path[n.time] == n)
					vcost++;
			}
		}
		return vcost;
	}

	/* Find the minimum path 
	while obeying the constraints (cstr) */
	static public List<Node> FindMinPath(Vector3 startPos, Vector3 targetPos, Grid grid, List<State> cstr, List<List<Node>> prevSoln, int curAgent)
	{
		grid.ResetNodes();

		// 获取起始点与目标点的坐标node
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

		/* openset 包含要访问的节点列表 */
		List<Node> openSet = new List<Node>();

		/* closedset 包含已经访问过的节点 */
		HashSet<Node> closedSet = new HashSet<Node>();

		/* 添加起始节点 */
		openSet.Add(startNode);

		while (openSet.Count > 0)
		{
			//从openset列表中找最小cost的节点，CBS or ECBS算法区别
			//Node node = getMinCostNode(openSet);
			Node node =  focalList(openSet);


			//把这个节点从openset列表中移除加入closedset列表中
			openSet.Remove(node);
			closedSet.Add(node);

			/* node就是目标节点
			 * if target node is found */
			if (node == targetNode)
			{
				return RetracePath(startNode, targetNode);
			}

			//找node中的8个Neighbours
			foreach (Node neighbour in grid.GetNeighbours(node))
			{
				/* 找到约束点会想办法绕过去 */
				if (cstr.Exists(
					state => state.node.gridX == neighbour.gridX && state.node.gridY == neighbour.gridY && state.time == node.time + 1
				)) continue;

				/* 检查相邻节点(Neighbours)是否是walkable并且没有被遍历过的
				 * check if neighbour is walkable and not already visited*/
				if (!neighbour.walkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				//计算newCostToNeighbour：当前节点与起始点的距离 + 当前节点与相邻节点neighbour的几何距离
				//一开始的node.gCost是0
				int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);

				/* if new cost is smaller than previous or if 
				 * neighbor is a new entry to open set */
				//如果newCostToNeighbour值小于当前节点的与起始点的步数，
				//或者openSet中不包含相邻节点neighbour（一开始的时候只有startNode），
				//那么这时要更新cost值并且添加startNode周围的neighbour进入openSet
				if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					/* update costs */
					neighbour.gCost = newCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);

					/* main path assign part */
					neighbour.parent = node;
					neighbour.time = neighbour.parent.time + 1;

					/* updating v cost */
					neighbour.vcost = neighbour.parent.vcost + GetVCost(neighbour, prevSoln, curAgent);

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
		}

		List<Node> path = new List<Node>();
		path.Add(startNode);
		return path;
	}

	static List<Node> RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Add(startNode);
		path.Reverse();

		return path;
	}
}
