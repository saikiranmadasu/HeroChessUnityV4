using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class BoardView : MonoBehaviour { public void ForceAIMove(){ var m = Engine.BestMove(board, PlayerPrefs.GetInt("aiDepth",2)); PlayMove(m); CheckGameOver(); }
  public GridLayoutGroup grid;
  public Image[] tiles = new Image[64];
  public Image[] pieceImgs = new Image[64];
  public HeroTheme theme;
  public AudioSource audioSource;
  public Text moveListText;
  public Button undoBtn;
  public Dropdown aiDepthDropdown;
  public Dropdown themeDropdown;
  public GameObject endPanel;
  public Text endText;

  Board board;
  int selected=-1;
  Stack<Board> history = new Stack<Board>();

  public void Init(HeroTheme themeAsset, GridLayoutGroup gridGLG, AudioSource src, Text moveText, Button undo, Dropdown depthDD, Dropdown themeDD, GameObject endPanelGO, Text endLabel){
    theme = themeAsset;
    grid = gridGLG;
    audioSource = src;
    moveListText = moveText;
    undoBtn = undo;
    aiDepthDropdown = depthDD;
    themeDropdown = themeDD;
    endPanel = endPanelGO;
    endText = endLabel;
    board = new Board(); board.LoadStart();
    history.Clear();
    BuildGrid();
    Redraw();
    undoBtn.onClick.AddListener(UndoMove);
    endPanel.SetActive(false);
    if (aiDepthDropdown!=null){
      aiDepthDropdown.ClearOptions();
      aiDepthDropdown.AddOptions(new List<string>{ "Easy (1)", "Medium (2)", "Hard (3)"});
      int d = PlayerPrefs.GetInt("aiDepth",2);
      aiDepthDropdown.value = Mathf.Clamp(d-1,0,2);
      aiDepthDropdown.onValueChanged.AddListener(i=>PlayerPrefs.SetInt("aiDepth", i+1));
    }
  }

  void BuildGrid(){
    for(int i=0;i<64;i++){
      var tileGO = new GameObject("Tile_"+i, typeof(Image), typeof(Button));
      tileGO.transform.SetParent(grid.transform, false);
      var img = tileGO.GetComponent<Image>();
      img.sprite = ((i + i/8)%2==0) ? theme.boardLight : theme.boardDark;
      tiles[i]=img;
      int idx=i;
      tileGO.GetComponent<Button>().onClick.AddListener(()=>OnTileTap(idx));

      var pieceGO = new GameObject("Piece_"+i, typeof(Image));
      pieceGO.transform.SetParent(tileGO.transform, false);
      var pimg = pieceGO.GetComponent<Image>();
      pieceImgs[i]=pimg;
      var rt = pimg.rectTransform; rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero;
    }
  }

  public void NewGame(){
    board = new Board(); board.LoadStart();
    history.Clear(); selected=-1;
    moveListText.text = "";
    endPanel.SetActive(false);
    Redraw();
  }

  public void OnTileTap(int sqIndex){
    if (endPanel.activeSelf) return;
    if (selected==-1){
      if (board.sq[sqIndex].color==board.sideToMove) { selected = sqIndex; }
    } else {
      var legal = Rules.GenerateLegalMoves(board).Where(m=>m.from==selected && m.to==sqIndex).ToList();
      if (legal.Count>0){
        PlayMove(legal[0]);
        if (CheckGameOver()) return;
        PlayTurnAIIfEnabled();
      }
      selected=-1;
      Redraw();
    }
  }

  void PlayMove(Move mv){
    PushHistory();
    bool capture = (board.sq[mv.to].type != PieceType.None) || (board.sq[mv.from].type==PieceType.Pawn && mv.to==board.enPassant);
    board.Make(mv);
    LogMove(mv);
    PlaySfx(capture ? theme.captureSfx : theme.moveSfx);
    Redraw();
    if (Rules.InCheck(board, board.sideToMove)) PlaySfx(theme.checkSfx);
  }

  bool CheckGameOver(){
    var status = Rules.GetStatus(board);
    if (status==GameStatus.Ongoing) return false;
    endPanel.SetActive(true);
    switch(status){
      case GameStatus.CheckmateWhite: endText.text="Checkmate! Black wins"; break;
      case GameStatus.CheckmateBlack: endText.text="Checkmate! White wins"; break;
      case GameStatus.Stalemate: endText.text="Stalemate!"; break;
      case GameStatus.Draw50: endText.text="Draw by 50-move rule"; break;
    }
    PlaySfx(theme.winSfx);
    return true;
  }

  void PlaySfx(AudioClip clip){ if (audioSource!=null && clip!=null) audioSource.PlayOneShot(clip); }

  void Redraw(){
    for(int i=0;i<64;i++){
      var p = board.sq[i];
      var img = pieceImgs[i];
      if (p.type==PieceType.None){ img.sprite=null; img.color=new Color(1,1,1,0); continue; }
      int idx = (int)p.type - 1;
      img.color = Color.white;
      img.sprite = (p.color==PieceColor.White) ? theme.whitePieceSprites[idx] : theme.blackPieceSprites[idx];
    }
  }

  void LogMove(Move m){
    string sqName(int s){ int f=s&7; int r=s>>3; return "" + (char)('a'+f) + (1+r).ToString(); }
    string pieceChar(PieceType t){ return t==PieceType.Pawn? "" : t.ToString()[0].ToString(); }
    var piece = board.sq[m.to];
    string entry = pieceChar(piece.type) + sqName(m.from) + (m.isCapture? "x":"-") + sqName(m.to);
    if (m.promo!=PieceType.None) entry += "="+m.promo.ToString()[0];
    moveListText.text += entry + "\n";
  }

  void PushHistory(){
    var n=new Board();
    System.Array.Copy(board.sq,n.sq,64);
    n.sideToMove=board.sideToMove; n.enPassant=board.enPassant;
    n.cwk=board.cwk; n.cwq=board.cwq; n.cbk=board.cbk; n.cbq=board.cbq;
    n.halfmoveClock=board.halfmoveClock; n.fullmoveNumber=board.fullmoveNumber;
    history.Push(n);
  }

  public void UndoMove(){
    if (history.Count>0){
      board = history.Pop();
      Redraw();
      var lines = moveListText.text.Split('\n').ToList();
      if (lines.Count>1){ lines.RemoveAt(lines.Count-2); moveListText.text = string.Join("\n", lines); }
      endPanel.SetActive(false);
    }
  }

  async void PlayTurnAIIfEnabled(){
    await System.Threading.Tasks.Task.Yield();
    int depth = PlayerPrefs.GetInt("aiDepth",2);
    var move = Engine.BestMove(board, depth);
    PlayMove(move);
    CheckGameOver();
  }
}
