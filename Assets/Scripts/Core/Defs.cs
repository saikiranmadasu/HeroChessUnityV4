using System;
public enum PieceType { None, Pawn, Knight, Bishop, Rook, Queen, King }
[Flags] public enum PieceColor { None=0, White=1, Black=2 }
public struct Piece { public PieceType type; public PieceColor color; public Piece(PieceType t, PieceColor c){ type=t;color=c; } }
public struct Move { public int from, to; public PieceType promo; public bool isCapture; public Move(int f,int t,PieceType p=PieceType.None,bool c=false){from=f;to=t;promo=p;isCapture=c;} }
public static class Util { public static int File(int sq)=>sq&7; public static int Rank(int sq)=>sq>>3; public static bool OnBoard(int f,int r)=>f>=0&&f<8&&r>=0&&r<8; public static int Id(int f,int r)=>r*8+f; }
