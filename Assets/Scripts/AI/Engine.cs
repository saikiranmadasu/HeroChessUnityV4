using System;
public static class Engine {
  static readonly int[] val = {0,100,320,330,500,900,20000};
  static int Eval(Board b){
    int score=0;
    for(int i=0;i<64;i++){
      var p=b.sq[i]; if (p.type==PieceType.None) continue;
      int v = val[(int)p.type] * (p.color==PieceColor.White ? 1 : -1);
      score += v;
    }
    return (b.sideToMove==PieceColor.White)?score:-score;
  }
  public static Move BestMove(Board b, int depth){
    int bestScore = int.MinValue; Move best = default;
    foreach(var m in Rules.GenerateLegalMoves(b)){
      var copy = Clone(b); copy.Make(m);
      int sc = -Search(copy, depth-1, int.MinValue+1, int.MaxValue-1);
      if (sc>bestScore){ bestScore=sc; best=m; }
    }
    return best;
  }
  static int Search(Board b, int d, int alpha, int beta){
    if (d==0) return Eval(b);
    foreach(var m in Rules.GenerateLegalMoves(b)){
      var c = Clone(b); c.Make(m);
      int sc = -Search(c, d-1, -beta, -alpha);
      if (sc>=beta) return beta;
      if (sc>alpha) alpha=sc;
    }
    return alpha;
  }
  static Board Clone(Board src){
    var n=new Board();
    Array.Copy(src.sq,n.sq,64);
    n.sideToMove=src.sideToMove; n.enPassant=src.enPassant;
    n.cwk=src.cwk; n.cwq=src.cwq; n.cbk=src.cbk; n.cbq=src.cbq;
    n.halfmoveClock=src.halfmoveClock; n.fullmoveNumber=src.fullmoveNumber;
    return n;
  }
}
