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
    public class Unit : MonoBehaviourPunCallbacks
    {
        public enum UnitType
        {
            Capital,
            AirForce,
            Paratrooper,
            Army,
            Navy
        };
        public UnitType unitType;
        string unitPath;

        private GameManager gameManager;
        private MapData map; // Reference to the Map script
        private Action_Manager action_manager;
        private PathRetrive pathfinder;
        public GameObject spotted_enemy;
        public bool enemy_attack = false;
        public GameObject cell_prefab;
        private GameObject parentUnit;
        private GameObject doubledUnit;
        public int healthPoints;
        public int TrooperNum;
        private int playerIndex;
        private int movementRange;
        private int attackRange;
        public string action_Mode;
        public List<string> doubleStack;
        public List<string> attackableStack;
        public Transform cell_parent;
        public Vector3 target_pos;
        public TMP_Text TroopernumText;
        public bool doubleMode = false;
        public bool isTurnActive = false;
        public bool actionMove = true;
        private bool isAlone = false;
        public bool alreadyattacked = false;
        GameObject startHex;
        Vector3 startPos;




        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        // Start is called before the first frame update
        void Start()
        {
            if (unitType != UnitType.Capital)
            {
                TroopernumText.text = TrooperNum.ToString();
            }
            initializeUnit(unitType);
            map = FindObjectOfType<MapData>();
            gameManager = FindObjectOfType<GameManager>();
            action_manager = FindObjectOfType<Action_Manager>();
            pathfinder = FindObjectOfType<PathRetrive>();
            parentUnit = gameManager.Unit_Panel.gameObject;
            CurrentlyDoubleStackCheck();
        }
        [PunRPC]
        public void Setparent(Vector3 pos, string playercolor, int troopernum)
        {
            transform.SetParent(GameObject.FindObjectOfType<GameManager>().Other_Panel);
            transform.localPosition = pos;
            transform.localScale = Vector3.one;
            if (unitType != UnitType.Capital)
            {
                TroopernumText.text = troopernum.ToString();  //Currently unit trooper name
            }
            transform.GetComponent<Image>().color = GameObject.FindObjectOfType<GameManager>().GetColorFromString(playercolor);
            gameObject.SetActive(true);
        }
        //Operation of land unit action
        public void UnitAction(string action_mode, GameObject SendHex, GameObject ReceiveHex)
        {
            print(action_mode + " and unit is " + SendHex.name);
            switch (action_mode)
            {
                case "airTransfer":
                    AirTransfer(SendHex, ReceiveHex);
                    break;
            }
        }
        //Init unit properties 
        public void initializeUnit(UnitType unitType)
        {
            doubleStack = new List<string>();
            switch (unitType)
            {
                case UnitType.Capital:
                    movementRange = 0;
                    attackRange = 0;
                    healthPoints = 5;
                    isAlone = true;
                    doubleStack.AddRange(new List<string> { "AirForce", "Paratrooper", "Army" });  // Add elements to the doubleStack list
                    attackableStack.AddRange(new List<string> { "Navy", "Paratrooper" });
                    break;
                case UnitType.AirForce:
                    movementRange = 3;
                    attackRange = 4;
                    healthPoints = 4;
                    isAlone = false;
                    doubleStack.AddRange(new List<string> { "Capital", "Army", "Paratrooper" });  // Add elements to the doubleStack list
                    break;
                case UnitType.Paratrooper:
                    movementRange = 1;
                    attackRange = 1;
                    healthPoints = 3;
                    isAlone = true;
                    doubleStack.AddRange(new List<string> { "Capital", "Army", "Navy" });  // Add elements to the doubleStack list
                    break;
                case UnitType.Army:
                    movementRange = 1;
                    attackRange = 1;
                    healthPoints = 2;
                    isAlone = true;
                    doubleStack.AddRange(new List<string> { "Capital", "Army", "AirForce", "Paratrooper", "Navy" });  // Add elements to the doubleStack list
                    break;
                case UnitType.Navy:
                    movementRange = 2;
                    attackRange = 2;
                    healthPoints = 1;
                    isAlone = true;
                    doubleStack.AddRange(new List<string> { "Army", "Paratrooper", "Navy" });  // Add elements to the doubleStack list
                    break;
            }
        }
        //All operation of unit once unit is clicked
        public void UnitClick()
        {
            if (photonView.IsMine && GameManager.Turn_flag && !GameManager.DragandDropStatus && isTurnActive)
            {
                if (!gameManager.current_Unit_isActive)  //current unit move turn check
                {
                    gameManager.isSelectedUnit(gameObject);
                }
                if (isTurnActive)   //Check whether currently unit's turn or not 
                {
                    GameObject.FindAnyObjectByType<GameManager>().commandPanel.SetActive(true);
                    GameObject hexParent = gameObject.GetComponent<Unit>().unitType == Unit.UnitType.Navy ? map.seaHex : map.landHex;
                    startHex = map.GetHexAtUnit(gameObject, hexParent);
                    CurrentlyDoubleStackCheck();
                    GameObject doubledstack = doubleddotherstack();
                    if (doubledstack != null && (doubledstack.GetComponent<Unit>().unitType == Unit.UnitType.Navy) && doubleMode) { } //If other one of double stack is Navy or this unit is Airforce, they can't move  theirself.
                    else
                    {
                        if (unitType == UnitType.AirForce || (doubledstack!=null && doubledstack.GetComponent<Unit>().unitType == Unit.UnitType.Navy))
                        {

                        }
                        else
                        {
                            GetHexsInRangeAroundUnit();
                        }
                        if (gameObject.GetComponent<Unit>().unitType != Unit.UnitType.Navy)
                        {

                            action_manager.selectedLandUnit(gameObject, doubleMode);
                        }
                        else
                        {
                            if (map.HasOppositeHexNextToUnit(gameObject))
                            {
                                action_manager.selectedSeaUnit(gameObject, doubleMode);
                            }
                            else { }
                        }
                    }
                }

            }
        }
        //Find path of unit movement
        public void GetHexsInRangeAroundUnit()
        {
            
            transform.SetAsLastSibling();
            List<GameObject> hexInRange = new List<GameObject>();
            // Get the hexes within the specified range from the selected unit
            List<GameObject> hexesInRange = map.GetHexesInRange(gameObject, movementRange);
            // Do something with the hexes in range
            foreach (GameObject hex in hexesInRange)
            {
                string Objtag = hex.tag;
                if (Objtag != "Unit") //check either currently gameobject is unit or hex
                {
                    hex.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/GridHex/Hex1");
                    GameObject cell = Instantiate(cell_prefab, hex.transform.position, Quaternion.identity);
                    cell.transform.SetParent(cell_parent);
                    cell.transform.localScale = Vector3.one;
                    cell.transform.Rotate(0f, 0f, 12.188f);
                    cell.GetComponent<cell>().action_mode = "";
                    if (!actionMove)  //If action button is pressed
                    {
                        cell.GetComponent<cell>().action_move = false;
                        cell.GetComponent<cell>().action_mode = action_Mode;
                    }
                    else {  //If unit only moves
                    }
                }
            }
            if (gameObject.GetComponent<Unit>().unitType != Unit.UnitType.Navy)
            {
                if (map.HasOppositeHexNextToUnit(gameObject) && action_manager.embarkEnable) // Check whether currently single unit is on the coast or not, once pressed seareceive button
                {
                    action_manager.SeaReceive(gameObject);
                }

            }
        }
        public void GetHexsInRangeAroundUnitTobomb() //Display attackable enemy unit within attack range
        {
            transform.SetAsLastSibling();
            List<GameObject> hexInRange = new List<GameObject>();
            // Get the hexes within the specified range from the selected unit
            List<GameObject> hexesInRange = map.GetHexesInRange(gameObject, movementRange);
            // Do something with the hexes in range
            foreach (GameObject hex in hexesInRange)
            {
                string Objtag = hex.tag;
                foreach (Transform enemychild in GameObject.FindObjectOfType<GameManager>().Other_Panel.transform)
                {
                    bool PosEqual = false;
                    float width = enemychild.GetComponent<RectTransform>().rect.width;
                    float hexPosX = hex.transform.position.x;
                    float hexPosY = hex.transform.position.y;
                    float enemyPosX = enemychild.transform.position.x;
                    float enemyPosY = enemychild.transform.position.y;
                    //for(int i=0;i< attackableStack.Count; i++)
                    //{
                       // if (enemychild.name != attackableStack[i] + "(Clone)") //check enemy is attackable unit
                       // {

                            if (hexPosX > enemyPosX - width / 2 && hexPosX < enemyPosX + width / 2 && hexPosY > enemyPosY - width / 2 && hexPosY < enemyPosY + width / 2)
                            {
                                PosEqual = true;

                                if (Objtag != "Unit" && PosEqual) //check either currently gameobject is unit or hex
                                {
                                    hex.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/GridHex/Hex1");
                                    GameObject cell = Instantiate(cell_prefab, hex.transform.position, Quaternion.identity);
                                    cell.transform.SetParent(cell_parent);
                                    cell.transform.localScale = Vector3.one;
                                    cell.GetComponent<cell>().action_mode = "";
                                    cell.transform.Rotate(0f, 0f, 12.188f);
                                    if (!actionMove)
                                    {
                                        cell.GetComponent<cell>().action_move = false;
                                        cell.GetComponent<cell>().action_mode = action_Mode;
                                    }
                                    else { }
                                }
                            }
                       // }
                    //}
                }
            }
            if (gameObject.GetComponent<Unit>().unitType != Unit.UnitType.Navy)
            {
                if (map.HasOppositeHexNextToUnit(gameObject) && action_manager.embarkEnable) // Check whether currently single unit is on the coast or not once pressed seareceive button
                {
                    action_manager.SeaReceive(gameObject);
                }

            }
        }
        //Check whether currently unit is doublestack or not
        public void CurrentlyDoubleStackCheck()
        {
            //int unit_cnt = 0;
            foreach (Transform child in parentUnit.transform)
            {
                //if (child.GetComponent<Unit>().unitType != UnitType.Capital && (child.transform.position.x - map.width / 3 < transform.position.x && child.transform.position.x + map.width / 3 > transform.position.x && child.transform.position.y - map.height / 3 < transform.position.y && child.transform.position.y + map.height / 3 > transform.position.y) && child != this.transform)
                if ((child.transform.position.x - map.width / 3 < transform.position.x && child.transform.position.x + map.width / 3 > transform.position.x && child.transform.position.y - map.height / 3 < transform.position.y && child.transform.position.y + map.height / 3 > transform.position.y) && child != this.transform)
                {
                    doubledUnit = child.gameObject;
                    if (transform.localScale==Vector3.one && doubledUnit.transform.localScale==Vector3.one)
                    {
                        GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
                        GameObject hex = map.GetHexAtUnit(gameObject,hexparent);
                        float width = hex.GetComponent<RectTransform>().rect.width;
                        Vector3 delta_pos = new Vector3(width / 10, width / 10, 1);
                        doubledUnit.transform.position = transform.position + delta_pos * GameObject.FindObjectOfType<MapData>().Controller.value; ;
                        transform.position = transform.position - delta_pos * GameObject.FindObjectOfType<MapData>().Controller.value; ;
                        transform.localScale = Vector3.one * 0.7f;
                        doubledUnit.transform.localScale = Vector3.one * 0.7f;
                    }
                    doubleMode = true;
                }
            }
        }
        public GameObject disembarkUnitCheck()  //Get currently embarked unit on the navy to disembark
        {
            CurrentlyDoubleStackCheck();
            return doubledUnit;
        }
        //Check wether targetUnit is doublestack or not
        public bool TargetDoubleStackCheck(GameObject pos)
        {
            int unit_cnt = 0;
            int stack_cnt = 0;
            foreach (Transform child in parentUnit.transform) //Check currently unit double status
            {
                if (pos.transform.position.x - map.width / 2 < child.transform.position.x && pos.transform.position.x + map.width / 2 > child.transform.position.x && pos.transform.position.y - map.height / 2 < child.transform.position.y && pos.transform.position.y + map.height / 2 > child.position.y && child != transform)
                {
                    unit_cnt++;
                }
            }
            foreach (Transform child in parentUnit.transform)
            {
                float width_pos = pos.GetComponent<RectTransform>().rect.width;
                if (pos.transform.position.x - width_pos / 2 < child.transform.position.x && pos.transform.position.x + width_pos / 2 > child.transform.position.x && pos.transform.position.y - width_pos / 2 < child.transform.position.y && pos.transform.position.y + width_pos / 2 > child.position.y && child != transform)
                {
                    Debug.LogError("there is already other unit on that hex"+child.name);
                    foreach (string Unit_Name in doubleStack)
                    {
                        stack_cnt++;
                        if (!doubleMode && Unit_Name == child.GetComponent<Unit>().unitType.ToString() && unit_cnt < 3 && isAlone)
                        {
                            return true;  //Once single unit is available to move to other single unit
                        }
                        else
                        {
                            if (stack_cnt == doubleStack.Count)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        public void MoveTo(GameObject hex,GameObject spotedEnemy)      //Unit movement once clicked path of unit
        {
            if (TargetDoubleStackCheck(hex))
            {
                Debug.LogError("move started");
                if (doubleMode && (doubledUnit.GetComponent<Unit>().unitType==Unit.UnitType.AirForce || unitType == UnitType.Navy)) //Display doublestack using offset sight
                {
                    Debug.LogError("double stack is airforce or navy these double stack move together");
                    GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
                    GameObject hexforwidth = map.GetHexAtUnit(gameObject, hexparent);
                    float width = hexforwidth.GetComponent<RectTransform>().rect.width;
                    Vector3 delta_pos = new Vector3(width / 10, width / 10, 1);
                    if (spotedEnemy != null)
                    {
                        GameObject enemynearesthex = GameObject.FindObjectOfType<MapData>().GetNearestHexFromUnit(spotedEnemy);
                        doubledUnit.transform.position = enemynearesthex.transform.position + delta_pos * GameObject.FindObjectOfType<MapData>().Controller.value;
                        transform.position = enemynearesthex.transform.position - delta_pos * GameObject.FindObjectOfType<MapData>().Controller.value;
                        Attack(spotedEnemy);
                    }
                    else
                    {
                        doubledUnit.transform.position = hex.transform.position + delta_pos * GameObject.FindObjectOfType<MapData>().Controller.value;
                        transform.position = hex.transform.position - delta_pos * GameObject.FindObjectOfType<MapData>().Controller.value;
                    }
                    doubledUnit.transform.localScale = Vector3.one*0.7f;
                    transform.localScale = Vector3.one*0.7f;
                    doubledUnit.GetComponent<Unit>().action_init(); 
                }
                else
                {
                    Debug.LogError("single or double esc");
                    if (spotedEnemy != null)
                    {
                        GameObject enemynearesthex = GameObject.FindObjectOfType<MapData>().GetNearestHexFromUnit(spotedEnemy);
                        transform.position = enemynearesthex.transform.position;
                        if (doubleMode)
                        {
                            Debug.LogError("unit moves anywhere and doubled another stack has original scale and pos");
                            transform.localScale = Vector3.one;

                            GameObject hexparent = doubledUnit.GetComponent<Unit>().unitType == UnitType.Navy ? map.seaHex : map.landHex; //If there was another double stack , double stack scale and position will be original
                            GameObject originalcorrectPos = GameObject.FindObjectOfType<MapData>().GetHexAtUnit(doubledUnit, hexparent);
                            doubledUnit.transform.position = originalcorrectPos.transform.position;
                            doubledUnit.transform.localScale = Vector3.one;

                        }
                        else
                        {
                            Debug.LogError("single unit move to other one unit as double stack");
                            CurrentlyDoubleStackCheck();
                        }
                        Attack(spotedEnemy);
                    }
                    else
                    {
                        transform.position = hex.transform.position;
                        if (doubleMode)
                        {
                            Debug.LogError("unit moves anywhere and doubled another stack has original scale and pos");
                            transform.localScale = Vector3.one;

                            GameObject hexparent = doubledUnit.GetComponent<Unit>().unitType == UnitType.Navy ? map.seaHex : map.landHex; //If there was another double stack , double stack scale and position will be original
                            GameObject originalcorrectPos = GameObject.FindObjectOfType<MapData>().GetHexAtUnit(doubledUnit, hexparent);
                            doubledUnit.transform.position = originalcorrectPos.transform.position;
                            doubledUnit.transform.localScale = Vector3.one;

                        }
                        else
                        {
                            Debug.LogError("single unit move to other one unit as double stack");
                            CurrentlyDoubleStackCheck();
                        }
                    }
                    
                    //GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
                    //GameObject endHex = map.GetHexAtUnit(hex, hexparent);
                    //UnitMovmentaccordingtoPath(startHex,endHex,gameObject);
                }
                path_init();
                action_manager.landTransfer_Panel.SetActive(false);
                action_manager.seaTransfer_Panel.SetActive(false);
                action_init();
                unit_countaction();
            }
        }
        public void UnitMovmentaccordingtoPath(GameObject startPos, GameObject targetPos, GameObject unit)
        {
            // Get the minimum path using the Pathfinding class
            GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
            List<GameObject> path = PathRetrive.FindPath(startPos, targetPos, hexparent);
            // Move the unit along the obtained path
            StartCoroutine(MoveAlongPath(path, unit));
        }

        IEnumerator MoveAlongPath(List<GameObject> path, GameObject unit)
        {
            print(path.Count);
            foreach (GameObject hex in path)
            {
                print(hex.name);
                // Move the unit to the next position in the path
                unit.transform.position = hex.transform.position;

                // Wait for some time before moving to the next position
                yield return new WaitForSeconds(0.5f);
            }
        }
        public void Action_MoveTo(GameObject hex, string action_mode)  //Move unit with action on the land
        {
            action_Mode = "";
            actionMove = true;
            startPos = transform.position;
            GameObject doubledstack = doubleddotherstack();
            doubledstack.GetComponent<Unit>().action_init();
            switch (action_mode)
            {
                case "airBombard":
                    StartCoroutine(AirBombard(gameObject, hex, startPos));
                    break;
                case "airDrop":
                    AirborneDrop(gameObject, hex);
                    break;
                case "airAssault":
                    AirborneAssault(gameObject, hex);
                    break;
            }
            path_init();
            action_manager.landTransfer_Panel.SetActive(false);
            action_manager.seaTransfer_Panel.SetActive(false);
        }
        private GameObject doubleddotherstack()
        {
            foreach (Transform child in parentUnit.transform)
            {
                if ((child.transform.position.x - map.width / 3 < transform.position.x && child.transform.position.x + map.width / 3 > transform.position.x && child.transform.position.y - map.height / 3 < transform.position.y && child.transform.position.y + map.height / 3 > transform.position.y) && child != this.transform)
                {
                    return child.gameObject;
                }
            }
            return null;
        }
        public void path_init()   //init movement path of unit
        {
            Transform cellParent = transform.Find("cell_parent");
            int childCount = cellParent.childCount;
            GameObject hexparent = unitType == UnitType.Navy ? map.seaHex : map.landHex;
            map.Hex_init(hexparent);
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(cellParent.GetChild(i).gameObject);
            }
        }
        private void enemyretrive()
        {
            for (int i = 0; i < GameObject.FindObjectOfType<GameManager>().Other_Panel.childCount; i++)
            {
                GameObject child = GameObject.FindObjectOfType<GameManager>().Other_Panel.GetChild(i).gameObject;
                Vector3 pos = transform.position;
                if (child.transform.position.x - map.width / 3 < pos.x && child.transform.position.x + map.width / 3 > pos.x && child.transform.position.y - map.height / 3 < pos.y && child.transform.position.y + map.height / 3 > transform.position.y)
                {
                    Attack(child);
                }
            }
        }
        public void Attack(GameObject targetUnit) //Attack enemy
        {
            print("attack" + targetUnit.name);
            //  if (isTurnActive)
            //  {
            // Code to handle attacking the target unit within attack range
            if (targetUnit.GetComponent<Image>().color != gameObject.GetComponent<Image>().color)
            {
                print("Enemy!!!!");
                if (targetUnit.GetComponent<Unit>().alreadyattacked)
                {
                    Debug.LogError("alreadyattacked by me");
                    action_init();
                }
                else if(!targetUnit.GetComponent<Unit>().alreadyattacked) //If targetunit is attacked first time, attacker return back to start position after attacked
                {
                    Debug.LogError("not alreadyattacked by me yet");
                    //gameObject.transform.position = startHex.transform.position;
                    targetUnit.GetComponent<Unit>().action_init();
                    action_init();
                }
                //for (int i = 0; i < attackableStack.Count; i++)
                //{
                    //if (attackableStack[i] + "(Clone)" == targetUnit.name)
                    //{
                        targetUnit.GetComponent<Unit>().TakeDamage(healthPoints, gameObject);
                    //}
                //}
                //targetUnit.GetComponent<PhotonView>().RPC("TakeDamage",RpcTarget.All, healthPoints, gameObject);
            }
            //  }
        }
        //      [PunRPC]
        public void TakeDamage(int damage, GameObject attacker)  //Is attacked by enemy
        {
            healthPoints -= damage;
            if (healthPoints <= damage)
            {
                GameObject targetGameObject = attacker.gameObject;
                PhotonView targetPhotonView = targetGameObject.GetComponent<PhotonView>();
                if (targetPhotonView != null)
                {
                    gameObject.GetComponent<PhotonView>().RPC("Die", RpcTarget.All, targetPhotonView.ViewID);
                }
            }
        }
        [PunRPC]
        public void Die(int attackerID)  //Lost by enemy
        {
            print("Die!");
            if (photonView.IsMine)
            {
                GameObject attacker = PhotonView.Find(attackerID)?.gameObject;
                if (alreadyattacked == false)
                {
                    Debug.LogError("I am attacked by enemy now");
                    alreadyattacked = true;
                    //action_init();
                }
                else if (alreadyattacked == true)
                {
                    Debug.LogError("I have already been attacked by enemy before");
                    if (unitType == UnitType.Capital)
                    {
                        //attacker.GetComponent<Unit>().kill(gameObject);
                        GameObject targetGameObject = this.gameObject;
                        PhotonView targetPhotonView = targetGameObject.GetComponent<PhotonView>();
                        if (targetPhotonView != null)
                        {
                            attacker.GetComponent<PhotonView>().RPC("kill", RpcTarget.All, targetPhotonView.ViewID);
                        }
                        GameObject.FindObjectOfType<GameManager>().Lost();
                        PhotonNetwork.Destroy(gameObject);
                    }
                    else
                    {
                        PhotonNetwork.Destroy(gameObject);
                    }
                    alreadyattacked = false;
                }
            }
        }
        [PunRPC]
        public void kill(int attackedEnemybymeID) //kill enemy
        {
            print("Kill!");
            if (photonView.IsMine)
            {
                GameObject attackedEnemybyme = PhotonView.Find(attackedEnemybymeID)?.gameObject;
                if (attackedEnemybyme.GetComponent<Unit>().unitType == UnitType.Capital)
                {
                    GameObject.FindObjectOfType<GameManager>().Win();
                }
            }
        }
        IEnumerator AirBombard(GameObject targetUnit, GameObject targetHex,Vector3 startpos) //Action so that airforce come back to starthex after bomb
        {
            print("Bombard" + targetHex.name);
            targetUnit.transform.position = targetHex.transform.position;
            enemyretrive();
            yield return new WaitForSeconds(1);
            targetUnit.transform.position = startpos;
            unit_action_finished();
        }

        private void AirTransfer(GameObject targetUnit, GameObject receiveHex) //Action of avaliable all unit movement via sky from sendhex to receivehex 
        {
            print("Transfer" + targetUnit.name + " to " + receiveHex.name);
            targetUnit.transform.SetAsLastSibling();
            targetUnit.transform.position = receiveHex.transform.position;
            unit_action_finished();
        }

        public void AirborneAssault(GameObject targetUnit, GameObject targetHex) //Action of paratrooper attack enemy via sky from sendhex to targethex/target non-friendly unit
        {
            print("Assault");
            targetUnit.transform.position = targetHex.transform.position;
            unit_action_finished();
        }

        public void AirborneDrop(GameObject targetUnit, GameObject targetHex) //Action of paratrooper attack enemy via sky from sendhex to targethex/target solor friendly unit
        {
            print("Drop" + targetUnit + targetHex);
            targetUnit.transform.position = targetHex.transform.position;
            unit_action_finished();
        }
        public void SeaReceive(GameObject receiveUnit) //Embark action of available land unit which is placed on the coast to navy on the coast
        {
            path_init();
            transform.SetAsFirstSibling();
            gameObject.transform.position = receiveUnit.transform.position;
            CurrentlyDoubleStackCheck();
            GameObject doubledstack = doubleddotherstack();
            doubledstack.GetComponent<Unit>().action_init();
            action_manager.embarkEnable = false;
            unit_action_finished();
        }
        [PunRPC]
        public void UnitDisturbedStatus(string path)
        {
            if (unitType != UnitType.Capital)
            {
                GameObject Unitnum = transform.Find("Trooper num").gameObject;
                Unitnum.GetComponent<TMP_Text>().color = Color.black;
            }
            GameObject UnitDisturbedDown = transform.Find("Type").gameObject;
            UnitDisturbedDown.GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
        }
        [PunRPC]
        public void UnitNormalStatus(string path)
        {
            if (unitType != UnitType.Capital)
            {
                GameObject Unitnum = transform.Find("Trooper num").gameObject;
                Unitnum.GetComponent<TMP_Text>().color = Color.white;
            }
            GameObject UnitDisturbedDown = transform.Find("Type").gameObject;
            UnitDisturbedDown.GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
        }
        public void SeaSend() //Disembark action of available land unit on the navy which is on the coast to landhex where is placed on the coast
        {
            if (action_manager.disembarkEnable)
            {
                GameObject seasendhexunit = doubleddotherstack();
                seasendhexunit.transform.localScale = Vector3.one;
                GameObject hexParent = gameObject.GetComponent<Unit>().unitType == Unit.UnitType.Navy ? map.seaHex : map.landHex;
                GameObject hex = map.GetHexAtUnit(gameObject, hexParent);
                seasendhexunit.transform.position = hex.transform.position;
                path_init();
                GetHexsInRangeAroundUnit();
                transform.SetAsLastSibling();
                doubledUnit = null;
                doubleMode = false;
                action_manager.disembarkEnable = false;
                unit_action_finished();
            }
        }
        private void unit_action_finished()  //Init all cell and variable of unit after finished action
        {
            enemyretrive();
            action_init();
            unit_countaction();
        }
        private void unit_countaction()
        {
            if (doubleMode)
            {
                doubleMode = false;
                GameObject.FindObjectOfType<GameManager>().Unitmovement_count(2);
            }
            else
            {
                GameObject.FindObjectOfType<GameManager>().Unitmovement_count(1);
            }
        }
        private void action_init()
        {
            isTurnActive = false;
            switch (unitType)
            {
                case UnitType.Capital:
                    unitPath = "Texture/Units/1-1";
                    break;
                case UnitType.AirForce:
                    unitPath = "Texture/Units/4-1";
                    break;
                case UnitType.Paratrooper:
                    unitPath = "Texture/Units/2-1";
                    break;
                case UnitType.Army:
                    unitPath = "Texture/Units/16-1";
                    break;
                case UnitType.Navy:
                    unitPath = "Texture/Units/8-1";
                    break;
            }
            GameObject.FindAnyObjectByType<GameManager>().commandPanel.SetActive(false);
            gameObject.GetComponent<PhotonView>().RPC("UnitDisturbedStatus", RpcTarget.All, unitPath);
            GameObject.FindObjectOfType<GameManager>().current_Unit_isActive = false;
        }
    }
}
