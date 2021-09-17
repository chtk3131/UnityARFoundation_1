using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityAR {

    [RequireComponent(typeof(ExMakeAppearOnPlane))]

    public class ExScaleAndRotation : MonoBehaviour
    {
        [SerializeField] Text message;
        [SerializeField] Text scaleText;
        [SerializeField] Slider scaleSlider;
        [SerializeField] Text rotationText;
        [SerializeField] Slider rotationSlider;

        bool isReady;


        void Awake()
        {
            // 初期化
            if(
                message == null
                || scaleText == null
                || scaleSlider == null
                || rotationSlider == null
                || rotationText == null
            ) {
                isReady = false;
            } else {
                isReady = true;
            }
        }

        void ShowMessage(string text) {
            message.text = $"{text}\r\n";
        }

        void AddMessage(string text) {
            message.text += $"{text}\r\n";
        }

        void ShowScale(float scale) {
            scaleText.text = $"倍率：{scale:F1}";
        }

        void ShowRotation(float angle) {
            rotationText.text = $"回転角：{angle:F0}度";
        }

        ExMakeAppearOnPlane makeAppearOnPlane;
        const float minScale = 0.2f;
        const float maxScale = 2f;
        const float minRotation = 0f;
        const float maxRotation = 360f;

        void Start()
        {
            // コンポーネントの初期化
            makeAppearOnPlane = GetComponent<ExMakeAppearOnPlane>();

            // 初期化チェック
            if(
                !isReady
                || makeAppearOnPlane == null
                || !makeAppearOnPlane.IsAvailable
            ) {
                isReady = false;
                ShowMessage("エラー：SerializeFieldなどの初期化不備");
            }
            
            isReady = true;
            ShowMessage("スケールと回転");
            AddMessage("床を撮影してください。しばらくすると床が認識されます。");
            AddMessage("平面をタップすると椅子が表示されます。");

            var initScale = 1f;
            ShowScale(initScale);

            // ％表示
            scaleSlider.value = (initScale - minScale) / (maxScale - minScale);
            // スケールスライダを変更したときのイベントを追加
            scaleSlider.onValueChanged.AddListener(OnScaleSliderValueChanged);

            var initRotation = Quaternion.identity;
            ShowRotation(initRotation.eulerAngles.y);
            rotationSlider.value = (initRotation.eulerAngles.y - minRotation) / (maxRotation - minRotation);
            rotationSlider.onValueChanged.AddListener(OnRotationSliderValueChanged);
        }

        private void OnRotationSliderValueChanged(float value)
        {
            if(!isReady) return;

            var rotY = value * (maxRotation - minRotation) + minRotation;
            ShowRotation(rotY);

            // 回転をオブジェクトに適用
            makeAppearOnPlane.Rotation = Quaternion.Euler(0f,rotY,0f);
        }

        private void OnScaleSliderValueChanged(float value)
        {
            if(!isReady) return;

            var scale = value * (maxScale - minScale) + minScale;
            ShowScale(scale);

            // スケールをオブジェクトに適用
            makeAppearOnPlane.Scale = scale;
        }
    }
}
