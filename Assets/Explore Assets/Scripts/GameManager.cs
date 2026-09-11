using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scoring")]
    [SerializeField] private int pointsPerPig = 5000;
    [SerializeField] private int pointsPerBlock = 500;
    [SerializeField] private int pointsPerUnusedBird = 10000;

    [Header("Losing")]
    [Tooltip("Seconds after the last bird before we call it a loss.")]
    [SerializeField] private float loseDelay = 6f;

    [Header("UI")]
    [SerializeField] private TMP_Text pigsLabel;
    [SerializeField] private TMP_Text birdsLabel;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text resultScore;

    private int pigsRemaining;
    private int birdsRemaining;
    private int score;
    private bool gameOver;

    private void Awake()
    {
        Instance = this;

        pigsRemaining = FindObjectsByType<Pig>(FindObjectsSortMode.None).Length;
        birdsRemaining = FindObjectsByType<Bird>(FindObjectsSortMode.None).Length;

        Debug.Log($"[GameManager] Level started with {pigsRemaining} pig(s) and {birdsRemaining} bird(s).");
    }

    private void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        Refresh();
    }

    public void AddScore(int amount)
    {
        if (gameOver) { return; }

        score += amount;
        Refresh();
    }

    public void OnPigDied()
    {
        if (gameOver) { return; }

        pigsRemaining--;
        score += pointsPerPig;
        Refresh();

        Debug.Log($"[GameManager] Pig died, {pigsRemaining} remaining.");

        if (pigsRemaining <= 0)
        {
            EndGame(true);
        }
    }

    public void OnBlockDestroyed()
    {
        AddScore(pointsPerBlock);
    }

    public void OnBirdLaunched()
    {
        if (gameOver) { return; }

        birdsRemaining--;
        Refresh();

        if (birdsRemaining <= 0)
        {
            // Give the level time to settle before calling it.
            Invoke(nameof(CheckLose), loseDelay);
        }
    }

    private void CheckLose()
    {
        if (gameOver == false && pigsRemaining > 0)
        {
            EndGame(false);
        }
    }

    private void EndGame(bool won)
    {
        gameOver = true;

        if (won)
        {
            score += birdsRemaining * pointsPerUnusedBird;
        }

        Refresh();

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultTitle != null)
        {
            resultTitle.text = won ? "LEVEL CLEARED" : "OUT OF BIRDS";
        }

        if (resultScore != null)
        {
            resultScore.text = score.ToString("N0");
        }

        Debug.Log($"[GameManager] Game over. Won = {won}, final score = {score}");
    }

    private void Refresh()
    {
        if (pigsLabel != null) { pigsLabel.text = "PIGS  " + pigsRemaining; }
        if (birdsLabel != null) { birdsLabel.text = "BIRDS  " + Mathf.Max(0, birdsRemaining); }
        if (scoreLabel != null) { scoreLabel.text = score.ToString("N0"); }
    }
}