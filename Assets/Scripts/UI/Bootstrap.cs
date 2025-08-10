using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;   // << needed for EventSystem

public class Bootstrap : MonoBehaviour {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Run(){ new GameObject("Bootstrap").AddComponent<Bootstrap>(); }

  static Sprite S(string p){
    var t = Resources.Load<Texture2D>(p);
    if (!t) { Debug.LogWarning("Missing sprite: " + p); return null; }
    return Sprite.Create(t, new Rect(0,0,t.width,t.height), new Vector2(0.5f,0.5f), 100f);
  }

  public bool aiStartsAsWhite = false;

  void Start(){
    // 0) EventSystem so buttons/taps work
    if (!FindObjectOfType<EventSystem>())
      new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

    // 1) Canvas
    var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
    var canvas = canvasGO.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    var scaler = canvasGO.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080,1920);

    // 2) Audio
    var audioGO = new GameObject("Audio"); audioGO.transform.SetParent(canvasGO.transform,false);
    var audio = audioGO.AddComponent<AudioSource>();

    // 3) Theme (runtime sprites)
    var theme = ScriptableObject.CreateInstance<HeroTheme>();
    theme.boardLight = S("board_light");    // ok if null; BoardView will color squares
    theme.boardDark  = S("board_dark");
    theme.whitePieceSprites = new Sprite[]{ S("w_p"),S("w_n"),S("w_b"),S("w_r"),S("w_q"),S("w_k") };
    theme.blackPieceSprites = new Sprite[]{ S("b_p"),S("b_n"),S("b_b"),S("b_r"),S("b_q"),S("b_k") };
    theme.moveSfx   = Resources.Load<AudioClip>("Audio/move");
    theme.captureSfx= Resources.Load<AudioClip>("Audio/capture");
    theme.checkSfx  = Resources.Load<AudioClip>("Audio/check");
    theme.winSfx    = Resources.Load<AudioClip>("Audio/win");

    // 4) Top buttons
    var top = new GameObject("TopBar", typeof(Image)); top.transform.SetParent(canvasGO.transform,false);
    top.GetComponent<Image>().color = new Color(0.08f,0.09f,0.12f,0.95f);
    var topRT = top.GetComponent<RectTransform>(); topRT.anchorMin=new Vector2(0,1); topRT.anchorMax=new Vector2(1,1); topRT.pivot=new Vector2(0.5f,1); topRT.sizeDelta=new Vector2(0,120);

    Button Btn(string label, float x){
      var go = new GameObject(label, typeof(Image), typeof(Button)); go.transform.SetParent(top.transform,false);
      go.GetComponent<Image>().color = new Color(0.18f,0.2f,0.25f,1);
      var rt = go.GetComponent<RectTransform>(); rt.sizeDelta=new Vector2(220,84); rt.anchoredPosition=new Vector2(x,-60);
      var txt = new GameObject("Text", typeof(Text)); txt.transform.SetParent(go.transform,false);
      var t = txt.GetComponent<Text>(); t.text=label; t.alignment=TextAnchor.MiddleCenter; t.color=Color.white; t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
      var trt = t.GetComponent<RectTransform>(); trt.anchorMin=Vector2.zero; trt.anchorMax=Vector2.one; trt.offsetMin=trt.offsetMax=Vector2.zero;
      return go.GetComponent<Button>();
    }
    var newBtn  = Btn("New", 140);
    var undoBtn = Btn("Undo", 380);

    // 5) Board grid
    var gridGO = new GameObject("BoardGrid", typeof(GridLayoutGroup)); gridGO.transform.SetParent(canvasGO.transform,false);
    var grid = gridGO.GetComponent<GridLayoutGroup>(); grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount=8; grid.cellSize=new Vector2(120,120); grid.spacing=new Vector2(2,2);
    var grt = gridGO.GetComponent<RectTransform>(); grt.sizeDelta=new Vector2(120*8+2*7,120*8+2*7); grt.anchoredPosition=new Vector2(0,-60); grt.anchorMin=grt.anchorMax=new Vector2(0.5f,0.5f); grt.pivot=new Vector2(0.5f,0.5f);

    // 6) Moves panel
    var movesPanel = new GameObject("MovesPanel", typeof(Image)); movesPanel.transform.SetParent(canvasGO.transform,false);
    movesPanel.GetComponent<Image>().color = new Color(0.08f,0.09f,0.12f,0.95f);
    var mpRT = movesPanel.GetComponent<RectTransform>(); mpRT.anchorMin=new Vector2(0,0); mpRT.anchorMax=new Vector2(1,0); mpRT.pivot=new Vector2(0.5f,0); mpRT.sizeDelta=new Vector2(0,320);
    var movesTextGO = new GameObject("MovesText", typeof(Text)); movesTextGO.transform.SetParent(movesPanel.transform,false);
    var movesText = movesTextGO.GetComponent<Text>(); movesText.font=Resources.GetBuiltinResource<Font>("Arial.ttf"); movesText.alignment=TextAnchor.UpperLeft; movesText.color=new Color(0.9f,0.95f,1f,1);
    var mtRT = movesText.GetComponent<RectTransform>(); mtRT.anchorMin=new Vector2(0,0); mtRT.anchorMax=new Vector2(1,1); mtRT.offsetMin=new Vector2(20,20); mtRT.offsetMax=new Vector2(-20,-20);

    // 7) End panel
    var endPanel = new GameObject("EndPanel", typeof(Image)); endPanel.transform.SetParent(canvasGO.transform,false);
    endPanel.GetComponent<Image>().color = new Color(0,0,0,0.75f); var epRT = endPanel.GetComponent<RectTransform>(); epRT.anchorMin=Vector2.zero; epRT.anchorMax=Vector2.one; epRT.offsetMin=epRT.offsetMax=Vector2.zero; endPanel.SetActive(false);
    var endTextGO = new GameObject("EndText", typeof(Text)); endTextGO.transform.SetParent(endPanel.transform,false);
    var endText = endTextGO.GetComponent<Text>(); endText.font=Resources.GetBuiltinResource<Font>("Arial.ttf"); endText.alignment=TextAnchor.MiddleCenter; endText.color=Color.white; endText.fontSize=64; endText.text="Game Over";
    var etRT = endText.GetComponent<RectTransform>(); etRT.anchorMin=Vector2.zero; etRT.anchorMax=Vector2.one; etRT.offsetMin=Vector2.zero; etRT.offsetMax=Vector2.zero;

    // 8) BoardView hookup
    var view = new GameObject("BoardView").AddComponent<BoardView>();
    view.transform.SetParent(canvasGO.transform,false);
    view.Init(theme, grid, audio, movesText, undoBtn, null, null, endPanel, endText);

    newBtn.onClick.AddListener(view.NewGame);
    view.NewGame();                      // start immediately
  }
}
