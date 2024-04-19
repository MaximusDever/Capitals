using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.Demo.Asteroids;

namespace Photon.Pun.Demo.Asteroids
{
    public class TurnSymbol : MonoBehaviourPunCallbacks
    {
        public List<string> PlayerColors = new List<string>();

        void Start()
        {
            List<string> colorRank = new List<string> { "Red", "Yellow", "Green", "Purple" };
            List<int> indexofRank = new List<int>();
            for(int i = 0; i < GameManager.playerColorlist.Count; i++)
            {
                indexofRank.Add(colorRank.IndexOf(GameManager.playerColorlist[i]));
            }
            indexofRank.Sort();
            foreach(int i in indexofRank)
            {
                PlayerColors.Add(colorRank[i]);
            }
            if (PlayerColors.Count > 0)
            {
                transform.GetComponent<Image>().color = GetColorFromString(PlayerColors[0]);
            }
        }

        [PunRPC]
        public void TurnStatus(string playerColor, bool turnFlag)
        {
            if (!turnFlag)
            {
                int currentIndex = PlayerColors.IndexOf(playerColor);
                int nextIndex = (currentIndex+1) % PlayerColors.Count;
                
                string nextColor = PlayerColors[nextIndex];
                print("Turn is Over and Next Turn" + playerColor + nextColor);
                GetComponent<Image>().color = GetColorFromString(nextColor);
                GameObject.FindObjectOfType<GameManager>().TurnDetection();
            }
        }

        private Color GetColorFromString(string colorString)
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
        // Update is called once per frame
        void Update()
        {

        }
    }
}
