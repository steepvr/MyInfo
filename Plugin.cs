using System;
using BepInEx;
using Photon.Pun;
using UnityEngine;
using GorillaNetworking;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;
using Utilla;
using System.Linq;
using System.Net;
using System.Collections;

namespace MyInfo
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        // GAME MODE/QUEUE MANAGER

        public string GameMode = "";
        public string Queue = "";

        // AVRAGE FPS:

        public int avgrageFrameRate;

        //INFO PAGES

        bool page1bool;
        bool page2bool;
        bool hidebool;

        // VOICE COMMAND STUFFS:

        private KeywordRecognizer keywordRecognizer;
        private Dictionary<string, Action> actions = new Dictionary<string, Action>();

        // MESSAGE STUFF

        public bool refresh;
        public string Message = "";
        public string github = "https://raw.githubusercontent.com/steepvr/MyInfo/main/GTAGHTUboard.txt";

        private void GetGitHubContent()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    Message = client.DownloadString(github);
                }
                catch (WebException ex)
                {
                    Debug.LogError("Error downloading content: " + ex.Message);
                }
            }
        }
        public IEnumerator RefreshMessage()
        {
            refresh = true;
            while (refresh)
            {
                GetGitHubContent();
                yield return new WaitForSeconds(5f);
            }
        }

        // WHOLE OTHER PART OF THE MOD:

        public void FixedUpdate()
        {

            GameObject MyInfo = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera/DebugCanvas/Text");

            // MYINFO

            GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/debugtext").SetActive(true);
            if (page1bool)
            {
                if (PhotonNetwork.InRoom)
                {
                    MyInfo.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
                    MyInfo.GetComponent<Text>().text = $"GAME TYPE: <color=green>{GameMode}</color>\r\nQUEUE: <color=green>{Queue}</color>\r\nCODE: <color=green>{PhotonNetwork.CurrentRoom.Name}</color>\r\nPLAYER COUNT: <color=green>{PhotonNetwork.CurrentRoom.PlayerCount}</color>\r\n\r\n\r\nFPS: <color=orange>{avgrageFrameRate}</color>";
                }
                if (!PhotonNetwork.InRoom)
                {
                    MyInfo.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
                    MyInfo.GetComponent<Text>().text = $"GAME TYPE: <color=red>NOT IN ROOM</color>\r\nQUEUE: <color=red>NOT IN ROOM</color>\r\nCODE: <color=red>NOT IN ROOM</color>\r\nPLAYER COUNT: <color=red>NOT IN ROOM</color>\r\n\r\n\r\nFPS: <color=orange>{avgrageFrameRate}</color>";
                }
            }
            if (page2bool)
            {
                    MyInfo.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
                    MyInfo.GetComponent<Text>().text = $"NAME: {PhotonNetwork.LocalPlayer.NickName}\r\nGTAG TIME: <color=green>{BetterDayNightManager.instance.currentTimeOfDay}</color>\r\nSHINY ROCK COUNT: <color=green>{CosmeticsController.instance.currencyBalance}</color>\r\n\r\n\r\nFPS: <color=orange>{avgrageFrameRate}</color>";
            }
            if (hidebool)
            {

                MyInfo.GetComponent<Text>().text = "";
            }
        }

        // VOICE RECOGNITION

        private void Start()
        {
            actions.Add("first page", PageOneEnable);
            actions.Add("second page", PageTwoEnable);
            actions.Add("hide", hide);


            keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());

            keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
            keywordRecognizer.Start();
            Events.GameInitialized += OnGameInitialized;
        }
        private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
        {
            actions[speech.text].Invoke();
        }

        private void PageOneEnable()
        {
            page1bool = true;
            page2bool = false;
            hidebool = false;
        }
        private void PageTwoEnable()
        {
            page2bool = true;
            page1bool = false;
            hidebool = false;
        }
        private void hide()
        {
            page2bool = false;
            page1bool = false;
            hidebool = true;
        }


        // STYLE MANAGEMENT

        private void OnGameInitialized(object sender, EventArgs e)
        {
            page1bool = true;
            page2bool = false;
            StartCoroutine(RefreshMessage());
        }

        void Update()
        {
            float current = 0;
            current = (int)(1f / Time.unscaledDeltaTime);
            avgrageFrameRate = (int)current;
            GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera/DebugCanvas/Text").GetComponent<Text>().alignment = TextAnchor.LowerLeft;
            GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera/DebugCanvas/Text").GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;

            GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/Main Camera/DebugCanvas").SetActive(true);

            // HOW TO USE BOARD

            GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/debugtext/debugtext").GetComponent<UnityEngine.UI.Text>().text = Message;
            GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/debugtext").GetComponent<UnityEngine.UI.Text>().text = "| HOW TO USE |";
            GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/debugtext").transform.localPosition = new Vector3(-28.4553f, 26.4324f, 0.5617f);

            // GAME MODE/QUEUE MANAGER SET

            if (PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("INFECTION"))
            {
                GameMode = "INFECTION";
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("CASUAL"))
            {
                GameMode = "CASUAL";
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("HUNT"))
            {
                GameMode = "HUNT";
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("BATTLEPAINTBRAWL"))
            {
                GameMode = "PAINTBALL";
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("COMPETITIVE"))
            {
                Queue = "COMPETITIVE";
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("CASUAL"))
            {
                Queue = "CASUAL";
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ToString().Contains("MINIGAMES"))
            {
                Queue = "MINIGAMES";
            }
        }
    }
}
