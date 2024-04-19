using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;


namespace Photon.Pun.Demo.Asteroids
{
    public class playerselectcolor2 : MonoBehaviour
    {
        public GameObject player;
        public TMP_Dropdown colorselect;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void select_valuechanged()
        {
            Hashtable Color = new Hashtable {
                {"Color2",  colorselect.GetComponent<TMP_Dropdown>().value}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(Color);
            
        }
        public void selected_color()
        {
            colorselect.GetComponent<TMP_Dropdown>().enabled = false;
            Hashtable Colorforall = new Hashtable {
                {"Everycolor",  colorselect.GetComponent<TMP_Dropdown>().value}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(Colorforall);
        }
    }
}
