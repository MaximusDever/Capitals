// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerListEntry.cs" company="Exit Games GmbH">
//   Part of: Asteroid Demo,
// </copyright>
// <summary>
//  Player List Entry
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

namespace Photon.Pun.Demo.Asteroids
{
    public class PlayerListEntry : MonoBehaviour
    {
       public static bool joinedplayer_flag=false;
       public static bool joinedsuccess_flag=false;
        public static int playercnt=0;
        private void Update()
        {
            if (joinedplayer_flag == true) {
                joinedplayer_flag = false;
                joinedsuccess_flag = true;
                playercnt++;
                if (playercnt > PhotonNetwork.CurrentRoom.MaxPlayers) {
                    playercnt = 0;
                }
            }
        }
    }
}