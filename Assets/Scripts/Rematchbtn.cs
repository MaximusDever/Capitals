using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun.Demo.Asteroids;
using ExitGames.Client.Photon;
using Photon.Realtime;
using TMPro;

namespace Photon.Pun.Demo.Asteroids
{
    public class Rematchbtn : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        public void rematch_clicked()
        {
            GameObject.FindObjectOfType<GameManager>().ReMatch();
        }
        public void exit_game_clicked()
        {
            GameObject.FindObjectOfType<GameManager>().LeaveGame();
        }
    }
}
