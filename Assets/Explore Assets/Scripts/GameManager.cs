using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    int pigsRemaining;
    bool gameWon;

    void Awake()
    {
        Instance = this;

        pigsRemaining = FindObjectsByType<Pig>(FindObjectsSortMode.None).Length;
        Debug.Log($"[GameManager] Level started with {pigsRemaining} pig(s).");
    }

    public void OnPigDied()
    {
        if (gameWon) return;

        pigsRemaining--;
        Debug.Log($"[GameManager] Pig died, {pigsRemaining} remaining.");

        if (pigsRemaining <= 0)
            WinGame();
    }

    void WinGame()
    {
        gameWon = true;
        Debug.Log("[GameManager] ALL PIGS DEFEATED - YOU WIN!");

    }
}