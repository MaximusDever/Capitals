using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class PathRetrive : MonoBehaviour
{
    public static List<GameObject> FindPath(GameObject startPos, GameObject targetPos, GameObject hexparent)
    {
        List<GameObject> path = new List<GameObject>();

        // Create a list of open nodes and closed nodes
        List<Node> openNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>();

        // Create a start node and goal node
        Node startNode = new Node(startPos.transform.position, startPos);
        Node goalNode = new Node(targetPos.transform.position, targetPos);

        // Add the start node to the open list
        openNodes.Add(startNode);

        while (openNodes.Count > 0)
        {
            // Get the node with the lowest F cost from the open list
            Node currentNode = openNodes.OrderBy(n => n.F).First();

            // Move the current node from the open list to the closed list
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            // If the current node is the goal node, reconstruct the path and return it
            if (currentNode.Position == goalNode.Position)
            {
                path = ReconstructPath(currentNode);
                break;
            }

            // Get the neighboring nodes
            List<Node> neighbors = GetNeighbors(currentNode, hexparent);

            foreach (Node neighbor in neighbors)
            {
                // If the neighbor is in the closed list, skip it
                if (closedNodes.Contains(neighbor))
                    continue;

                // Calculate the cost to move to the neighbor
                int newCostToNeighbor = currentNode.G + GetDistance(currentNode, neighbor);

                // If the neighbor is not in the open list or the new cost is lower
                if (!openNodes.Contains(neighbor) || newCostToNeighbor < neighbor.G)
                {
                    // Update the neighbor's cost and set its parent to the current node
                    neighbor.G = newCostToNeighbor;
                    neighbor.H = GetDistance(neighbor, goalNode);
                    neighbor.Parent = currentNode;

                    // If the neighbor is not in the open list, add it
                    if (!openNodes.Contains(neighbor))
                        openNodes.Add(neighbor);
                }
            }
        }

        return path;
    }

    private static List<Node> GetNeighbors(Node node, GameObject hexparent)
    {
        List<Node> neighbors = new List<Node>();

        // Perform pathfinding algorithm on each child game object
        foreach (Transform child in hexparent.transform)
        {
            // Create a node for each child game object and add it to the neighbors list
            neighbors.Add(new Node(child.position, child.gameObject));
        }

        return neighbors;
    }

    private static int GetDistance(Node nodeA, Node nodeB)
    {
        // Calculate the distance between two nodes (e.g., using Manhattan distance or Euclidean distance)
        return 0;  // Replace with actual distance calculation
    }

    private static List<GameObject> ReconstructPath(Node currentNode)
    {
        List<GameObject> path = new List<GameObject>();

        while (currentNode != null)
        {
            path.Add(currentNode.GameObject);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }
}

public class Node
{
    public Vector3 Position { get; set; }
    public int G { get; set; }  // Cost from start node to current node
    public int H { get; set; }  // Heuristic estimate of cost from current node to goal node
    public int F { get { return G + H; } }  // Total cost of the node
    public Node Parent { get; set; }  // Parent of the current node
    public GameObject GameObject { get; set; }  // Reference to the game object associated with the node

    public Node(Vector3 position, GameObject gameObject)
    {
        Position = position;
        GameObject = gameObject;
    }
}
