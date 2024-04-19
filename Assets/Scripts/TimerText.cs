using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.Demo.Asteroids
{

    public class TimerText : MonoBehaviour
    {
        public TMP_Text TimeText;

        // Start is called before the first frame update
        void Start()
        {

        }
        [PunRPC]
        public void Timer(int minutes, int seconds)
        {
            string formattedTime = $"{minutes:00}:{seconds:00}";
            TimeText.text = formattedTime.ToString();
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
