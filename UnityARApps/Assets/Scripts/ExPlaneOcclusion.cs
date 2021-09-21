using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityAR {

    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    [RequireComponent(typeof(PlayerInput))]

    public class ExPlaneOcclusion : MonoBehaviour
    {
        [SerializeField] Text message;
        [SerializeField] GameObject placementPrefab;
        [SerializeField] GameObject occlusionPlane;

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

            planeManager = GetComponent<ARPlaneManager>();
            raycastManager = GetComponent<ARRaycastManager>();
            playerInput = GetComponent<PlayerInput>();

            if(
                placementPrefab == null
                || occlusionPlane == null
                || planeManager == null
                || planeManager.planePrefab == null
                || raycastManager == null
                || playerInput == null
                || playerInput.actions == null
            ) {
                isReady = false;
                ShowMessage("エラー：SerializeFieldなどの初期値不備");
            }
            else {
                isReady = true;
                ShowMessage("床を撮影してください。しばらくすると平面が認識されます。平面をタップすると椅子が表示されます。");
            }
        }

        GameObject instantiateObject = null;

        void OnTouch(InputValue touchInfo) {
            if(!isReady || instantiateObject != null) return;

            // 画面上のタッチした位置の座標を取得
            var touchPoint = touchInfo.Get<Vector2>();

            // レイキャストで取得した位置情報を格納する配列を定義
            var hits = new List<ARRaycastHit>();

            // レイキャストが成功したら
            if(raycastManager.Raycast(touchPoint,hits,TrackableType.PlaneWithinPolygon)) {
                // 配列の先頭を取得
                var hitPose = hits[0].pose;

                // オブジェクトを指定位置に配置
                instantiateObject = Instantiate(placementPrefab,hitPose.position,hitPose.rotation);

                foreach(var plane in planeManager.trackables) {
                    plane.gameObject.SetActive(false);
                }

                planeManager.planePrefab = occlusionPlane;
                ShowMessage("平面オクルージョン");
                AddMessage("仮想物体より前にある物体を撮影し、オクルージョンの状況を確認してください");
            }
        }
    }
}
