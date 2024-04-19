using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Photon.Pun.Demo.Asteroids;
using ExitGames.Client.Photon;
using Photon.Realtime;
using TMPro;

namespace Photon.Pun.Demo.Asteroids
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
    
        public GameObject Player_Color;
        public GameObject turn_Symbol;
        public Transform Unit_Panel;
        public Transform Other_Panel;
        public Transform Unit_Asset_Panel;
        public Transform Command_Panel;
        public Transform Lost_Panel;
        public Transform Win_Panel;
        public Button next_btn;
        public Button next_confirm_btn;
        public Button confirm_btn;
        public Button exit_btn;
        public Button rematch_btn;
        public Button increaser_btn;
        public Button decreaer_btn;
        public GameObject commandPanel;
        private int seconds;
        private int minutes;
        private float timer;
        public TMP_Text TimeText;
        public TMP_Text AlertText;
        private float rpcInterval = 1f;
        private float rpcTimer = 0f;
        public bool timeup_flag = false;
        public static int move_count;
        public static string PlayerName;
        public static List<string> playerColorlist = new List<string>();
        public static int MaxPlayer;
        public static int timer_count;
        public static string PlayerColor;
        public static bool DragandDropStatus = true;
        public bool current_Unit_isActive = false;
        public TMP_Text UserName;
        public static bool Turn_flag = false;
        public int timelimit_val=3;

        // Start is called before the first frame update
        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (timer_count == 0)
                {
                    timelimit_val = 3;
                }
                else
                {
                    timelimit_val = timer_count;
                }
            }
            UserName.text = PlayerName.ToString();
            Player_Color.GetComponent<Image>().color = GetColorFromString(PlayerColor);
            TurnDetection();
            next_confirm_btn.onClick.AddListener(()=> {
                if (DragandDropStatus)
                {
                    DragandDropStatus = false;
                }
                Turn_Completed();
            });
            exit_btn.GetComponent<Button>().onClick.AddListener(() => {
                Application.Quit();
            });
        }
        
        public void ReMatch()
        {
            PhotonNetwork.LoadLevel("Map");
        }
        public void LeaveGame()
        {
            // Leave the current Photon room
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            // Load the room scene
            SceneManager.LoadScene("Room");
        }
       
        public void Win()
        {
            Win_Panel.gameObject.SetActive(true);
        }
        public void Lost()
        {
            Lost_Panel.gameObject.SetActive(true);
        }
        public void Turn_Completed()
        {
            if (DragandDropStatus)
            {
                Unit_Asset_Panel.gameObject.SetActive(false);
            }
            Turn_flag = false;
            next_btn.gameObject.SetActive(false);
            if (timeup_flag)
            {
                string str = "Time is up";
                StartCoroutine(AlertMessage(str, str));
            }
            else
            {
                string str = "I’m done with actions";
                string str1 = "I’m finished with my phase";
                StartCoroutine(AlertMessage(str, str1));
            }
            Inite();
            timer = 0;
            move_count = 0;
            timeup_flag = false;
            turn_Symbol.GetComponent<PhotonView>().RPC("TurnStatus", RpcTarget.All, PlayerColor, false);
        }
        public void Turn_Started()
        {
            if (DragandDropStatus)
            {
                Unit_Asset_Panel.gameObject.SetActive(true);
            }
            string str = "Your turn";
            StartCoroutine(AlertMessage(str, str));
            Turn_flag = true;
            next_btn.gameObject.SetActive(true);
            if (DragandDropStatus)
            {
                Command_Panel.gameObject.SetActive(true);
            }
            else
            {
                Command_Panel.gameObject.SetActive(false);
            }
            UnitsNormalSideUp();
        }
        public void Drop_finished()
        {
            Turn_Completed();
        }
        private void Inite()
        {
            UnitsNormalSideUp();
            GameObject.FindObjectOfType<MapData>().Hex_init(GameObject.FindObjectOfType<MapData>().landHex);
            GameObject.FindObjectOfType<MapData>().Hex_init(GameObject.FindObjectOfType<MapData>().seaHex);
            Command_Panel.gameObject.SetActive(false);
            Unit_Asset_Panel.gameObject.SetActive(false);
        }
        public void Unit_instantiate(string UnitName, Vector3 position, int troopernum)
        {
            GameObject unit = PhotonNetwork.Instantiate(UnitName, position, Quaternion.identity);
            unit.GetComponent<Unit>().TrooperNum = troopernum;
            unit.transform.SetParent(Unit_Panel);
            unit.transform.localScale = Vector3.one;
            unit.GetComponent<Image>().color = GetColorFromString(PlayerColor);
            unit.GetComponent<PhotonView>().RPC("Setparent",RpcTarget.Others, unit.transform.localPosition, PlayerColor,troopernum);
        }
        public void TurnDetection()
        {
            if (GetColorFromString(PlayerColor) == turn_Symbol.GetComponent<Image>().color)
            {
                Turn_Started();
            }
            else
            {
                Turn_flag = false;
            }

        }
        private void UnitsNormalSideUp()
        {
            for(int i=0;i<Unit_Panel.childCount;i++)
            {
                GameObject unit = Unit_Panel.GetChild(i).gameObject;
                if (unit != null)
                {
                    unit.GetComponent<Unit>().isTurnActive = true;
                    unit.GetComponent<Unit>().alreadyattacked = false;
                    switch (unit.GetComponent<Unit>().unitType)
                    {
                        case Unit.UnitType.Capital:
                            string path = "Texture/Units/1-2";
                            unit.GetComponent<PhotonView>().RPC("UnitNormalStatus", RpcTarget.All, path);
                            break;
                        case Unit.UnitType.AirForce:
                            string path1 = "Texture/Units/4-2";
                            unit.GetComponent<PhotonView>().RPC("UnitNormalStatus", RpcTarget.All, path1);
                            break;
                        case Unit.UnitType.Paratrooper:
                            string path2 = "Texture/Units/2-3";
                            unit.GetComponent<PhotonView>().RPC("UnitNormalStatus", RpcTarget.All, path2);
                            break;
                        case Unit.UnitType.Army:
                            string path3 = "Texture/Units/16-2";
                            unit.GetComponent<PhotonView>().RPC("UnitNormalStatus", RpcTarget.All, path3);
                            break;
                        case Unit.UnitType.Navy:
                            string path4 = "Texture/Units/8-2";
                            unit.GetComponent<PhotonView>().RPC("UnitNormalStatus", RpcTarget.All, path4);
                            break;
                    }
                }
            }
        }
        private void UnitsDisturbedSideUp()
        {
            foreach (GameObject unit in Unit_Panel)
            {
                unit.GetComponent<Unit>().isTurnActive = false;
            }
        }
        public Color GetColorFromString(string colorString)
        {
            switch (colorString)
            {
                case "Red":
                    return Color.red;
                case "Yellow":
                    return Color.yellow;
                case "Green":
                    return Color.green;
                case "Purple":
                    return new Color(0.5f, 0.0f, 0.5f);
                default:
                    return Color.white;
            }
        }
        public void isSelectedUnit(GameObject unit)
        {      // Check available movement turn of currently selected unit
            unit.GetComponent<Unit>().isTurnActive = true;
            current_Unit_isActive = true;
        }
        public void Unitmovement_count(int cnt)
        {
            move_count+=cnt;
            if (move_count >= Unit_Panel.childCount-1) //count movement of unit time up without capital
            {
                move_count = 0;
                Turn_Completed();
            }
        }
        private int doubledunitcount()
        {
            List<Transform> doubledChildren = new List<Transform>();

            foreach (Transform child1 in Unit_Panel.transform)
            {
                foreach (Transform child2 in Unit_Panel.transform)
                {
                    if (child1 != child2 && child1.position == child2.position)
                    {
                        if (!doubledChildren.Contains(child1))
                        {
                            doubledChildren.Add(child1);
                        }
                        if (!doubledChildren.Contains(child2))
                        {
                            doubledChildren.Add(child2);
                        }
                    }
                }
            }
            int cnt = 0;
            foreach (Transform doubledChild in doubledChildren)
            {
                cnt++;
            }
            return cnt;
        }
        // Update is called once per frame
        void Update()
        {
            /*if (PhotonNetwork.IsMasterClient && !timeup_flag)
            {
                if (timelimit_val > 0)
                {
                    timer += Time.deltaTime;
                    seconds = 60-(int)(timer % 60);
                    minutes = timelimit_val-(int)(timer / 60)-1;
                    if (minutes < 0)
                    {
                        timeup_flag = true;
                        TimeisUp();
                        timer = 0;
                        minutes = timelimit_val;
                    }

                    rpcTimer += Time.deltaTime; // Increment the timer

                    // Check if the interval has passed
                    if (rpcTimer >= rpcInterval)
                    {
                        TimeText.GetComponent<PhotonView>().RPC("Timer", RpcTarget.All, minutes, seconds);
                        rpcTimer = 0f; // Reset the timer
                    }
                }
            }*/
        }
        void TimeisUp()
        {
            Turn_Completed();
        }
        IEnumerator AlertMessage(string content, string content1)
        {
            AlertText.text = content.ToString();
            yield return new WaitForSeconds(2f);
            AlertText.text = content1.ToString();
            yield return new WaitForSeconds(2f);
            AlertText.text = "".ToString();
        }
    }
}
