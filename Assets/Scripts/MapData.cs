using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun.Demo.Asteroids;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.Demo.Asteroids
{
    public class MapData : MonoBehaviourPunCallbacks
    {
        // Start is called before the first frame update
        public GameObject landHex; // GameObject containing all land hexes on the map
        public GameObject seaHex; // GameObject containing all sea hexes on the map
        public GameObject cell;
        public Slider Controller;
        public List<GameObject> NationBordHex;
        private GameManager gameManager; // Reference to the GameManager script
        private MapData map; // Reference to the Map script
        private List<GameObject> BordRange;
        public float width;
        public float height;
        public GameObject unithex1;
        public GameObject unithex2;
        // ...
        private void Update()
        {
        }
        private void Start()
        {
            width = cell.GetComponent<RectTransform>().rect.width;
            gameManager = FindObjectOfType<GameManager>();
            map = FindObjectOfType<MapData>();
        }
        public void GetHexAroundCapital() //Get available Hexes to place unit around capital
        {
            List<GameObject> seahexes = new List<GameObject>();
            GameObject Given_Unit = gameManager.Unit_Panel.transform.Find("capital(Clone)").gameObject;
            BordRange = GetlandHexesInRangeFromCapital(Given_Unit, 4, landHex);
            BordRange.AddRange(GetseaHexesInRangeFromCapital(Given_Unit, 4));
        }
        public List<GameObject> GetlandHexesInRangeFromCapital(GameObject unit, int range, GameObject hexparent)
        {
            // Perform a BFS search to find all hexes within the given range
            List<GameObject> hexesInRangeFromCapital = new List<GameObject>();
            Queue<(GameObject, int)> queue = new Queue<(GameObject, int)>();
            HashSet<GameObject> visited = new HashSet<GameObject>();
            queue.Enqueue((unit, 0));
            visited.Add(unit);
            while (queue.Count > 0)
            {
                (GameObject currentHex, int distance) = queue.Dequeue();
                if (distance <= range)
                {
                    hexesInRangeFromCapital.Add(currentHex);
                    if (currentHex.tag == "Edge" && currentHex.GetComponent<Connected_hex>().connected_hexagon!=null)
                    {
                        GameObject additinal_hex = currentHex.GetComponent<Connected_hex>().connected_hexagon.gameObject;
                        hexesInRangeFromCapital.Add(additinal_hex);
                    }
                }
                // Determine whether the current hex is on land or sea
                foreach (GameObject neighbor in GetNeighbors(currentHex, hexparent))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue((neighbor, distance + 1));
                        visited.Add(neighbor);
                    }
                }
            }
            return hexesInRangeFromCapital;
        }
        public List<GameObject> GetseaHexesInRangeFromCapital(GameObject unit, int range)
        {
            List<GameObject> result = new List<GameObject>();
            for (int i = 0; i < seaHex.transform.childCount; i++)
            {
                GameObject hex = seaHex.transform.GetChild(i).gameObject;
                float distance = Vector3.Distance(unit.transform.position, hex.transform.position);
                float unithex_distance= Vector3.Distance(unithex1.transform.position, unithex2.transform.position);
                if (distance <= range * unithex_distance)
                {
                    result.Add(hex);
                    if (hex.tag == "Edge" && hex.GetComponent<Connected_hex>().connected_hexagon!=null)
                    {
                        GameObject additional_hex = hex.GetComponent<Connected_hex>().connected_hexagon.gameObject;
                        result.Add(additional_hex);
                    }
                }
            }

            return result;
        } 
        
        public List<GameObject> GetHexesInRange(GameObject unit, int range)
        {
            List<GameObject> hexesInRange = new List<GameObject>();
            hexesInRange = GethexRange(unit,range);
      
            return hexesInRange;
        }
        public List<GameObject> GethexRange(GameObject unit, int range)
        {
            List<GameObject> hexesInRange = new List<GameObject>();
            // Determine whether the unit is on a land hex or a sea hex
            GameObject hexParent = unit.GetComponent<Unit>().unitType == Unit.UnitType.Navy ? seaHex : landHex;
            // Perform a BFS search to find all hexes within the given range
            Queue<(GameObject, int)> queue = new Queue<(GameObject, int)>();
            HashSet<GameObject> visited = new HashSet<GameObject>();
            queue.Enqueue((unit, 0));
            visited.Add(unit);
            while (queue.Count > 0)
            {
                (GameObject currentHex, int distance) = queue.Dequeue();
                if (distance <= range) // change here
                {
                    if (currentHex.tag == "Edge" || currentHex.tag=="Cannal")
                    {
                        if (currentHex.GetComponent<Connected_hex>().connected_hexagon.gameObject != null)
                        {
                            GameObject connected_hex = currentHex.GetComponent<Connected_hex>().connected_hexagon.gameObject;
                            if (connected_hex.transform.parent.name == currentHex.transform.parent.name)
                            {
                                hexesInRange.Add(connected_hex);
                            }
                        }
                    }
                    hexesInRange.Add(currentHex);
                }
                foreach (GameObject neighbor in GetNeighbors(currentHex, hexParent))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue((neighbor, distance + 1));
                        visited.Add(neighbor);
                    }
                }
            }
            return hexesInRange;
        }
        public bool HasOppositeHexNextToUnit(GameObject unit)
        {
            bool hasOppositeHex = false;
            // Determine whether the unit is an army or a navy
            Unit.UnitType unitType = unit.GetComponent<Unit>().unitType;
            // Determine the opposite hex parent based on the unit type
            GameObject oppositeHexParent = unitType == Unit.UnitType.Army ? seaHex : landHex;
            // Determine whether the unit is on a land hex or a sea hex
            GameObject hexParent = unitType == Unit.UnitType.Navy ? seaHex : landHex;
            // Perform a BFS search to find all hexes within the given range
            Queue<(GameObject, int)> queue = new Queue<(GameObject, int)>();
            HashSet<GameObject> visited = new HashSet<GameObject>();
            queue.Enqueue((unit, 0));
            visited.Add(unit);
            while (queue.Count > 0)
            {
                (GameObject currentHex, int distance) = queue.Dequeue();
                if (distance <= 0)
                {
                    if (hexParent == landHex && HasOppositeHexNeighbor(currentHex, oppositeHexParent))
                    {
                        hasOppositeHex = true;
                        break;
                    }
                    else if (hexParent == seaHex && HasOppositeHexNeighbor(currentHex, oppositeHexParent))
                    {
                        hasOppositeHex = true;
                        break;
                    }
                }
                foreach (GameObject neighbor in GetNeighbors(currentHex, hexParent))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue((neighbor, distance + 1));
                        visited.Add(neighbor);
                    }
                }
            }
            return hasOppositeHex;
        }
        private bool HasOppositeHexNeighbor(GameObject hex, GameObject oppositeHexParent)
        {
            foreach (GameObject neighbor in GetNeighbors(hex, oppositeHexParent))
            {
                return true;
            }
            return false;
        }
        public GameObject GetHexAtPosition(GameObject hexParent, Vector3 position, GameObject unit)
        {
            foreach (Transform child in hexParent.transform)
            {
                width = (cell.GetComponent<RectTransform>().rect.width) * Controller.value;
                height = (cell.GetComponent<RectTransform>().rect.width) * Controller.value;
                if (unit.GetComponent<Unit_Drop>().unitType == Unit_Drop.UnitType.Capital)
                {
                    if (child.position.x - width / 2 < position.x && child.position.x + width / 2 > position.x && child.position.y - height / 2 < position.y && child.position.y + height / 2 > position.y)
                    {
                        return child.gameObject;
                    }
                    else
                    {
                        GameObject hex = GetHexAtPosition(child.gameObject, position, unit);
                        if (hex != null)
                        {
                            return hex;
                        }
                    }
                }
                else
                {
                    GameObject hexParentForinit = unit.GetComponent<Unit_Drop>().unitType == Unit_Drop.UnitType.Navy ? seaHex : landHex;
                    if (gameManager.Unit_Panel.childCount <= 0)
                    {
                        for (int i = 0; i < gameManager.Unit_Panel.childCount; i++)
                        {
                            GameObject childunit = gameManager.Unit_Asset_Panel.GetChild(i).gameObject;
                            float width = childunit.GetComponent<RectTransform>().rect.width;
                            if (childunit.GetComponent<Unit>().unitType == Unit.UnitType.Navy && unit.transform.position.x > childunit.transform.position.x - width / 2 && unit.transform.position.x < childunit.transform.position.x + width / 2 && unit.transform.position.y > childunit.transform.position.y - width / 2 && unit.transform.position.y < childunit.transform.position.y + width / 2)
                            {
                                return childunit;
                            }
                        }
                    }
                    //Hex_init(hexParentForinit);
                    for (int i = 0; i < BordRange.Count; i++)
                    {
                        if (child.name != "capital(Clone)" && child.name == BordRange[i].name)
                        {
                            if (child.position.x - width / 2 < position.x && child.position.x + width / 2 > position.x && child.position.y - height / 2 < position.y && child.position.y + height / 2 > position.y)
                            {
                                return child.gameObject;
                            }
                            else
                            {
                                GameObject hex = GetHexAtPosition(child.gameObject, position, unit);
                                if (hex != null)
                                {
                                    return hex;
                                }
                            }
                        }
                    }
                }
            }
            return null; // Return null if no landHex is found at the position
        }
        public GameObject GetHexAtUnit(GameObject unit, GameObject hexParent)
        {
            foreach (Transform child in hexParent.transform)
            {
                width = (cell.GetComponent<RectTransform>().rect.width) * Controller.value;
                height = (cell.GetComponent<RectTransform>().rect.width) * Controller.value;

                if (child.position.x - width / 2 < unit.transform.position.x && child.position.x + width / 2 > unit.transform.position.x && child.position.y - height / 2 < unit.transform.position.y && child.position.y + height / 2 > unit.transform.position.y)
                {
                    return child.gameObject;
                }
                else
                {
                    GameObject hex = GetHexAtUnit(child.gameObject, unit);
                    if (hex != null)
                    {
                        return hex;
                    }
                }
            }
            return null;
        }
        public List<GameObject> UpdateHexesInRange(GameObject unit, int range)
        {
            List<GameObject> hexesInRange = new List<GameObject>();
            GameObject landHexParent = landHex; // Replace this with the actual reference to the land hex parent
            GameObject seaHexParent = seaHex; // Replace this with the actual reference to the sea hex parent
            Hex_init(landHex);
            Hex_init(seaHex);

            // Perform a BFS search to find all hexes within the given range
            Queue<(GameObject, int)> queue = new Queue<(GameObject, int)>();
            HashSet<GameObject> visited = new HashSet<GameObject>();
            queue.Enqueue((unit, 0));
            visited.Add(unit);
            while (queue.Count > 0)
            {
                (GameObject currentHex, int distance) = queue.Dequeue();
                if (distance <= range) // change here
                {
                    hexesInRange.Add(currentHex);
                    if (currentHex.tag == "Edge" && currentHex.GetComponent<Connected_hex>().connected_hexagon != null)
                    {
                        GameObject additional_hex = currentHex.GetComponent<Connected_hex>().connected_hexagon.gameObject;
                        hexesInRange.Add(additional_hex);
                    }
                }
                foreach (GameObject neighbor in GetNeighbors(currentHex, landHexParent))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue((neighbor, distance + 1));
                        visited.Add(neighbor);
                    }
                }
                foreach (GameObject neighbor in GetNeighbors(currentHex, seaHexParent))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue((neighbor, distance + 1));
                        visited.Add(neighbor);
                    }
                }
            }

            return hexesInRange;
        }

        private List<GameObject> GetNeighbors(GameObject hex, GameObject hexParent)
        {
            // Get the list of neighboring hexes for the given hex
            List<GameObject> neighbors = new List<GameObject>();
            foreach (Transform child in hexParent.transform)
            {
                if (Vector3.Distance(hex.transform.position, child.transform.position) <= (cell.GetComponent<RectTransform>().rect.width) * Controller.value)
                {
                    neighbors.Add(child.gameObject);
                }
            }
            return neighbors;
        }
        public void Hex_init(GameObject hexParent)
        {
            foreach (Transform child in hexParent.transform)
            {
                child.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/GridHex/Hex0");
            }
        }
        public bool DoubleStackCheck(GameObject UNIT)  //Once Drag and Drop double stack disable/enable check
        {
            if (gameManager.Unit_Panel.childCount == 0)
            {
                return true;
            }
            else
            {
                int unit_cnt = 0;
                foreach (Transform unit in gameManager.Unit_Panel.transform)
                {

                    if (unit.transform.position.x - map.width / 2 < UNIT.transform.position.x && unit.transform.position.x + map.width / 2 > UNIT.transform.position.x && unit.transform.position.y - map.height / 2 < UNIT.transform.position.y && unit.transform.position.y + map.height / 2 > UNIT.transform.position.y)
                    {
                        unit_cnt++;
                    }
                }
                foreach (Transform unit in gameManager.Unit_Panel.transform)
                {
                    if (!(UNIT.GetComponent<Unit_Drop>().unitType == Unit_Drop.UnitType.AirForce) && unit == gameManager.Unit_Panel.transform.GetChild(gameManager.Unit_Panel.transform.childCount - 1) && (!(unit.transform.position.x - map.width / 2 < UNIT.transform.position.x && unit.transform.position.x + map.width / 2 > UNIT.transform.position.x && unit.transform.position.y - map.height / 2 < UNIT.transform.position.y && unit.transform.position.y + map.height / 2 > UNIT.transform.position.y)))
                    {
                        return true;
                    }

                    if (unit.transform.position.x - map.width / 2 < UNIT.transform.position.x && unit.transform.position.x + map.width / 2 > UNIT.transform.position.x && unit.transform.position.y - map.height / 2 < UNIT.transform.position.y && unit.transform.position.y + map.height / 2 > UNIT.transform.position.y)
                    {
                        int cnt = 0;
                        foreach (string stack in unit.GetComponent<Unit>().doubleStack)
                        {
                            cnt++;
                            if (UNIT.GetComponent<Unit_Drop>().unitType.ToString() == stack && unit_cnt < 2)
                            {
                                return true;
                            }
                            else
                            {
                                if (cnt > unit.GetComponent<Unit>().doubleStack.Count - 1)
                                {
                                    return false;
                                }
                            }
                        }
                    }

                }
            }
            return false;
        }
        public GameObject GetNearestHexFromUnit(GameObject unit)
        {
            Debug.LogError("GetNearestHexfromenemy");
            // Perform a BFS search to find the nearest hex
            GameObject hexParent = unit.GetComponent<Unit>().unitType == Unit.UnitType.Navy ? seaHex : landHex;
            Queue<(GameObject, int)> queue = new Queue<(GameObject, int)>();
            HashSet<GameObject> visited = new HashSet<GameObject>();
            queue.Enqueue((unit, 0));
            visited.Add(unit);
            GameObject nearestHex = null;
            int nearestDistance = int.MaxValue;

            while (queue.Count > 0)
            {
                (GameObject currentHex, int distance) = queue.Dequeue();
                if (distance < nearestDistance && currentHex != unit) // Check if the current hex is not the same as the given unit
                {
                    nearestHex = currentHex;
                    nearestDistance = distance;
                    break;  // Stop the search once we find the nearest hex
                }
                foreach (GameObject neighbor in GetNeighbors(currentHex,hexParent))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue((neighbor, distance + 1));
                        visited.Add(neighbor);
                    }
                }
            }
            nearestHex.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/GridHex/1");
            return nearestHex;
        }
    }
}
