using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Splash : MonoBehaviour {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void Run(){
    var go = new GameObject("Splash"); go.AddComponent<Splash>();
  }

  IEnumerator Start(){
    var canvasGO = new GameObject("SplashCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
    var canvas = canvasGO.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    var scaler = canvasGO.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080,1920);

    var bg = new GameObject("BG", typeof(Image)); bg.transform.SetParent(canvasGO.transform,false);
    var bgI = bg.GetComponent<Image>(); bgI.color = new Color(0.05f,0.06f,0.09f,1);
    var bgRT = bg.GetComponent<RectTransform>(); bgRT.anchorMin=Vector2.zero; bgRT.anchorMax=Vector2.one; bgRT.offsetMin=bgRT.offsetMax=Vector2.zero;

    var logo = new GameObject("Logo", typeof(Image)); logo.transform.SetParent(canvasGO.transform,false);
    var li = logo.GetComponent<Image>(); li.sprite = Resources.Load<Sprite>("splash_logo");
    var lrt = logo.GetComponent<RectTransform>(); lrt.sizeDelta = new Vector2(600,600); lrt.anchorMin=lrt.anchorMax=new Vector2(0.5f,0.5f); lrt.anchoredPosition=Vector2.zero;

    float timer=0f;
    while(!Input.GetMouseButtonDown(0) && timer<1.5f){ timer += Time.deltaTime; yield return null; }

    Destroy(canvasGO);
    var go = new GameObject("Bootstrap"); go.AddComponent<Bootstrap>();
  }
}
