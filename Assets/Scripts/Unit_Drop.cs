using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun.Demo.Asteroids;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.Demo.Asteroids
{
    public class Unit_Drop : MonoBehaviourPunCallbacks
    {
        public enum UnitType
        {
            Capital,
            AirForce,
            Navy,
            Army,
            Paratrooper
        };
        public TMP_Text[] unit_text = new TMP_Text[5];
        public TMP_Text[] unit_number = new TMP_Text[5];
        public TMP_Text UnitCount;
        public UnitType unitType;
        public Transform cell;
        public Slider Controller;
        public GameObject unit_asset_panel;
        private MapData map; // Reference to the Map script
        private GameManager gameManager; // Reference to the GameManager script
        private bool isDragging = false;
        private bool isPlacedInBord = false;
        public bool isDouble = false;
        private Vector3 startPosition;
        private Vector3 startScale;
        GameObject placed_unit;
        // Start is called before the first frame update
        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            gameObject.GetComponent<Image>().color = gameManager.GetColorFromString(GameManager.PlayerColor);
            map = FindObjectOfType<MapData>();
        }

        public void OnMouseDown()
        {
            isDragging = true;
            startPosition = transform.position;
            startScale = transform.localScale;
        }
        void Update()
        {
            if (isDragging)
            {
                transform.localScale = cell.localScale * Controller.value ;
                // Update the position of the unit based on the mouse drag
                Vector3 mousePosition = Input.mousePosition;
                transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
                if (unitType == UnitType.Capital)
                {
                    List<GameObject> hexesInRange = new List<GameObject>();
                    hexesInRange = map.UpdateHexesInRange(gameObject, 4);   //While capital is dragged , get all hexes within 4 hex around capital
                    NationBordRetrive(hexesInRange, "Hex1");
                    GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
                    placed_unit = map.GetHexAtPosition(hexparent, transform.position, gameObject);
                    if (placed_unit == null)
                    {
                        isPlacedInBord = false;
                    }
                    else
                    {
                        isPlacedInBord = true;
                    }
                }
                else
                {
                    List<GameObject> HexInBord = new List<GameObject>();
                    GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
                    placed_unit = map.GetHexAtPosition(hexparent, mousePosition, gameObject);
                    map.Hex_init(hexparent);
                    if (placed_unit == null)
                    {
                        isPlacedInBord = false;
                    }
                    else
                    {
                        //if (placed_unit.transform.position.x - map.width / 2 < mousePosition.x && placed_unit.transform.position.x + map.width / 2 > mousePosition.x && placed_unit.transform.position.y - map.height / 2 < mousePosition.y && placed_unit.transform.position.y + map.height / 2 > mousePosition.y)
                        //{

                            placed_unit.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/GridHex/Hex1");
                            isPlacedInBord = true;

                        //}
                    }
                }
            }
        }

        public void OnMouseUp()
        {
            if (isDragging)
            {
                isDragging = false;

                // Check if the unit is dropped on a valid position on the map
                if (IsValidDropPosition())
                {
                    // Instantiate a new unit at the drop position
                    PlaceUnit(placed_unit.transform.position);
                }
                else
                {
                    //GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
                    map.Hex_init(GameObject.FindObjectOfType<MapData>().landHex);
                    map.Hex_init(GameObject.FindObjectOfType<MapData>().seaHex);
                    // Return the unit to its original position
                    transform.position = startPosition;
                    transform.localScale = startScale;
                }
            }
        }

        private bool IsValidDropPosition()
        {
            if (isPlacedInBord && map.DoubleStackCheck(gameObject))  //Check whether dragging currently unit is avaliable to drop into target hex or not
            {
                return true;
            }
            return false;
        }

        private void PlaceUnit(Vector3 position)  //Drop unit and calculation of droped unit among rest units on the asset panel
        {
            bool Drop_finished = false;
            int cnt = int.Parse(UnitCount.text);
            int num = (int)unitType;
            int text_val = int.Parse(unit_number[num].text);
            gameManager.Unit_instantiate(unitType.ToString(), position, text_val);
            Destroy(gameObject);
            text_val--;
            cnt--;
            UnitCount.text = cnt.ToString();
            // Instantiate a new unit prefab at the specified position
            string numtext = text_val.ToString();
            if (text_val <= 0)
            {
                if (cnt <= 0)
                {
                    unit_asset_panel.SetActive(false);
                    GameManager.DragandDropStatus = false;
                    Drop_finished = true;
                }
                unit_text[num].text = "".ToString();
                numtext = "";
            }
            unit_number[num].text = numtext.ToString();
            //GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
            map.Hex_init(GameObject.FindObjectOfType<MapData>().landHex);
            map.Hex_init(GameObject.FindObjectOfType<MapData>().seaHex);
            if (unitType == UnitType.Capital)
            {
                map.GetHexAroundCapital();
            }
            if (Drop_finished)
            {
                GameObject.FindObjectOfType<GameManager>().Drop_finished();
            }
        }
        private void NationBordRetrive(List<GameObject> Range, string cell)  //Express avaliable hexes so that drop units
        {
            foreach (GameObject hex in Range)
            {
                if (hex.name != gameObject.name)
                {
                    hex.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/GridHex/" + cell);
                }
            }
        }
    }
}

