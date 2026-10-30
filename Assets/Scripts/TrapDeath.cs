using UnityEngine;
public class TrapDeath : MonoBehaviour
{
    public void KillPlayer(Char9Movement player)
    {
        if (player != null) player.Die();
    }
    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.collider.GetComponentInParent<Char9Movement>();
        if (player != null) player.Die();
    }
}
