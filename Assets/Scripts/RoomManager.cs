using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Photon.Pun.Demo.Asteroids
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
       

        public TMP_InputField PlayerNameInput;

        [Header("Selection Panel")]
        public GameObject SelectionPanel;
        
        [Header("Waiting Panel")]
        public GameObject WaitingPanel;

        [Header("Create Room Panel")]
        public GameObject CreateRoomPanel;

        public InputField RoomNameInputField;
        public TMP_Dropdown MaxPlayersInputField;

        [Header("Join Random Room Panel")]
        public GameObject JoinRandomRoomPanel;

        [Header("Room List Panel")]
        public GameObject RoomListPanel;

        public GameObject RoomListContent;
        public GameObject RoomListEntryPrefab;

        [Header("Inside Room Panel")]
        public GameObject InsideRoomPanel;

        public GameObject Loading_Panel;
        public GameObject loadingroomlist_panel;

        public static GameObject StartGameButton;
        public Button Ready1Button;
        public Button Ready2Button;
        public Button Ready3Button;
        public Button Ready4Button;
        public GameObject readystate1;
        public GameObject readystate2;
        public GameObject readystate3;
        public GameObject readystate4;
        public GameObject PlayerListEntryPrefab;

        private Dictionary<string, RoomInfo> cachedRoomList;
        private Dictionary<string, GameObject> roomListEntries;
        private Dictionary<int, GameObject> playerListEntries;

        public int MaxplayernNum;
        public int GamePlayerNum;
        public int PlayerColor;
        public int playercnt=1;
        public string CountName="";
        public string RoomName="";
        public int timer;
        public TMP_Text RoomNameText;
        public TMP_Text user1Text;
        public TMP_Text user2Text;
        public TMP_Text user3Text;
        public TMP_Text user4Text;
        public TMP_Dropdown user1selectcolor;
        public TMP_Dropdown user2selectcolor;
        public TMP_Dropdown user3selectcolor;
        public TMP_Dropdown user4selectcolor;

        public bool connected_flag = false;
        public bool gameplay_flag = false;
        public bool isplayerReady = false;
        public static int readyflag = 0;
        public int [] SelectColor=new int[4]; 
        public TMP_Dropdown [] ColorSelecter=new TMP_Dropdown[4];
        #region UNITY

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            cachedRoomList = new Dictionary<string, RoomInfo>();
            roomListEntries = new Dictionary<string, GameObject>();
            PlayerNameInput.text = "Player " + Random.Range(1000, 10000);
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected!");
            connected_flag = true;
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ClearRoomListView();
            UpdateCachedRoomList(roomList);
            UpdateRoomListView();
            loadingroomlist_panel.SetActive(false);
        }
       
        public override void OnJoinedLobby()
        {
            cachedRoomList.Clear();
            ClearRoomListView();
        }

        // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
        public override void OnLeftLobby()
        {
            cachedRoomList.Clear();
            ClearRoomListView();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            string roomName = "Room " + Random.Range(1000, 10000);
            RoomOptions options = new RoomOptions { MaxPlayers = 4 };
            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        public override void OnJoinedRoom()
        {
            int cnt = 0;
            cachedRoomList.Clear();
            Hashtable maxnum = new Hashtable {
                 {"Maxplayernum",  PhotonNetwork.CurrentRoom.MaxPlayers}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(maxnum);

            Hashtable playername = new Hashtable {
                {"Playername",  AuthenManager.current_username}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playername); 
            foreach(Player p in PhotonNetwork.PlayerList)
            {
                cnt++;
                if (p.NickName == AuthenManager.current_username)
                {
                    if (cnt == 1) { 
                        user1selectcolor.GetComponent<TMP_Dropdown>().interactable = true;
                        Ready1Button.GetComponent<Button>().interactable = true;
                        Ready2Button.GetComponent<Button>().interactable = false;
                        Ready3Button.GetComponent<Button>().interactable = false;
                        Ready4Button.GetComponent<Button>().interactable = false;
                    }
                    if (cnt == 2)
                    {
                        user2selectcolor.GetComponent<TMP_Dropdown>().interactable = true;
                        Ready1Button.GetComponent<Button>().interactable = false;
                        Ready2Button.GetComponent<Button>().interactable = true;
                        Ready3Button.GetComponent<Button>().interactable = false;
                        Ready4Button.GetComponent<Button>().interactable = false;
                    }
                    if (cnt == 3)
                    {
                        user3selectcolor.GetComponent<TMP_Dropdown>().interactable = true;
                        Ready1Button.GetComponent<Button>().interactable = false;
                        Ready2Button.GetComponent<Button>().interactable = false;
                        Ready3Button.GetComponent<Button>().interactable = true;
                        Ready4Button.GetComponent<Button>().interactable = false;
                    }
                    if (cnt == 4)
                    {
                        user4selectcolor.GetComponent<TMP_Dropdown>().interactable = true;
                        Ready1Button.GetComponent<Button>().interactable = false;
                        Ready2Button.GetComponent<Button>().interactable = false;
                        Ready3Button.GetComponent<Button>().interactable = false;
                        Ready4Button.GetComponent<Button>().interactable = true;
                    }
                    PlayerColor = cnt;
                }
            }
        }

        public override void OnLeftRoom()
        {
            foreach (GameObject entry in playerListEntries.Values)
            {
                Destroy(entry.gameObject);
            }
            playerListEntries.Clear();
            playerListEntries = null;
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            PlayerListEntry.joinedplayer_flag = true;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
            playerListEntries.Remove(otherPlayer.ActorNumber);

        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {

            object everycolor;
            if (changedProps.TryGetValue("Everycolor", out everycolor))
            {
                int selectedIndex = int.Parse(everycolor.ToString()); // Get the index of the selected option
                List<int> selectedIndexes = new List<int>(); // Create a list to track selected indexes
                for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    if (PhotonNetwork.PlayerList[i] != targetPlayer && ColorSelecter[i].value==0) // Exclude the current player's index
                    {
                        ColorSelecter[i].options.RemoveAt(selectedIndex);// Remove the selected option from the list
                        ColorSelecter[i].RefreshShownValue(); // Refresh the displayed options in the dropdown
                    }
                }
            }
            object color1;
            if (changedProps.TryGetValue("Color1", out color1))
            {
                user1selectcolor.GetComponent<TMP_Dropdown>().value = int.Parse(color1.ToString());
                SelectColor[0]= int.Parse(color1.ToString());
            }
            object color2;
            if (changedProps.TryGetValue("Color2", out color2))
            {
                user2selectcolor.GetComponent<TMP_Dropdown>().value = int.Parse(color2.ToString());
                SelectColor[1] = int.Parse(color2.ToString());
            }
            object color3;
            if (changedProps.TryGetValue("Color3", out color3))
            {
                user3selectcolor.GetComponent<TMP_Dropdown>().value = int.Parse(color3.ToString());
                SelectColor[2] = int.Parse(color3.ToString());
            }
            object color4;
            if (changedProps.TryGetValue("Color4", out color4))
            {
                user4selectcolor.GetComponent<TMP_Dropdown>().value = int.Parse(color4.ToString());
                SelectColor[3] = int.Parse(color4.ToString());
            }
            int namecount = 0;
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                namecount++;
                switch (namecount)
                {
                    case 1:
                        GameObject player1 = GameObject.Find("WaitingPlayer").transform.Find("Player1").gameObject;
                        player1.SetActive(true);
                        user1Text.text = p.NickName.ToString();
                        break;
                    case 2:
                        GameObject player2 = GameObject.Find("WaitingPlayer").transform.Find("Player2").gameObject;
                        player2.SetActive(true);
                        user2Text.text = p.NickName.ToString();
                        break;
                    case 3:
                        GameObject player3 = GameObject.Find("WaitingPlayer").transform.Find("Player3").gameObject;
                        player3.SetActive(true);
                        user3Text.text = p.NickName.ToString();
                        break;
                    case 4:
                        GameObject player4 = GameObject.Find("WaitingPlayer").transform.Find("Player4").gameObject;
                        player4.SetActive(true);
                        user4Text.text = p.NickName.ToString();
                        break;
                    case 5:
                        playercnt = 0;
                        break;
                    case 0:
                        break;
                }
            }
            object gameplay_flag1;
            if (changedProps.TryGetValue("Gameplay", out gameplay_flag1))
            {
                gameplay_flag = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                GameManager.PlayerName=AuthenManager.current_username;
                GameManager.MaxPlayer=PhotonNetwork.CurrentRoom.MaxPlayers;
                GameManager.timer_count = timer;
                List<string> colorslist = new List<string>();
                for(int i=0; i < PhotonNetwork.CurrentRoom.PlayerCount;i++)
                {
                    int colorlist = SelectColor[i];
                    string SelectedColorlist;
                    switch (colorlist)
                    {
                        case 0:
                            SelectedColorlist = "Red";
                            colorslist.Add( SelectedColorlist);
                            break;
                        case 1:
                            SelectedColorlist = "Yellow";
                            colorslist.Add(SelectedColorlist);
                            break;
                        case 2:
                            SelectedColorlist = "Purple";
                            colorslist.Add(SelectedColorlist);
                            break;
                        case 3:
                            SelectedColorlist = "Green";
                            colorslist.Add(SelectedColorlist);
                            break;
                    }
                }
                for(int i=0; i < colorslist.Count; i++)
                {
                    GameManager.playerColorlist.Add(colorslist[i]);
                }
                int color = SelectColor[PlayerColor-1];
                string SelectedColor;
                switch (color)
                {
                    case 0: SelectedColor = "Red";
                        GameManager.PlayerColor= SelectedColor;
                        break;
                    case 1:
                        SelectedColor = "Yellow";
                        GameManager.PlayerColor = SelectedColor;
                        break;
                    case 2:
                        SelectedColor = "Purple";
                        GameManager.PlayerColor = SelectedColor;
                        break;
                    case 3:
                        SelectedColor = "Green";
                        GameManager.PlayerColor = SelectedColor;
                        break;
                }
                SceneManager.LoadScene("Map");
            }
            object check;
            if(changedProps.TryGetValue("isPlayerReady", out check))
            {
                readyflag++;
                bool ready_flag = false;
                if (readyflag == PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    ready_flag = true;
                    LocalPlayerPropertiesUpdated(ready_flag);
                }
                else
                {
                    ready_flag = false;
                }
            }
            object isReady1;
            if(changedProps .TryGetValue("isPlayerReady1",out isReady1))
            {
                Ready1Button.gameObject.SetActive(false);
                readystate1.gameObject.SetActive(true);
            }
            object isReady2;
            if (changedProps.TryGetValue("isPlayerReady2", out isReady2))
            {
                Ready2Button.gameObject.SetActive(false);
                readystate2.gameObject.SetActive(true);
            }
            object isReady3;
            if (changedProps.TryGetValue("isPlayerReady3", out isReady3))
            {
                Ready3Button.gameObject.SetActive(false);
                readystate3.gameObject.SetActive(true);
            }
            object isReady4;
            if (changedProps.TryGetValue("isPlayerReady4", out isReady4))
            {
                Ready4Button.gameObject.SetActive(false);
                readystate4.gameObject.SetActive(true);
            }
        }

        #endregion

        #region UI CALLBACKS

        public void OnBackButtonClicked()
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }
        }
       
        public void OnCreateRoomButtonClicked()
        {
            int temp= MaxPlayersInputField.value;
            if (temp == 0) {
                MaxplayernNum = 2;
            }
            else if (temp == 1)
            {
                MaxplayernNum = 3;
            }
            else if (temp == 2)
            {
                MaxplayernNum = 4;
            }
            string roomName = AuthenManager.current_username;
            roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;
            byte maxPlayers;
            byte.TryParse(MaxplayernNum.ToString(), out maxPlayers);
            maxPlayers = (byte)Mathf.Clamp(MaxplayernNum, 2, 4);
            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000 };
            PhotonNetwork.CreateRoom(roomName, options, null);
        }
        public void OnPlaybuttonclicked()
        {
            gameplay_flag = true;
            Hashtable gameplay_mode = new Hashtable {
                 {"Gameplay",  gameplay_flag}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(gameplay_mode);
        }
       
        public void OnJoinRoomButtonClicked()
        {
            loadingroomlist_panel.SetActive(true);
            PhotonNetwork.JoinRandomRoom();
        }

        public void OnLeaveGameButtonClicked()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void Start()
        {
            Loading_Panel.SetActive(true);
            AuthenManager.compare_flag = false;
            CountName = AuthenManager.current_username;
            if (!AuthenManager.current_username.Equals(""))
            {
                PhotonNetwork.LocalPlayer.NickName = AuthenManager.current_username;
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                Debug.LogError("Player Name is invalid.");
            }

        }
        public void Update()
        {
            if (connected_flag && PhotonNetwork.IsConnectedAndReady)
            {
                Loading_Panel.SetActive(false);
                connected_flag = false;
            }
        }
        public void OnRoomListButtonClicked()
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }

        }
   
        #endregion

        private void ClearRoomListView()
        {
            foreach (GameObject entry in roomListEntries.Values)
            {
                Destroy(entry.gameObject);
            }
            roomListEntries.Clear();
        }

        public void LocalPlayerPropertiesUpdated(bool readyflag)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject getting = GameObject.Find("WaitingPlayer").transform.Find("Start_btn").gameObject;
                getting.SetActive(readyflag);
            }
        }

        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }
                    continue;
                }
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }
        
        private void UpdateRoomListView()
        {
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                GameObject entry = Instantiate(RoomListEntryPrefab);
                entry.SetActive(true);
                entry.transform.SetParent(RoomListContent.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, (byte)info.MaxPlayers);
                roomListEntries.Add(info.Name, entry);
            }
        }
    }
}