using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Rendering;


namespace UnityAR {
    [RequireComponent(typeof(Light))]
    
    public class ExLightEstimation : MonoBehaviour
    {
        [SerializeField] Text message;
        [SerializeField] ARCameraManager cameraManager;
        [SerializeField] GameObject worldSpaceObject;
        [SerializeField] GameObject lightDirectionArrow;
        Light directionLight;
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

            // GameObjectの初期化
            // cameraManager = GetComponent<ARCameraManager>(); // 初期化不要
            directionLight = GetComponent<Light>();

            if(
                directionLight == null
                || cameraManager == null
                || lightDirectionArrow == null
                || worldSpaceObject == null
            ) {
                isReady = false;
                ShowMessage("エラー：SerializeFieldなどの初期化不備");
            } else {
                isReady = true;
            }
        }

        void OnEnable()
        {
            // 初期化できていなければ何もしない
            if(!isReady) return;

            // フレーム情報を取得したときのイベントを追加
            cameraManager.frameReceived += FrameChanged;

            // レンダリング前に発生するイベント追加
            Application.onBeforeRender += OnBeforeRender;

            // 光源方向は表示しないでおく
            lightDirectionArrow.SetActive(false);
        }

        void OnDisable()
        {
            // 初期化できていなければ何もしない
            if(!isReady) return;

            // イベントを取り除く
            cameraManager.frameReceived -= FrameChanged;
            Application.onBeforeRender -= OnBeforeRender;
        }

        private void OnBeforeRender()
        {
            // 初期化できていなければ何もしない
            if(!isReady) return;

            // デバイスのカメラの位置を取得
            var cameraTransform = cameraManager.GetComponent<Camera>().transform;

            // 表示する仮想物体からカメラまでの距離を定義
            var distanceFromCamera = cameraTransform.forward * 10f;

            // 仮想物体（球体）の位置を定義
            // カメラの位置＋上で定義した距離
            worldSpaceObject.transform.position = cameraManager.transform.position + distanceFromCamera;

            // 主光源の方向を示すオブジェクトも同じ位置に配置
            lightDirectionArrow.transform.position = worldSpaceObject.transform.position;
        }

        // 光源推定の各種フィールド
        float? brightness;
        float? colorTemperature;
        Color? colorCorrection;
        Vector3? mainLightDirection;
        Color? mainLightColor;
        float? averageMainLightBrightness;
        SphericalHarmonicsL2? sphericalHarmonics;

        // カメラフレーム受信時に取得される
        private void FrameChanged(ARCameraFrameEventArgs obj)
        {
            // 初期化できていなければ何もしない
            if(!isReady) return;

            // 現実世界の平均的な明るさを取得
            var lightEst = obj.lightEstimation;
            brightness = lightEst.averageBrightness;
            print($"brightness:{brightness}");

            // 取得できているかのチェック
            if(brightness.HasValue) {
                directionLight.intensity = brightness.Value;
            }

            // 現実世界の色についても同様に処理
            colorTemperature = lightEst.averageColorTemperature;
            print($"colorTemperature:{colorTemperature}");

            if(colorTemperature.HasValue) {
                directionLight.colorTemperature = colorTemperature.Value;
            }

            // 色の種類についても同様に処理
            colorCorrection = lightEst.colorCorrection;
            print($"colorCorrection:{colorCorrection}");

            if(colorCorrection.HasValue) {
                directionLight.color = colorCorrection.Value;
            }

            // 光源の方向についても同様
            mainLightDirection = lightEst.mainLightDirection;
            print($"mainLightDirection:{mainLightDirection}");

            if(mainLightDirection.HasValue) {
                // 仮想光源の向きを調整する
                directionLight.transform.rotation = Quaternion.LookRotation(mainLightDirection.Value);
                // 光源の向きのオブジェクトに対しても同じ処理
                lightDirectionArrow.transform.rotation = directionLight.transform.rotation;
                // 方向の矢印オブジェクトを表示させる
                lightDirectionArrow.SetActive(true);
            } else {
                lightDirectionArrow.SetActive(false);
            }

            mainLightColor = lightEst.mainLightColor;
            print($"mainLightColor:{mainLightColor}");
            if(mainLightColor.HasValue) {
                directionLight.color = mainLightColor.Value;
            }

            averageMainLightBrightness = lightEst.averageMainLightBrightness;
            print($"averageMainLightBrightness:{averageMainLightBrightness}");

            if(averageMainLightBrightness.HasValue) {
                directionLight.intensity = averageMainLightBrightness.Value;
            }

            sphericalHarmonics = lightEst.ambientSphericalHarmonics;
            print($"sphericalHarmonics:{sphericalHarmonics}");

            if(sphericalHarmonics.HasValue) {
                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.ambientProbe = sphericalHarmonics.Value;
            }
        }
    
        // 光源についてのデータが取得できていれば○、そうでなければxを返す
        string setTextForUI<T>(string label,T? data) where T:struct {
            string result = data.HasValue? "○" : "x";
            return $"{label}:{result}";
        }

        // 各種サポート状況のテキストを表示する
        void Update()
        {
            if(!isReady) return;

            ShowMessage("光源推定");
            
            AddMessage(setTextForUI("環境光の明るさ",brightness));
            AddMessage(setTextForUI("環境光の色温度",colorTemperature));
            AddMessage(setTextForUI("環境光の色補正係数",colorCorrection));
            AddMessage(setTextForUI("主光源の向き",mainLightDirection));
            AddMessage(setTextForUI("主光源の色",mainLightColor));
            AddMessage(setTextForUI("主光源の明るさ",averageMainLightBrightness));
            AddMessage(setTextForUI("環境光の環境調和係数",sphericalHarmonics));
        }
    }
}
