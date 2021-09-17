using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace UnityAR {
    // 必要なコンポーネントを定義
    [RequireComponent(typeof(ARSessionOrigin))]
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    [RequireComponent(typeof(PlayerInput))]

    public class ExMakeAppearOnPlane : MonoBehaviour
    {
        ARSessionOrigin sessionOrigin;
        ARPlaneManager planeManager;
        ARRaycastManager raycastManager;
        PlayerInput playerInput;

        // 配置するプレファブ
        [SerializeField] GameObject placementPrefab;

        GameObject instantiatedObject = null;

        float scale;
        public float Scale {
            get {return scale;}
            set {
                scale = value;
                // ARセッションが有効＆一時オブジェクトがnullでないなら処理
                if(sessionOrigin != null && instantiatedObject != null) {
                    // 仮想物体が拡大or縮小
                    sessionOrigin.transform.localScale = Vector3.one / scale;
                }
            }
        }

        Quaternion rotation;
        public Quaternion Rotation {
            get {return rotation;}
            set {
                rotation = value;

                // ARセッションが有効かつ一時オブジェクトが存在しているなら
                if(sessionOrigin != null && instantiatedObject != null) {
                    sessionOrigin.MakeContentAppearAt(
                        instantiatedObject.transform,
                        instantiatedObject.transform.position,
                        rotation
                    );
                }
            }
        }

        public bool IsAvailable {get;private set;}

        void Awake()
        {
            // gameobjectの初期化
            sessionOrigin = GetComponent<ARSessionOrigin>();
            planeManager = GetComponent<ARPlaneManager>();
            raycastManager = GetComponent<ARRaycastManager>();
            playerInput = GetComponent<PlayerInput>();

            if(
                sessionOrigin == null 
                || sessionOrigin.camera == null
                || planeManager == null
                || planeManager.planePrefab == null
                || raycastManager == null
                || playerInput == null
                || playerInput.actions == null
                || placementPrefab == null
            ) {
                IsAvailable = false;
            } else {
                IsAvailable = true;
            }
        }

        void OnTouch(InputValue touchInfo) {
            // 使用できないなら何もしない
            if(!IsAvailable) return;

            // タップした位置の座標を取得
            var touchPosition = touchInfo.Get<Vector2>();

            // raycastがヒットしたときに格納されるターゲットの配列
            var hits = new List<ARRaycastHit>();

            // raycastした結果成功したら
            if(raycastManager.Raycast(touchPosition,hits,TrackableType.PlaneWithinPolygon)) {
                // hitした先頭のデータを取得
                var hitPose = hits[0].pose;

                if(instantiatedObject == null) {
                    instantiatedObject = Instantiate(placementPrefab,hitPose.position,hitPose.rotation);
                }else {
                    sessionOrigin.MakeContentAppearAt(instantiatedObject.transform,hitPose.position,rotation);
                }
            }
        }

        void Update()
        {
            if(instantiatedObject == null) return;

            foreach(var plane in planeManager.trackables) {
                plane.gameObject.SetActive(false);
            }
        }
    }
}
