using UnityEngine;
[CreateAssetMenu(menuName="Hero Chess/Theme")]
public class HeroTheme : ScriptableObject {
  public Sprite boardLight;
  public Sprite boardDark;
  public Sprite[] whitePieceSprites; // P,N,B,R,Q,K
  public Sprite[] blackPieceSprites;
  public AudioClip moveSfx, captureSfx, checkSfx, winSfx;
}
