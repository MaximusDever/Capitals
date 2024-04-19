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
    public class TimeLimitController : MonoBehaviour
    {
        public TMP_InputField timer;
        // Start is called before the first frame update
        void Start()
        {

        }
        public void timer_increaser_clicked()
        {
            print(timer.text.ToString());
            int timer_val;
            timer_val= int.Parse(timer.text.ToString());
            timer_val++;
            timer.text = timer_val.ToString();
        }
        public void timer_decreaser_clicked()
        {
            int timer_val;
            timer_val = int.Parse(timer.text.ToString());
            timer_val--;
            timer.text = timer_val.ToString();
        }
        public void timer_setting_applyBtn()
        {
            GameObject.FindObjectOfType<RoomManager>().timer = int.Parse(timer.text.ToString());
        }
    }
}
