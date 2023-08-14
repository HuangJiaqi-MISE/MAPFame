<div align="center">

[![MAPFame logo](https://raw.githubusercontent.com/HuangJiaqi-MISE/Image-storage/main/MAPFame.png)](http://www.mapfame.site/)    

**Multi-Agent Path Finding based on Advanced Methods and Evaluation (MAPFame)**


</div> 

Multi-Agent Path Finding (MAPF) is the problem of computing collision-free paths for a team of agents from their current locations to given destinations. Application examples include autonomous aircraft towing vehicles, automated warehouse systems, office robots, and game characters in video games. Practical systems must find high-quality collision-free paths for such agents quickly.

In this work, we developed an open-source MAPF simulation platform named Multi-Agent Path Finding based on Advanced Methods and Evaluation (MAPFame). Furthermore, several state-of-the-art MAPF algorithms have been implemented and integrated into MAPFame, such as A* algorithm, conflict-based search (CBS), bounded CBS (BCBS), and enhanced CBS (ECBS). This work intends to make the following unique contributions:

* It develops an open-source simulation platform named MAPFame for MAPF. It can provide the functionality to produce customizable visualizations of MAPF algorithms. This functionality can help researchers to evaluate the algorithm's further details and operational status. In addition, MAPFame supports users in building a simulation map by themselves. The user may then open the MAPFame extension in their web browser or executable file to interact with a 3D movie, according to the actual running environment of the algorithm. Hence, the simulation results are more convincing. As the algorithm runs, MAPFame records vital data sets for users to compare which MAPF algorithm is the most efficient for different environments.

* It publicizes a code repository with several MAPF algorithms with C# language. The source code, installation instructions, and dependencies of MAPFame are available at http://www.mapfame.site.

## Overview

![](https://raw.githubusercontent.com/HuangJiaqi-MISE/Image-storage/main/Overview.gif)

MAPFame utilizes a modular programming idea. It mainly integrates four key modules: a map creation module, a visualization module, a path planning module, and a data processing module.


## Visualization

![](https://raw.githubusercontent.com/HuangJiaqi-MISE/Image-storage/main/Visualization.gif)

A visualization module supports this feature. Both 3D film generation and the application of intuitive point-and-click user interaction are tied to the visualization module. For user interaction, MAPFame has four interactable interfaces. They are present in each of the four corners of the screen.

## Custom maps
![](https://raw.githubusercontent.com/HuangJiaqi-MISE/Image-storage/main/User-defined-Position.gif)

This instance map is created based on a logistics center. The AGV travels to drop-off shelves in accordance with each package's delivery path. The green dot indicates the start position of the AGV. The red dot indicates a target position, and the white block shows drop-off shelves. We can change the size and position of the shelves through the interactive interface. Of course, the position of AGVs can also be supported to change.

![](https://raw.githubusercontent.com/HuangJiaqi-MISE/Image-storage/main/User-defined-AGVs.gif)



## Citation
If you use this repository in your research or wish to cite it, please make a reference to our paper: 
```
@misc{MAPFame,
	author= {{Jiaqi Huang, Yangming Zhou and Mengchu Zhou}},
	year  = {2023},
	title = {Multi-Agent Path Finding based on Advanced Methods and Evaluation, {MAPFame}},
	note  = {\url{http://www.mapfame.site/}, 
	Last accessed on 2023-08-01},
}
```
