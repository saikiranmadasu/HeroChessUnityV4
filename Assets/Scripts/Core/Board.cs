using System.Collections.Generic;
using static Util;

public class Board {
  public Piece[] sq = new Piece[64];
  public PieceColor sideToMove = PieceColor.White;
  public int halfmoveClock, fullmoveNumber = 1;
  public int enPassant = -1;
  public bool cwk=true,cwq=true,cbk=true,cbq=true;

  public void LoadStart() {
    sq = new Piece[64];
    PieceColor W=PieceColor.White,B=PieceColor.Black;
    PieceType[] back = { PieceType.Rook,PieceType.Knight,PieceType.Bishop,PieceType.Queen,PieceType.King,PieceType.Bishop,PieceType.Knight,PieceType.Rook };
    for(int f=0;f<8;f++){
      sq[Id(f,0)] = new Piece(back[f], W); sq[Id(f,1)] = new Piece(PieceType.Pawn, W);
      sq[Id(f,6)] = new Piece(PieceType.Pawn, B); sq[Id(f,7)] = new Piece(back[f], B);
    }
    sideToMove = PieceColor.White; enPassant=-1; cwk=cwq=cbk=cbq=true; halfmoveClock=0; fullmoveNumber=1;
  }

  public IEnumerable<Move> GenerateMoves() {
    var color = sideToMove;
    for (int s=0;s<64;s++){
      var p = sq[s]; if (p.color != color) continue;
      switch(p.type){
        case PieceType.Pawn: foreach(var m in PawnMoves(s,p)) yield return m; break;
        case PieceType.Knight: foreach(var m in JumpMoves(s,p,new[]{(1,2),(2,1),(-1,2),(-2,1),(1,-2),(2,-1),(-1,-2),(-2,-1)})) yield return m; break;
        case PieceType.Bishop: foreach(var m in RayMoves(s,p,new[]{(1,1),(1,-1),(-1,1),(-1,-1)})) yield return m; break;
        case PieceType.Rook: foreach(var m in RayMoves(s,p,new[]{(1,0),(-1,0),(0,1),(0,-1)})) yield return m; break;
        case PieceType.Queen: foreach(var m in RayMoves(s,p,new[]{(1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)})) yield return m; break;
        case PieceType.King: foreach(var m in KingMoves(s,p)) yield return m; break;
      }
    }
  }

  IEnumerable<Move> PawnMoves(int s, Piece p){
    int dir = p.color==PieceColor.White ? 1 : -1;
    int r = Rank(s), f = File(s);
    int fwdR = r + dir;
    if (OnBoard(f,fwdR) && sq[Id(f,fwdR)].type==PieceType.None) {
      if ((p.color==PieceColor.White && fwdR==7) || (p.color==PieceColor.Black && fwdR==0))
        foreach (var promo in new[]{PieceType.Queen,PieceType.Rook,PieceType.Bishop,PieceType.Knight})
          yield return new Move(s, Id(f,fwdR), promo);
      else yield return new Move(s, Id(f,fwdR));
      if ((p.color==PieceColor.White && r==1) || (p.color==PieceColor.Black && r==6)){
        int twoR = r + 2*dir;
        if (sq[Id(f,twoR)].type==PieceType.None) yield return new Move(s, Id(f,twoR));
      }
    }
    foreach (int df in new[]{-1,1}){
      int cf = f+df; int cr = r+dir;
      if (!OnBoard(cf,cr)) continue;
      int t = Id(cf,cr);
      if (sq[t].type!=PieceType.None && sq[t].color!=p.color){
        bool promo = (p.color==PieceColor.White && cr==7) || (p.color==PieceColor.Black && cr==0);
        if (promo) foreach(var pr in new[]{PieceType.Queen,PieceType.Rook,PieceType.Bishop,PieceType.Knight}) yield return new Move(s,t,pr,true);
        else yield return new Move(s,t,PieceType.None,true);
      } else if (t==enPassant) yield return new Move(s,t,PieceType.None,true);
    }
  }

