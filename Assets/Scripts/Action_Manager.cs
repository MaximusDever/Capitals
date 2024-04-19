using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun.Demo.Asteroids;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.Demo.Asteroids
{
    public class Action_Manager : MonoBehaviourPunCallbacks
    {
        public Button AirSend_btn;
        public Button AirReceive_btn;
        public Button AirBomb_btn;
        public Button AirTransfer_btn;
        public Button AirDrop_btn;
        public Button AirAssault_btn;
        public Button SeaSend_btn;
        public Button SeaReceive_btn;
        public GameObject landTransfer_Panel;
        public GameObject seaTransfer_Panel;
        public GameObject SendHex;
        public GameObject ReceiveHex;
        public GameObject SeaSendHex;
        public GameObject SeaReceiveHex;
        public bool Landstatus = false;
        public string action_mode;
        public bool embarkEnable = false;
        public bool disembarkEnable = false;
        // Start is called before the first frame update
        void Start()
        {
            AirSend_btn.onClick.AddListener(() =>
            {
                if (!Landstatus)
                {
                    Landstatus = true;
                    SendUnit(SendHex);
                    SendHex.GetComponent<Unit>().path_init();
                }
            });
            AirReceive_btn.onClick.AddListener(() =>
            {
                if (Landstatus)
                {
                    Landstatus = false;
                    ReceiveHex.GetComponent<Unit>().path_init();
                    AirReceive_btn.gameObject.SetActive(false);
                    ReceiveUnit(ReceiveHex);
                }
            });
            SeaSend_btn.onClick.AddListener(() =>
            {
                SeaSendHex.GetComponent<Unit>().path_init();
                disembarkEnable = true;
                GameObject disembarkUnit = SeaSendHex.GetComponent<Unit>().disembarkUnitCheck(); //Get embarked unit on the navy to disembark
                disembarkUnit.GetComponent<Unit>().SeaSend();
                SeaSendHex.GetComponent<Unit>().doubleMode = false;
                SeaSendHex.GetComponent<Unit>().doubleStack = null;
                SeaSend_btn.gameObject.SetActive(false);
            });
            SeaReceive_btn.onClick.AddListener(() =>
            {
                SeaReceiveHex.GetComponent<Unit>().path_init();
                embarkEnable = true;
                SeaReceive_btn.gameObject.SetActive(false);
            });
            AirTransfer_btn.onClick.AddListener(() =>
            {
                action_mode = "airTransfer";
            });
            AirBomb_btn.onClick.AddListener(() =>
            {
                Landstatus = false;
                action_mode = "airBombard";
                SendHex.GetComponent<Unit>().actionMove = false;
                SendHex.GetComponent<Unit>().action_Mode = action_mode;
                SendHex.GetComponent<Unit>().GetHexsInRangeAroundUnitTobomb();
            });
            AirDrop_btn.onClick.AddListener(() =>
            {
                Landstatus = false;
                action_mode = "airDrop";
                SendHex.GetComponent<Unit>().actionMove = false;
                SendHex.GetComponent<Unit>().action_Mode = action_mode;
                SendHex.GetComponent<Unit>().GetHexsInRangeAroundUnit();
            });
            AirAssault_btn.onClick.AddListener(() =>
            {
                Landstatus = false;
                action_mode = "airAssault";
                SendHex.GetComponent<Unit>().actionMove = false;
                SendHex.GetComponent<Unit>().action_Mode = action_mode;
                SendHex.GetComponent<Unit>().GetHexsInRangeAroundUnit();
            });
        }

        public void selectedLandUnit(GameObject unit, bool double_mode)
        {
            GameObject selected_unit = unit.gameObject;
            landTransfer_Panel.SetActive(true);
            seaTransfer_Panel.SetActive(false);
            Transfer_Landstatus(selected_unit, double_mode);
        }
        public void selectedSeaUnit(GameObject unit, bool double_mode)
        {
            seaTransfer_Panel.SetActive(true);
            landTransfer_Panel.SetActive(false);
            if (double_mode)
            {
                SeaSend_btn.gameObject.SetActive(true);
                SeaReceive_btn.gameObject.SetActive(false);
                SeaSendHex = unit.gameObject;
            }
            else
            {
                SeaReceive_btn.gameObject.SetActive(true);
                SeaSend_btn.gameObject.SetActive(false);
                SeaReceiveHex = unit.gameObject;
            }
        }
        public void Transfer_Landstatus(GameObject unit, bool double_mode)
        {
            if (!Landstatus && double_mode)
            {
                SendHex = null;
                commandPanel_init();
                if (unit.GetComponent<Unit>().unitType != Unit.UnitType.Navy)
                {
                    AirSend_btn.gameObject.SetActive(true);
                }
                else
                {
                    SeaSend_btn.gameObject.SetActive(true);
                }
                SendHex = unit.gameObject;
            }
            else if (Landstatus && !double_mode)
            {
                ReceiveHex = null;
                commandPanel_init();
                AirReceive_btn.gameObject.SetActive(true);
                ReceiveHex = unit.gameObject;
            }
            else
            {

            }
        }
        public void SeaReceive(GameObject embarkUnit)
        {
            embarkUnit.GetComponent<Unit>().SeaReceive(SeaReceiveHex);
        }

        public void ReceiveUnit(GameObject unit)
        {
            ReceiveHex = unit.gameObject;
            SendHex.GetComponent<Unit>().UnitAction(action_mode, SendHex, unit);
            print("ReceiveUnit is " + ReceiveHex.name);
        }
        public void SendUnit(GameObject unit)
        {
            AirSend_btn.gameObject.SetActive(false);
            print("SendUnit is " + SendHex.name);
            actionMatrix(unit);
        }
        //Enable/disable commanbd buttons according to available unit to act
        private void actionMatrix(GameObject startHex)
        {
            switch (startHex.GetComponent<Unit>().unitType)
            {
                case Unit.UnitType.AirForce:
                    commandPanel_init();
                    AirTransfer_btn.gameObject.SetActive(true);
                    AirBomb_btn.gameObject.SetActive(true);
                    break;
                case Unit.UnitType.Paratrooper:
                    commandPanel_init();
                    AirTransfer_btn.gameObject.SetActive(true);
                    AirAssault_btn.gameObject.SetActive(true);
                    AirDrop_btn.gameObject.SetActive(true);
                    break;
                case Unit.UnitType.Army:
                    commandPanel_init();
                    AirTransfer_btn.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
        private void commandPanel_init()
        {
            AirSend_btn.gameObject.SetActive(false);
            AirTransfer_btn.gameObject.SetActive(false);
            AirBomb_btn.gameObject.SetActive(false);
            AirAssault_btn.gameObject.SetActive(false);
            AirDrop_btn.gameObject.SetActive(false);
            AirReceive_btn.gameObject.SetActive(false);
        }
    }
}
