using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityAR {
    public class ExCheckAvailableFeatures : MonoBehaviour
    {
        [SerializeField] Text message;
        [SerializeField] ARSession session;
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

            if(session == null) {
                isReady = false;
                ShowMessage("エラー：SerializeFieldなどの初期化不備");
            }
            else {
                isReady = true;
            }
        }

        // サポート確認
        bool planeDetectionSupported = false;
        bool raycastSupported = false;
        bool faceTrackingSupported = false;
        bool eyeTrackingSupported = false;
        bool faceTrackingWithWorldFacingCameraSupported = false;
        bool faceTrackingWithUserFacingCameraSupported = false;

        void CheckAvailableFeatures() {
            var planeDescriptors = new List<XRPlaneSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(planeDescriptors);
            planeDetectionSupported = planeDescriptors.Count > 0;

            var raycastDescriptors = new List<XRRaycastSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(raycastDescriptors);
            raycastSupported = raycastDescriptors.Count > 0;

            var faceTrackingDescriptors = new List<XRFaceSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(faceTrackingDescriptors);
            
            
            if(faceTrackingDescriptors.Count > 0) {
                faceTrackingSupported = true;

                foreach(var faceDescriptor in faceTrackingDescriptors) {
                    if(faceDescriptor.supportsEyeTracking) {
                        eyeTrackingSupported = true;
                        break;
                    }
                }
            }

            // よくわからん
            var configs = session.subsystem.GetConfigurationDescriptors(Unity.Collections.Allocator.Temp);

            if(configs.IsCreated) {
                using(configs) {
                    foreach(var config in configs) {
                        if(config.capabilities.All(Feature.WorldFacingCamera | Feature.FaceTracking)) {
                            faceTrackingWithWorldFacingCameraSupported = true;
                        }

                        if(config.capabilities.All(Feature.UserFacingCamera | Feature.FaceTracking)) {
                            faceTrackingWithUserFacingCameraSupported = true;
                        }
                    }
                }
            }
            
        }
   
        void Start()
        {
            // 初期化できていないなら何もしない
            if(!isReady) return;

            // サポート情報取得
            CheckAvailableFeatures();

            ShowMessage("機能のサポート調査");
            AddMessage($"平面検出：{planeDetectionSupported}");
            AddMessage($"レイキャスト：{raycastSupported}");
            AddMessage($"顔追随：{faceTrackingSupported}");
            AddMessage($"目線追随：{eyeTrackingSupported}");
            AddMessage($"フロント側カメラ顔追随：{faceTrackingWithWorldFacingCameraSupported}");
            AddMessage($"リア側カメラ顔追随：{faceTrackingWithUserFacingCameraSupported}");
        }
    }
}
