using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityAR {
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ExImageTracking : MonoBehaviour
    {
        [SerializeField] Text message;
        [SerializeField] List<GameObject> placementPrefabs;
        [SerializeField] ARTrackedImageManager imageManager;
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

            // コンポーネントの定義
            imageManager = GetComponent<ARTrackedImageManager>();

            // 初期化チェック
            if(
                imageManager == null
                || imageManager.referenceLibrary == null
                || imageManager.referenceLibrary.count != placementPrefabs.Count
            ) {
                isReady = false;
                ShowMessage("エラー：SerializedFieldなど初期不備");
            }
            else {
                isReady = true;
            }
        }

        Dictionary<string,GameObject> correspondingChartForMarkersAndPrefabs = new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> instantiatedObjects = new Dictionary<string, GameObject>();

        void OnEnable()
        {
            // 処理できないなら何もしない
            if(!isReady) return;

            // マーカーのリスト初期化
            var markers = new List<string>();

            // 読み取る画像の数だけループ
            for(var i = 0; i < imageManager.referenceLibrary.count; i++) {
                // マーカーのリストに画像名を追加していく
                markers.Add(imageManager.referenceLibrary[i].name);

                // リストをソート
                markers.Sort();
            }

            // 設置するプレファブの分だけループ
            for(var i = 0; i < placementPrefabs.Count; i++) {
                correspondingChartForMarkersAndPrefabs.Add(markers[i],placementPrefabs[i]);
                instantiatedObjects.Add(markers[i],null);
            }

            imageManager.trackedImagePrefab = null;
            imageManager.trackedImagesChanged += OnTrackedImageChanged;

            ShowMessage("ARマーカーとプレファブの対応");

            foreach(var data in correspondingChartForMarkersAndPrefabs) {
                AddMessage($"{data.Key}:{data.Value}");
            }

            AddMessage("ARマーカーと対応するプレハブの確認後、マーカーを撮影してください");
        }

        void OnDisable()
        {
            if(!isReady) return;

            // イベントを取り除く
            imageManager.trackedImagesChanged -= OnTrackedImageChanged;
        }

        private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs obj)
        {
            ShowMessage("イメージ検出");

            foreach(var trackedImage in obj.added) {
                // 画像の名前を取得
                var imageName = trackedImage.referenceImage.name;

                // correspondingリストに存在したら、
                if(correspondingChartForMarkersAndPrefabs.TryGetValue(imageName,out var prefab)) {
                    // 表示するときのスケール
                    var scale = 0.2f;

                    trackedImage.transform.localScale = Vector3.one *scale; 

                    // 初期化
                    instantiatedObjects[imageName] = Instantiate(prefab,trackedImage.transform);
                }
            }

            foreach(var trackedImage in obj.updated) {
                // 画像の名前を取得
                var imageName = trackedImage.referenceImage.name;

                // すでにinstantiateのリストに存在するはずなので
                if(instantiatedObjects.TryGetValue(imageName,out var instantiatedObject)) {
                    if(trackedImage.trackingState != TrackingState.None) {
                        instantiatedObject.SetActive(true);
                    } else {
                        instantiatedObject.SetActive(false);
                    }
                    AddMessage($"{imageName}:{trackedImage.trackingState}");
                }
            }
        }
    }
}
