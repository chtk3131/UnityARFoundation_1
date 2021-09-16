using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace UnityAR {

    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    [RequireComponent(typeof(PlayerInput))]

    public class ExPlaneDetection : MonoBehaviour {
        [SerializeField] Text message;
        [SerializeField] GameObject placementPrefab;

        ARPlaneManager planeManager;
        ARRaycastManager raycastManager;
        PlayerInput playerInput;
        bool isReady;

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

            // 各コンポーネントを初期化
            planeManager = GetComponent<ARPlaneManager>();
            raycastManager = GetComponent<ARRaycastManager>();
            playerInput = GetComponent<PlayerInput>();

            // 各コンポーネントやフィールドに不備がないかチェック
            if(
                placementPrefab == null
                || planeManager == null
                || planeManager.planePrefab == null
                || raycastManager == null
                || playerInput == null
                || playerInput.actions == null
            ) {
                isReady = false;
                ShowMessage("エラー：SerializeFieldなどの初期化不備");
            }
            else {
                // 初期化が完了したことを通知
                isReady = true;
                ShowMessage("初期化完了");
                AddMessage("床を撮影してください。しばらくすると平面が検出されます。");
                AddMessage("検出された平面をタップすると、椅子が表示されます。");
            }
        }
    
        GameObject instantiatedObject = null;

        void OnTouch(InputValue touchInfo) {

            // ARの初期化が完了していなければ何も起こさない
            if(!isReady) return;

            // ディスプレイのタッチした部分の座標を取得
            var touchPosition = touchInfo.Get<Vector2>();
            
            // レイキャストのヒット結果を格納する配列を定義
            var hits = new List<ARRaycastHit>();

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                // レイキャストで取得した１つめの情報にアクセス
                var hitPose = hits[0].pose;

                // objectインスタンスを作成
                if(instantiatedObject == null) {
                    instantiatedObject = Instantiate(placementPrefab,hitPose.position,hitPose.rotation);
                }
                // すでに作成済みなら位置だけ変更する
                else {
                    instantiatedObject.transform.position = hitPose.position;
                }
            }
        }
    }
}
