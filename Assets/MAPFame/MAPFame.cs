using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CBS : MonoBehaviour
{

	public Transform[] agents;
	public Transform[] targets;

	public Transform[] Dagents;
	public Transform[] Dtargets;

	// Vector3[] agentPositions;

	private Button ButtonStart;
	public static bool cbsComplete = false;

	public int maxAgents;
	//public int maxDAgents = 5;
	int nAgents;
	int dAgents;


	public bool testing;
	public bool ECBS_activate;
	string logdir = "Assets/Resources/";

	Grid grid;

	
	void Awake()
	{
		// 三目运算，if(agents.Length < targets.Length)为真，则agents.Length，否则targets.Length
		nAgents = agents.Length < targets.Length ? agents.Length : targets.Length;
		nAgents = nAgents < maxAgents ? nAgents : maxAgents;

		for (int i = nAgents; i < agents.Length; i++)
			agents[i].gameObject.SetActive(false);

		for (int i = nAgents; i < targets.Length; i++)
			targets[i].gameObject.SetActive(false);
		/*
		dAgents = Dagents.Length < Dtargets.Length ? Dagents.Length : Dtargets.Length;
		dAgents = dAgents < maxDAgents ? dAgents : maxDAgents;
		*/


		/*
		for (int i = dAgents; i < Dagents.Length; i++)
			Dagents[i].gameObject.SetActive(false);

		for (int i = dAgents; i < Dtargets.Length; i++)
			Dtargets[i].gameObject.SetActive(false);
		*/

		// 初始化DoubleAgent的坐标
		//InitdAgents();

		// get the whole grid layout
		grid = GetComponent<Grid>();

	}
	// 初始化
	void InitdAgents()
	{
		Vector3 pos = new Vector3(-1, 0, 0);
		for (int i = 0; i < dAgents; i++)
			Dagents[i].position = agents[i].position + pos;
	}


	// “开始仿真”按键捕获，若捕获成功执行ClickStart()函数，将cbsComplete标志位置为1
	void Start()
	{
		ButtonStart = GameObject.Find("ButtonStart").GetComponent<Button>();
		ButtonStart.onClick.AddListener(ClickStart);
	}

	public void ClickStart()
	{
		cbsComplete = true;
	}

	void Update()
	{
		// 主要算法循环
		if (cbsComplete)
		{
			cbsComplete = false;
			if (testing)
				AStar.Mode = AStar.MODE_VH;
			else
				AStar.Mode = AStar.MODE_N;
			// 开启Unity协程
			StartCoroutine(StepCBS());
		}
	}

	void Logtime(string str)
	{
		StreamWriter writer;
		if (ECBS_activate)
			writer = new StreamWriter(logdir + "ECBS_Time.txt", true);
		else
			writer = new StreamWriter(logdir + "CBS_Time.txt", true);

		writer.WriteLine(str);
		writer.Close();
	}


	void Log_Cost(string str)
	{
		StreamWriter writer;
		// 实验数据缓存文件判断
		if (ECBS_activate)
			writer = new StreamWriter(logdir + "ECBS_Cost.txt", true);
		else
			writer = new StreamWriter(logdir + "CBS_Cost.txt", true);
		writer.WriteLine(str);
		writer.Close();
	}

	void Log_Node(string str)
	{
		StreamWriter writer;
		// 实验数据缓存文件判断
		if (ECBS_activate)
			writer = new StreamWriter(logdir + "ECBS_Node.txt", true);
		else
			writer = new StreamWriter(logdir + "CBS_Node.txt", true);
		writer.WriteLine(str);
		writer.Close();
	}

	/* 获取新约束、以前的路径解决方案和新的受约束代理的列表 */
	List<List<Node>> GetSolution(List<State>[] cstr, List<List<Node>> prevSoln = null, int cstrAgent = -1)
	{
		//存储求解路径信息的列表，emptyCstr = 10
		List<List<Node>> soln = new List<List<Node>>();

		List<Node> cstrPath = new List<Node>();

		// solve constraint agent（cstrAgent） first
		if (cstrAgent != -1)
		{
			cstrPath = AStar.FindMinPath(agents[cstrAgent].position, targets[cstrAgent].position, grid, cstr[cstrAgent], prevSoln, cstrAgent);
			prevSoln[cstrAgent] = cstrPath;
		}

		// solve the rest of the agents，
		// NO. 001
		for (int i = 0; i < cstr.Length; i++)
		{
			// 这个循环在一次进入GetSolution执行，或者其他没有冲突的Agent可执行
			if (cstrAgent == -1 || i != cstrAgent)
			{
				List<Node> path = AStar.FindMinPath(agents[i].position, targets[i].position, grid, cstr[i], prevSoln, i);
				// 第一次执行的时候prevSoln已经在GetSolution函数传入时赋值为null，这个If不执行
				// 当有一个有冲突的Agent重新运算路径后，之前的A*已经为它把路径数据单独放入prevSoln中，这里要把其他没有冲突的Agent路径更新至prevSoln
				if (prevSoln != null) prevSoln[i] = path;

				// 这个Add是在for循环中的，每Add一次，对应会添加一个agent的路径path到soln列表中，这时候soln列表的键值会自动+1
				// 由此循环，之后所有没有冲突的Agent坐标会进行新的一轮更新，重新放入soln
				soln.Add(path);
				
			}
			if (cstrAgent != -1 && i == cstrAgent)
				soln.Add(cstrPath);
		}
		
		// 自己加的 = CP-CBS
		/*
		if (cstrAgent == -1)
		{
			//存储求解路径信息的列表，emptyCstr = 10
			List<List<Node>> DAGVsoln = new List<List<Node>>();
			// 将已经计算好的路径copy给Double-AGV
			for (int i = 0; i < dAgents; i++)
			{
				DAGVsoln.Add(soln[i]);
				//Debug.Log("世界坐标为：" + "(" + DAGVsoln[i] + ")");
			}


			// 在求解坐标集合的起始位置添加DAGV的起始坐标
			for (int i = 0; i < 5; i++)
			{
				Node DAGVstartNode = grid.NodeFromWorldPoint(Dagents[i].position);
				Debug.Log("DAGV起点世界坐标为：" + "(" + DAGVstartNode.worldPosition + ")");
				DAGVsoln[i].Insert(0, DAGVstartNode);
				//Debug.Log(i);
			}


			// 拼接矩阵
			soln.AddRange(DAGVsoln);
		}
		*/


		/*
		foreach (var item in soln)
		{
			foreach (var item2 in item)
			{
				Debug.Log("世界坐标为：" + item2.worldPosition.ToString());
				//Debug.Log("坐标为： " + "（" + item2.gridX.ToString() + "," + item2.gridY.ToString() + "）");
			}
		}
		*/
		/*
		foreach (var item in DAGVsoln)
		{
			foreach (var item2 in item)
			{
				Debug.Log("世界坐标为：" + "(" + item2.worldPosition.ToString() + ")");
				//Debug.Log("坐标为： " + "（" + item2.gridX.ToString() + "," + item2.gridY.ToString() + "）");
			}
		}
		*/
		return soln;
	}


	int GetSolutionCost(List<List<Node>> paths)
	{
		int cost = 0;
		foreach (List<Node> path in paths)
			cost += path.Count;
		return cost;
	}

	/* 从OPEN列表中筛选cost较小的节点，如果cost相等那就寻找约束数ConstraintsCount较小的 */
	static CTNode GetMinCostNode(List<CTNode> nodes)
	{
		CTNode node = nodes[0];
		for (int i = 1; i < nodes.Count; i++)
			if (nodes[i].cost < node.cost)
				node = nodes[i];
			else if (nodes[i].cost == node.cost
			&& nodes[i].GetConstraintsCount() < node.GetConstraintsCount())
				node = nodes[i];

		return node;
	}


	IEnumerator StepCBS()
	{
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Start();

		grid.paths = null;
		// CBS算法High level的OPEN列表
		List<CTNode> OPEN = new List<CTNode>();
		// 用于储存当前冲突的列表，Conflict类包括：agent编号,Node(walkable,worldPos,gridX,gridY)，第几次
		List<Conflict> curConflicts = new List<Conflict>();

		// 空约束存放列表，用A star算法中的每一个特定状态State类描述并初始化，里面有node和time
		List<State>[] emptyCstr = new List<State>[nAgents];
		//List<State>[] emptyCstr = new List<State>[nAgents + dAgents];
		// 有几对agent就初始化几个emptyCstr列表: 10 + 5 = 15
		for (int i = 0; i < emptyCstr.Length; i++)
			emptyCstr[i] = new List<State>();

		// List嵌套，里面以List<Node>为基本单位，有很多个。
		List<List<Node>> soln = GetSolution(emptyCstr);
		// 计算全部path所花费的代价
		int cost = GetSolutionCost(soln);

		// 添加根节点到CT树，包括：空的约束集合emptyCstr，所有agent路径解的集合soln（可能有冲突），总共的代价cost
		// emptyCstr是上面初始化已经定义好的，cstr会根据传入值进行变量的复制，emptyCstr不会复制
		CTNode curNode = new CTNode(emptyCstr, soln, cost);
		OPEN.Add(curNode);

		// 已遍历的节点数
		int nodesTraversed = 0;

		//NO. 002
		while (OPEN.Count > 0)
		{
			nodesTraversed++;

			// 寻找代价cost小或约束数(冲突点)少的节点
			curNode = GetMinCostNode(OPEN);
			// 从open列表弹出这个节点
			OPEN.Remove(curNode);

			// 把当前节点的解赋值给grid.paths，在OnDrawGizmos中进行可视化处理
			grid.paths = curNode.soln;



			// 进行冲突检测，如果检测到冲突。则添加信息(curAgents, paths[i][t], t)到conflicts列表
			// curAgents表示有冲突agent的序数
			curConflicts = GetConflicts(curNode.soln);

			// Debug.Log(curNode.cost);
			yield return 0;
			if (curConflicts.Count == 0)
			{
				//Debug.Log("final cost -> " + curNode.cost);
				stopwatch.Stop();
				//Debug.Log("Time using -> " + stopwatch.ElapsedMilliseconds.ToString());
				Logtime(stopwatch.ElapsedMilliseconds.ToString());
				stopwatch.Reset();
				break;
			}


			else if (curConflicts.Count > 0)
			{
				foreach (Conflict conflict in curConflicts)
				{
					foreach (int agentID in conflict.agents)
					{
						// 新建newConstraints列表，newConstraints[i]对应agent[i]，newConstraints包含两个元素：node和time
						List<State>[] newConstraints = new List<State>[nAgents];
						// 复制已经存在的constraint约束，new之后newConstraints.Length已经为10，进入循环
						for (int i = 0; i < newConstraints.Length; i++)
							// 第一次执行时，emptyCstr传入curNode，这时候cstr为空，
							// 但是下面经过newConstraints后约束不为空
							newConstraints[i] = new List<State>(curNode.cstr[i]);

						// 新增约束
						//Debug.Log("存在冲突的agent序数："+agentID);
						/*
						if (agentID < 30)
						{
							newConstraints[agentID].Add(new State(conflict.node, conflict.time));
							// 解决新约束
							List<List<Node>> newSoln = GetSolution(newConstraints, curNode.soln, agentID);
							int newCost = GetSolutionCost(newSoln);
							CTNode newNode = new CTNode(newConstraints, newSoln, newCost);
							OPEN.Add(newNode);
						}
						*/
						newConstraints[agentID].Add(new State(conflict.node, conflict.time));
						// 解决新约束
						List<List<Node>> newSoln = GetSolution(newConstraints, curNode.soln, agentID);
						int newCost = GetSolutionCost(newSoln);
						CTNode newNode = new CTNode(newConstraints, newSoln, newCost);
						OPEN.Add(newNode);

					}
				}
			}
		}
		// 版本修改：webGL由于打包问题不能携带txt文件，注意根据打包的平台选择是否打印Log
		// 数据采集: 结点数 + 代价
		Log_Node(nodesTraversed.ToString());
		Log_Cost(curNode.cost.ToString());


		if (AStar.Mode == AStar.MODE_VH)
		{
			AStar.Mode = AStar.MODE_N;
			StartCoroutine(StepCBS());
		}
		else if (AStar.Mode == AStar.MODE_N)
		{
			StartCoroutine(FlyDrones());
		}
	}

	IEnumerator FlyDrones()
	{
		int minTimeStep = int.MaxValue;
		int MAXSTEPS = 5;
		float speed = grid.nodeRadius * 100 / MAXSTEPS;

		foreach (List<Node> path in grid.paths)
			if (path.Count != 1 && path.Count < minTimeStep)
				minTimeStep = path.Count;

		if (minTimeStep == int.MaxValue)
		{
			Debug.Log("Deadlock!");
			yield break;
		}

		// for each time step
		for (int t = 0; t < minTimeStep; t++)
		{
			// for each agent
			if (!testing && t > 0)
				for (int step = 0; step < MAXSTEPS; step++)
				{
					for (int i = 0; i < nAgents; i++)
						if (t < grid.paths[i].Count)
						{
							agents[i].position = Vector3.MoveTowards(agents[i].position, grid.paths[i][t].worldPosition, speed * Time.deltaTime);						}
					yield return 0;
				}

			for (int i = 0; i < nAgents; i++)
			{
				if (t < grid.paths[i].Count)
					agents[i].position = grid.paths[i][t].worldPosition;

				/*
				if (t < grid.paths[i].Count && i < 5 && t > 1)
					Dagents[i].position = grid.paths[i][t].worldPosition;
				*/

				// change target location to new random node
				if (grid.paths[i].Count != 0 && t == grid.paths[i].Count - 1)
				{
					Vector3 newNodePos = grid.GetRandomNode().worldPosition;
					Debug.Log("new StartPosition： " + newNodePos);
					targets[i].position = new Vector3((int)newNodePos.x, 1, (int)newNodePos.z);
				}
			}
			yield return 0;
		}

		cbsComplete = true;
	}

	List<Conflict> GetConflicts(List<List<Node>> paths)
	{
		List<Conflict> conflicts = new List<Conflict>();

		bool conflictFound = false;
		bool agentLeft = false;

		for (int t = 0; !conflictFound; t++, agentLeft = false)
		{
			for (int i = 0; i < paths.Count && !conflictFound; i++)
			{
				for (int j = i + 1; j < paths.Count && !conflictFound; j++)
				{
					if (i != j)
					{
						// 遍历查找的时候，t时刻不能超过单个agent路径的长度
						if (t < paths[i].Count && t < paths[j].Count)
						{
							agentLeft = true;

							/* CASE 1 
									   t时刻
								0: A B C F	<---  第i个agent
								1: D E C G	<---  第j个agent
									   |
									    ------ conflict (same node at same time)
							
							*/
							if (paths[i][t] == paths[j][t])
							{
								// 用来存储冲突Agent的序号，列表里都是整型int变量
								//Debug.Log("conflicts: same node at same time");
								List<int> curAgents = new List<int>();
								curAgents.Add(i);
								curAgents.Add(j);
								conflicts.Add(new Conflict(curAgents, paths[i][t], t));
								conflictFound = true;
								continue;
							}

							/* CASE 2
								0: A B C E
								1: D C B F
									 | |
									  ------- conflict (crossing each other)
							*/
							if (t + 1 < paths[i].Count && t + 1 < paths[j].Count)
							{
								if (paths[i][t] == paths[j][t + 1] && paths[i][t + 1] == paths[j][t])
								{
									//Debug.Log("conflicts: crossing each other");
									List<int> curAgents = new List<int>();
									curAgents.Add(i);
									conflicts.Add(new Conflict(curAgents, paths[i][t + 1], t + 1));

									curAgents = new List<int>();
									curAgents.Add(j);
									conflicts.Add(new Conflict(curAgents, paths[j][t + 1], t + 1));
									conflictFound = true;
									continue;
								}
							}
						}

						// 检查静止物体碰撞
						if (t > 0 && t < paths[j].Count && paths[i].Count == 1 && paths[j][t] == paths[i][0])
						{
							List<int> curAgents = new List<int>();
							curAgents.Add(j);
							conflicts.Add(new Conflict(curAgents, paths[i][0], t));
							conflictFound = true;
							continue;
						}
						if (t > 0 && t < paths[i].Count && paths[j].Count == 1 && paths[i][t] == paths[j][0])
						{
							List<int> curAgents = new List<int>();
							curAgents.Add(i);
							conflicts.Add(new Conflict(curAgents, paths[j][0], t));
							conflictFound = true;
							continue;
						}

					}
				}
			}

			if (!agentLeft) break;
		}

		return conflicts;
	}
}
