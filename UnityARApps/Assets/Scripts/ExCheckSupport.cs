using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace UnityAR{
    public class ExCheckSupport : MonoBehaviour
    {
        [SerializeField] Text message;
        [SerializeField] ARSession session;
        bool isReady = false;

        void ShowMessage(string text) {
            message.text = $"{text}\r\n";
        }

        void AddMessage(string text) {
            message.text += $"{text}\r\n";
        }

        void Awake()
        {
            if(message == null) {
                Application.Quit();
            }

            if(session == null) {
                isReady = false;
                ShowMessage("エラー：SerializedFieldの初期設定不良");
            } else {
                isReady = true;
                ShowMessage("ARサポート調査");
            }
        }

        IEnumerator CheckSupport() {
            yield return ARSession.CheckAvailability();

            if(ARSession.state == ARSessionState.NeedsInstall) {
                ShowMessage("ARのインストールが必要。");
                yield return ARSession.Install();
            }

            if(ARSession.state == ARSessionState.NeedsInstall || ARSession.state == ARSessionState.Installing) {
                ShowMessage("ソフトウェアの更新に失敗、または更新を拒否されました。");
                AddMessage($"state:{ARSession.state}");
                yield break;
            }

            if(ARSession.state == ARSessionState.Unsupported) {
                ShowMessage("このデバイスはARをサポートしていません。");
                AddMessage($"state:{ARSession.state}");
                yield break;
            }

            ShowMessage("このデバイスはARをサポートしていませす。");
            ShowMessage("ARセッションの初期化..");

            session.enabled = true;
            const float interval = 30f;
            var timer = interval;

            while((ARSession.state == ARSessionState.Ready || ARSession.state == ARSessionState.SessionInitializing) && timer > 0) {
                var waitTime = 0.5f;
                timer -= waitTime;
                yield return new WaitForSeconds(waitTime);
            }

            if(timer < 0) {
                ShowMessage("初期化タイムオーバー！");
                AddMessage($"state:{ARSession.state}");
                yield break;
            }

            AddMessage("初期化完了！");
            AddMessage($"state:{ARSession.state}");

            AddMessage($"state:{ARSession.state}");
        }

        void OnEnable()
        {
         if(!isReady) return;

         StartCoroutine(CheckSupport());  
        }
    }
}
