using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using SimpleFirebaseUnity;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class AuthenManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static bool compare_flag = false;
    public bool username_compare_flag = false;
    public bool password_compare_flag = false;
    public bool email_compare_flag = false;
    public static string current_username;
    public static string current_password;

    public GameObject SIGN_PANEL;
    public GameObject LOGIN_PANEL;
    public GameObject RESET_PANEL;
    public GameObject  ALERT_PANEL;

    public InputField UserName;
    public InputField Password;
    public InputField sign_UserName;
    public InputField sign_Password;
    public InputField sign_RePassword;
    public InputField sign_Email;
    public InputField reset_Password;
    public InputField reset_Email;

    public TMP_Text content_warning;
    public TMP_Text title_warning;

    public string sign_username;
    public string sign_email;
    public string sign_password;
    public string sign_repassword;
    public string registered_password;
    public string registered_username;
    public string registered_email;

    List<string> Key = new List<string>();
    List<string> ListName = new List<string>();
    List<string> ListEmail = new List<string>();
    List<string> ListPassword = new List<string>();

    Firebase firebase = Firebase.CreateNew("https://hexcapitals-bc6de-default-rtdb.firebaseio.com", "WQV9t78OywD8Pp7jvGuAi8K6g0MV8p9FAzkJ7rWK");
    FirebaseQueue firebaseQueue = new FirebaseQueue(true, 3, 1f);

    // Start is called before the first frame update
    void Start()
    {
        firebase.OnGetSuccess += GetOKHandler;
        firebase.OnGetFailed += GetFailHandler;
        firebase.OnSetSuccess += SetOKHandler;
        firebase.OnSetFailed += SetFailHandler;
        firebase.OnUpdateSuccess += UpdateOKHandler;
        firebase.OnUpdateFailed += UpdateFailHandler;
        firebase.OnPushSuccess += PushOKHandler;
        firebase.OnPushFailed += PushFailHandler;
        firebase.OnDeleteSuccess += DelOKHandler;
        firebase.OnDeleteFailed += DelFailHandler;

        GetData();
    }
    public void GetData() { 
        firebaseQueue.AddQueueGet(firebase.Child("Users", true));
    }
    // Update is called once per frame
    void Update()
    {
        GetData();
    }

    public void OnLoginButtonclicked()
    {
        
        string username = UserName.text;
        string password = Password.text;
        foreach (string names in ListName)
        foreach(string passes in ListPassword)
        {
            if (username == names) {
                username_compare_flag = true;
            }
            if (password == passes)
            {
                password_compare_flag = true;
            }
            }
        if (username_compare_flag && password_compare_flag && UserName.text != "" && Password.text != "")
        {
            current_username = UserName.text;
            current_password = Password.text;
            SceneManager.LoadScene("Room");
        }
        else if(!username_compare_flag || UserName.text == "")
        {
            ALERT_PANEL.SetActive(true);
            content_warning.text = ("Please enter a valid user name").ToString();
            title_warning.text = ("WARNING").ToString();
        }
        else if (!password_compare_flag || Password.text == "")
        {
            ALERT_PANEL.SetActive(true);
            content_warning.text = ("Please enter a valid password").ToString();
            title_warning.text = ("WARNING").ToString();
        }
    }
    
    public void OnRegisterButtonclicked()
    {
        sign_username = sign_UserName.text;
        sign_password = sign_Password.text;
        sign_repassword = sign_RePassword.text;
        sign_email = sign_Email.text;
        if (sign_password == sign_repassword && sign_username != "" && sign_password != "" && sign_email != "" && sign_repassword != "")
        {
            StartCoroutine(Tests());
            ALERT_PANEL.SetActive(true);
            content_warning.text = ("Successfully registered").ToString();
            title_warning.text = ("WELCOME").ToString();
            SIGN_PANEL.SetActive(false);
            LOGIN_PANEL.SetActive(true);
        }
        else { 
        }
    }

    public void OnResetBttonclick() {
        int i=0;
        foreach (string emails in ListEmail)
        {
            if (emails == reset_Email.text)
            {
                firebaseQueue.AddQueueDelete(firebase.Child("Users", true).Child(Key[i], true).Child("UserPassword", true));
                firebaseQueue.AddQueueUpdate(firebase.Child("Users",true).Child(Key[i], true), "{\"UserPassword\":\""+reset_Password.text+"\"}");
                email_compare_flag = true;
            }
            i++;
        }
        if (email_compare_flag && reset_Email.text != "" && reset_Password.text != "")
        {
            email_compare_flag = false;
            ALERT_PANEL.SetActive(true);
            content_warning.text = ("Password changed successfully").ToString();
            title_warning.text = ("WELCOME").ToString();
            RESET_PANEL.SetActive(false);
            LOGIN_PANEL.SetActive(true);
        }
        else
        {
        }
    }

    void GetTimeStamp(Firebase sender, DataSnapshot snapshot)
    {
        long timeStamp = snapshot.Value<long>();
        
    }
    void GetOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        
        if (snapshot.RawJson != "null")
        {
            JObject converted = JsonConvert.DeserializeObject<JObject>(snapshot.RawJson.ToString());
            if (converted != null && converted.Count > 0)
            {
                foreach (KeyValuePair<string, JToken> keyValuePair in converted)
                {
                    Key.Add(keyValuePair.Key);
                    JObject convert = JsonConvert.DeserializeObject<JObject>(keyValuePair.Value.ToString());
                    if (convert != null && convert.Count > 0)
                    {
                        foreach (KeyValuePair<string, JToken> keyValuePair1 in convert)
                        {
                            if (keyValuePair1.Key == "UserName")
                            {
                                ListName.Add(keyValuePair1.Value.ToString());
                            }
                            if (keyValuePair1.Key == "UserEmail")
                            {
                                ListEmail.Add(keyValuePair1.Value.ToString());
                            }
                            if (keyValuePair1.Key == "UserPassword")
                            {
                                ListPassword.Add(keyValuePair1.Value.ToString());
                            }
                        }
                        
                    }
                }
            }
        }
        
    }

    void GetFailHandler(Firebase sender, FirebaseError err)
    {
    }

    void SetOKHandler(Firebase sender, DataSnapshot snapshot)
    {
    }

    void SetFailHandler(Firebase sender, FirebaseError err)
    {
    }

    void UpdateOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        
    }

    void UpdateFailHandler(Firebase sender, FirebaseError err)
    {
    }

    void DelOKHandler(Firebase sender, DataSnapshot snapshot)
    {
    }

    void DelFailHandler(Firebase sender, FirebaseError err)
    {
    }

    void PushOKHandler(Firebase sender, DataSnapshot snapshot)
    {
    }

    void PushFailHandler(Firebase sender, FirebaseError err)
    {
    }

    IEnumerator Tests()
    {
        Firebase lastUpdate = firebase.Child("lastUpdate");
        lastUpdate.OnGetSuccess += GetTimeStamp;


        FirebaseQueue firebaseQueue = new FirebaseQueue(true, 3, 1f);

        // Make observer on "last update" time stamp
        FirebaseObserver observer = new FirebaseObserver(lastUpdate, 1f);
       
        observer.Start();
       
        yield return null;

        // Create a FirebaseQueue
        firebaseQueue.AddQueuePush(firebase.Child("Users", true), "{ \"UserName\": \"" + sign_username + "\", \"UserEmail\": \""+sign_email+"\", \"UserPassword\": \""+sign_password+"\"}", true);
        //firebaseQueue.AddQueueSetTimeStamp(firebase, "lastUpdate");
        //firebaseQueue.AddQueueGet(firebase, "print=pretty");
        //firebaseQueue.AddQueueUpdate(firebase.Child("layout", true), "{\"x\": 5.0, \"y\":-94}");
        //firebaseQueue.AddQueueGet(lastUpdate);

        // ~~(-.-)~~
        yield return null;
        yield return new WaitForSeconds(1.5f);
        observer.Stop();
    }
    
}
