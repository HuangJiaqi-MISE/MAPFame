using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 网格化，用来创建网格并使用OnDrawGizmos可视化效果 Grid的GameObject X、Y轴要在场景正中

public class Grid : MonoBehaviour
{
	// 用来存储unwalkable的layermask
	public LayerMask unwalkableMask;

	// grid的大小是一个二位数组,这里的X对应的世界坐标的X轴，Y对应世界坐标轴的Z轴
	public Vector2 gridWorldSize;

	// grid中的node的半径（node立方体边长的一半）
	public float nodeRadius;

	// grid是二维的node数组
	public Node[,] grid;

	// grid中的node的直径
	float nodeDiameter;

	// Grid中的尺寸
	int gridSizeX, gridSizeY;

	public List<Color> colors = new List<Color>();


	public Node GetRandomNode()
	{
		for (int i = 0; i < 16; i++)
		{
			int col = Random.Range(0, (grid.GetLength(0)-2) );
			int row = Random.Range(0, (grid.GetLength(1)-2) );
			if (grid[col, row].walkable)
				return grid[col, row];
		}
		return grid[0, 0];
	}

	public Node GetNode(int x, int y)
	{
		return grid[x, gridSizeY - y - 1];
	}

	public void ResetNodes()
	{
		foreach (Node n in grid)
		{
			n.time = 0;
			n.parent = null;
		}
	}

	void AddColors()
	{
		colors.Add(Color.yellow);
		colors.Add(Color.red);
		colors.Add(Color.green);
		colors.Add(Color.blue);
		colors.Add(Color.black);
	}

	void Awake()
	{
		AddColors();
		// 根据grid的尺寸和node的尺寸计算node的数量并填入二维数组
		nodeDiameter = nodeRadius * 2;      // 直径 = 半径 * 2 = 0.5 * 2 = 1,其实也就是一个Node对应一个grid。

		// 坐标转化，将Scence面板中输入的gridWorldSize.X & Y转换为 gridSizeX 和 gridSizeY
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
		CreateGrid();
	}


	// 创建grid实例
	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];

		// 计算得到grid（从上往下看）左下角的世界坐标位置worldBottomLeft = （0，0，0）-（30，0，0）-（0，0，30） = (-30.0, 0.0, -30.0)
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				// 从最左下角点(-30.0, 0.0, -30.0)开始密铺平面
				// 每个点坐标 worldPoint = (-30.0, 0.0, -30.0) +（1，0，0）*（x * 1 + 0.5）+ （0，0，1）*（y * 1 + 0.5）
				//						 = (-30.0, 0.0, -30.0) + (0.5，0，0) + （0，0，0.5） 
				//						 = (-30.0, 0.0, -30.0) + (1.5，0，0) +  (0，0，1.5)
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

				// 判断是否有obstacles，如果有就将node设置为unwalkable
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

				// 创建每个node实例并给成员赋值
				grid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0 || x == y || x == -y)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					// 把符合条件的点假加入neighbours列表，grid是二维的node数组
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}


	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		// 分别计算X、Y坐标的百分比 = （世界坐标X & Z + 0.5 * Scence面板中输入的gridWorldSize.X & Y）/（gridWorldSize.X & Y）
		float percentX = (worldPosition.x + (gridWorldSize.x) / 2) / (gridWorldSize.x);
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		// 限制value在0,1之间并返回value。如果value小于0，返回0。如果value大于1,返回1，否则返回value
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		//if (percentX > 0.74f)
			//x++;
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}

	public List<List<Node>> paths = new List<List<Node>>();
	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

		if (grid != null)
		{
			foreach (Node n in grid)
			{
				Gizmos.color = (n.walkable) ? Color.white : Color.white;
				bool _is_path = false;
				if (paths != null)
					foreach (List<Node> path in paths)
						if (path.Contains(n))
						{
							Gizmos.color = colors[paths.FindIndex(tmp_path => tmp_path == path) % 5];
							_is_path = true;
						}

				if (_is_path || !n.walkable)
					Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .2f));

			}
		}
	}

}