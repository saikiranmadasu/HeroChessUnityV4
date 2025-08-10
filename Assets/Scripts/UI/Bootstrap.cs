using UnityEngine;
using UnityEngine.UI;

public class Bootstrap : MonoBehaviour {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Run(){
    var go = new GameObject("Bootstrap"); go.AddComponent<Bootstrap>();
  }

  void Start(){
    var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
    var canvas = canvasGO.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    var scaler = canvasGO.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080,1920);

    var audioGO = new GameObject("Audio"); audioGO.transform.SetParent(canvasGO.transform,false);
    var audioSource = audioGO.AddComponent<AudioSource>();

    var theme = ScriptableObject.CreateInstance<HeroTheme>();
    theme.boardLight = Resources.Load<Sprite>("board_light");
    theme.boardDark  = Resources.Load<Sprite>("board_dark");
    theme.whitePieceSprites = new Sprite[]{Resources.Load<Sprite>("w_p"),Resources.Load<Sprite>("w_n"),Resources.Load<Sprite>("w_b"),Resources.Load<Sprite>("w_r"),Resources.Load<Sprite>("w_q"),Resources.Load<Sprite>("w_k")};
    theme.blackPieceSprites = new Sprite[]{Resources.Load<Sprite>("b_p"),Resources.Load<Sprite>("b_n"),Resources.Load<Sprite>("b_b"),Resources.Load<Sprite>("b_r"),Resources.Load<Sprite>("b_q"),Resources.Load<Sprite>("b_k")};
    theme.moveSfx = Resources.Load<AudioClip>("Audio/move");
    theme.captureSfx = Resources.Load<AudioClip>("Audio/capture");
    theme.checkSfx = Resources.Load<AudioClip>("Audio/check");
    theme.winSfx = Resources.Load<AudioClip>("Audio/win");

    var topBar = new GameObject("TopBar", typeof(Image));
    topBar.transform.SetParent(canvasGO.transform,false);
    var topImg = topBar.GetComponent<Image>(); topImg.color = new Color(0.08f,0.09f,0.12f,0.95f);
    var topRT = topBar.GetComponent<RectTransform>(); topRT.anchorMin=new Vector2(0,1); topRT.anchorMax=new Vector2(1,1); topRT.pivot=new Vector2(0.5f,1); topRT.sizeDelta=new Vector2(0,120);

    Button MakeBtn(string label, float x){
      var go = new GameObject(label, typeof(Image), typeof(Button));
      go.transform.SetParent(topBar.transform,false);
      var img = go.GetComponent<Image>(); img.color = new Color(0.18f,0.2f,0.25f,1);
      var rt = go.GetComponent<RectTransform>(); rt.sizeDelta=new Vector2(220,84); rt.anchoredPosition = new Vector2(x,-60);
      var txtGO = new GameObject("Text", typeof(Text)); txtGO.transform.SetParent(go.transform,false);
      var t = txtGO.GetComponent<Text>(); t.text = label; t.alignment=TextAnchor.MiddleCenter; t.color=Color.white; t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
      var trt = t.GetComponent<RectTransform>(); trt.anchorMin=Vector2.zero; trt.anchorMax=Vector2.one; trt.offsetMin=trt.offsetMax=Vector2.zero;
      return go.GetComponent<Button>();
    }

    var newBtn = MakeBtn("New", 140);
    var undoBtn = MakeBtn("Undo", 380);

    var gridGO = new GameObject("BoardGrid", typeof(GridLayoutGroup));
    gridGO.transform.SetParent(canvasGO.transform, false);
    var grid = gridGO.GetComponent<GridLayoutGroup>();
    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    grid.constraintCount = 8;
    grid.cellSize = new Vector2(120,120);
    grid.spacing = new Vector2(2,2);
    var rt = gridGO.GetComponent<RectTransform>();
    rt.sizeDelta = new Vector2(120*8+2*7, 120*8+2*7);
    rt.anchoredPosition = new Vector2(0,-60);
    rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
    rt.pivot = new Vector2(0.5f,0.5f);

    var movesPanel = new GameObject("MovesPanel", typeof(Image));
    movesPanel.transform.SetParent(canvasGO.transform,false);
    var mpRT = movesPanel.GetComponent<RectTransform>(); mpRT.anchorMin=new Vector2(0,0); mpRT.anchorMax=new Vector2(1,0); mpRT.pivot=new Vector2(0.5f,0); mpRT.sizeDelta=new Vector2(0,320);
    var mpImg = movesPanel.GetComponent<Image>(); mpImg.color = new Color(0.08f,0.09f,0.12f,0.95f);
    var movesTextGO = new GameObject("MovesText", typeof(Text));
    movesTextGO.transform.SetParent(movesPanel.transform,false);
    var movesText = movesTextGO.GetComponent<Text>(); movesText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); movesText.alignment=TextAnchor.UpperLeft; movesText.color = new Color(0.9f,0.95f,1f,1); movesText.text = "";
    var mtRT = movesText.GetComponent<RectTransform>(); mtRT.anchorMin=new Vector2(0,0); mtRT.anchorMax=new Vector2(1,1); mtRT.offsetMin=new Vector2(20,20); mtRT.offsetMax=new Vector2(-20,-20);

    var endPanel = new GameObject("EndPanel", typeof(Image));
    endPanel.transform.SetParent(canvasGO.transform,false);
    var epI = endPanel.GetComponent<Image>(); epI.color = new Color(0,0,0,0.75f);
    var epRT = endPanel.GetComponent<RectTransform>(); epRT.anchorMin=Vector2.zero; epRT.anchorMax=Vector2.one; epRT.offsetMin=epRT.offsetMax=Vector2.zero;
    endPanel.SetActive(false);
    var endTextGO = new GameObject("EndText", typeof(Text)); endTextGO.transform.SetParent(endPanel.transform,false);
    var endText = endTextGO.GetComponent<Text>(); endText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); endText.alignment=TextAnchor.MiddleCenter; endText.color=Color.white; endText.fontSize=64; endText.text="Game Over";
    var etRT = endText.GetComponent<RectTransform>(); etRT.anchorMin=Vector2.zero; etRT.anchorMax=Vector2.one; etRT.offsetMin=new Vector2(0,0); etRT.offsetMax=new Vector2(0,0);

    var viewGO = new GameObject("BoardView"); viewGO.transform.SetParent(canvasGO.transform,false);
    var view = viewGO.AddComponent<BoardView>();
    view.Init(theme, grid, audioSource, movesText, undoBtn, null, null, endPanel, endText);
    newBtn.onClick.AddListener(view.NewGame);
  }
}
