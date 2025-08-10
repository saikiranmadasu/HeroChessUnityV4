using System.Linq;
using System.Collections.Generic;
public enum GameStatus { Ongoing, CheckmateWhite, CheckmateBlack, Stalemate, Draw50 }
public static class Rules {
  static readonly (int df,int dr)[] kingDirs = new[]{(1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)};
  static readonly (int df,int dr)[] knightDirs = new[]{(1,2),(2,1),(-1,2),(-2,1),(1,-2),(2,-1),(-1,-2),(-2,-1)};

  public static bool InCheck(Board b, PieceColor side){
    int kingSq=-1;
    for(int i=0;i<64;i++) if (b.sq[i].type==PieceType.King && b.sq[i].color==side) { kingSq=i; break; }
    if (kingSq==-1) return true;
    return IsSquareAttacked(b, kingSq, Opp(side));
  }

  public static bool IsSquareAttacked(Board b, int sq, PieceColor bySide){
    int f = Util.File(sq), r = Util.Rank(sq);
    int dir = (bySide==PieceColor.White)?1:-1;
    foreach(int df in new[]{-1,1}){
      int nf=f+df, nr=r+dir; if (!Util.OnBoard(nf,nr)) continue;
      int t=Util.Id(nf,nr);
      var p=b.sq[t];
      if (p.type==PieceType.Pawn && p.color==bySide) return true;
    }
    foreach(var (df,dr) in knightDirs){
      int nf=f+df, nr=r+dr; if (!Util.OnBoard(nf,nr)) continue;
      int t=Util.Id(nf,nr); var p=b.sq[t];
      if (p.type==PieceType.Knight && p.color==bySide) return true;
    }
    foreach(var (df,dr) in new[]{(1,1),(1,-1),(-1,1),(-1,-1)}){
      int nf=f+df, nr=r+dr;
      while(Util.OnBoard(nf,nr)){
        int t=Util.Id(nf,nr); var p=b.sq[t];
        if (p.type!=PieceType.None){
          if (p.color==bySide && (p.type==PieceType.Bishop || p.type==PieceType.Queen)) return true;
          break;
        }
        nf+=df; nr+=dr;
      }
    }
    foreach(var (df,dr) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){
      int nf=f+df, nr=r+dr;
      while(Util.OnBoard(nf,nr)){
        int t=Util.Id(nf,nr); var p=b.sq[t];
        if (p.type!=PieceType.None){
          if (p.color==bySide && (p.type==PieceType.Rook || p.type==PieceType.Queen)) return true;
          break;
        }
        nf+=df; nr+=dr;
      }
    }
    foreach(var (df,dr) in kingDirs){
      int nf=f+df, nr=r+dr; if (!Util.OnBoard(nf,nr)) continue;
      int t=Util.Id(nf,nr); var p=b.sq[t];
      if (p.type==PieceType.King && p.color==bySide) return true;
    }
    return false;
  }

  public static IEnumerable<Move> GenerateLegalMoves(Board b){
    foreach(var m in b.GenerateMoves()){
      var copy = Clone(b);
      copy.Make(m);
      if (!InCheck(copy, Opp(copy.sideToMove)))
        yield return m;
    }
  }

  public static GameStatus GetStatus(Board b){
    if (b.halfmoveClock >= 100) return GameStatus.Draw50;
    var legal = GenerateLegalMoves(b).Any();
    bool inCheck = InCheck(b, b.sideToMove);
    if (!legal && inCheck) return (b.sideToMove==PieceColor.White) ? GameStatus.CheckmateWhite : GameStatus.CheckmateBlack;
    if (!legal && !inCheck) return GameStatus.Stalemate;
    return GameStatus.Ongoing;
  }

  public static PieceColor Opp(PieceColor c) => (c==PieceColor.White)?PieceColor.Black:PieceColor.White;

  static Board Clone(Board src){
    var n=new Board();
    System.Array.Copy(src.sq,n.sq,64);
    n.sideToMove=src.sideToMove; n.enPassant=src.enPassant;
    n.cwk=src.cwk; n.cwq=src.cwq; n.cbk=src.cbk; n.cbq=src.cbq;
    n.halfmoveClock=src.halfmoveClock; n.fullmoveNumber=src.fullmoveNumber;
    return n;
  }
}
