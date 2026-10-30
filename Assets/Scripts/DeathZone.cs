using UnityEngine;
public class DeathZone : MonoBehaviour
{
    public void KillPlayer(Char9Movement player)
    {
        if (player != null) player.Die();
    }
}