  IEnumerable<Move> JumpMoves(int s, Piece p, (int df,int dr)[] deltas){
    int f=File(s), r=Rank(s);
    foreach(var (df,dr) in deltas){
      int nf=f+df, nr=r+dr; if (!OnBoard(nf,nr)) continue;
      int t=Id(nf,nr); var q = sq[t];
      if (q.color==p.color) continue;
      yield return new Move(s,t,PieceType.None,q.type!=PieceType.None);
    }
  }

  IEnumerable<Move> RayMoves(int s, Piece p, (int df,int dr)[] rays){
    int f=File(s), r=Rank(s);
    foreach(var (df,dr) in rays){
      int nf=f+df, nr=r+dr;
      while(OnBoard(nf,nr)){
        int t=Id(nf,nr); var q = sq[t];
        if (q.type==PieceType.None) { yield return new Move(s,t); }
        else { if (q.color!=p.color) yield return new Move(s,t,PieceType.None,true); break; }
        nf+=df; nr+=dr;
      }
    }
  }

  IEnumerable<Move> KingMoves(int s, Piece p){
    foreach(var m in JumpMoves(s,p,new[]{(1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)})) yield return m;
    if (p.color==PieceColor.White && Rank(s)==0 && File(s)==4){
      if (cwk && sq[5].type==PieceType.None && sq[6].type==PieceType.None) yield return new Move(4,6);
      if (cwq && sq[3].type==PieceType.None && sq[2].type==PieceType.None && sq[1].type==PieceType.None) yield return new Move(4,2);
    }
    if (p.color==PieceColor.Black && Rank(s)==7 && File(s)==4){
      if (cbk && sq[61].type==PieceType.None && sq[62].type==PieceType.None) yield return new Move(60,62);
      if (cbq && sq[59].type==PieceType.None && sq[58].type==PieceType.None && sq[57].type==PieceType.None) yield return new Move(60,58);
    }
  }

  public void Make(Move m){
    var piece = sq[m.from];
    if (piece.type==PieceType.Pawn && m.to==enPassant){
      int capSq = piece.color==PieceColor.White ? m.to-8 : m.to+8;
      sq[capSq] = new Piece(PieceType.None, PieceColor.None);
    }
    bool capture = sq[m.to].type!=PieceType.None;
    sq[m.to] = new Piece(m.promo!=PieceType.None ? m.promo : piece.type, piece.color);
    sq[m.from] = new Piece(PieceType.None, PieceColor.None);
    if (piece.type==PieceType.King){
      if (piece.color==PieceColor.White){
        cwk=cwq=false;
        if (m.from==4 && m.to==6){ sq[5]=new Piece(PieceType.Rook,PieceColor.White); sq[7]=new Piece(PieceType.None,PieceColor.None); }
        if (m.from==4 && m.to==2){ sq[3]=new Piece(PieceType.Rook,PieceColor.White); sq[0]=new Piece(PieceType.None,PieceColor.None); }
      } else {
        cbk=cbq=false;
        if (m.from==60 && m.to==62){ sq[61]=new Piece(PieceType.Rook,PieceColor.Black); sq[63]=new Piece(PieceType.None,PieceColor.None); }
        if (m.from==60 && m.to==58){ sq[59]=new Piece(PieceType.Rook,PieceColor.Black); sq[56]=new Piece(PieceType.None,PieceColor.None); }
      }
      enPassant = -1;
    }
    if (piece.type==PieceType.Pawn && System.Math.Abs(m.to - m.from)==16)
      enPassant = (m.from + m.to)/2;
    else enPassant=-1;

    halfmoveClock = (piece.type==PieceType.Pawn || capture) ? 0 : halfmoveClock+1;
    if (sideToMove==PieceColor.Black) fullmoveNumber++;
    sideToMove = (sideToMove==PieceColor.White) ? PieceColor.Black : PieceColor.White;
  }
}
