using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Photon.Pun.Demo.Asteroids
{
    public class ready3chek : MonoBehaviour
    {
        private string isplayerReady = "isPlayerReady";
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void onReadyButtonclicked()
        {
            GameObject.FindObjectOfType<playerselectcolor3>().selected_color();
            Hashtable Readystatus = new Hashtable {
                {"isPlayerReady3",isplayerReady }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(Readystatus);
            Hashtable Readystatus1 = new Hashtable {
                {"isPlayerReady",isplayerReady }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(Readystatus1);
        }
    }
}