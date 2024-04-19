using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.Demo.Asteroids;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.Demo.Asteroids
{
    public class cell : MonoBehaviourPunCallbacks
    {
        public bool action_move = true;  //Determine either movement or action movement
        public string action_mode;       //action info of urrently selected unit
        public bool attack_mode=false;  // Start is called before the first frame update
        public GameObject spotedenemy;
        void Start()
        {
            gameObject.GetComponent<cell>().spotedenemy =enemyRetrive();
        }
        private GameObject enemyRetrive()
        {
            print("Enemy???");
            for (int i = 0; i < GameObject.FindObjectOfType<GameManager>().Other_Panel.childCount; i++)
            {
                print(GameObject.FindObjectOfType<GameManager>().Other_Panel.childCount);
                GameObject other = GameObject.FindObjectOfType<GameManager>().Other_Panel.GetChild(i).gameObject;
                float width = other.GetComponent<RectTransform>().rect.width;
                if (other.transform.position.x > transform.position.x - width / 2 && other.transform.position.x < transform.position.x + width / 2 && other.transform.position.y > transform.position.y - width / 2 && other.transform.position.y < transform.position.y + width / 2)
                {
                    print("Enemy is spotted!"+ ColorUtility.ToHtmlStringRGBA(other.GetComponent<Image>().color));
                    gameObject.GetComponent<cell>().attack_mode = true;
                    GameObject enemy = other.gameObject;
                    return enemy;
                }
            }
            return null;
        }
        public void cell_clicked()
        {
            GameObject cell_parent = transform.parent.gameObject;
            GameObject my_unit = cell_parent.transform.parent.gameObject;
            if (action_move)
            {
                if (spotedenemy != null)
                {
                    my_unit.GetComponent<Unit>().MoveTo(gameObject,spotedenemy);
                }
                else
                {
                    my_unit.GetComponent<Unit>().MoveTo(gameObject,null);
                }
            }
            else
            {
                Debug.LogError("action move");
                my_unit.GetComponent<Unit>().Action_MoveTo(gameObject, action_mode);
            }
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
} 
