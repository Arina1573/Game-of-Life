using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("UI Elements - Control")]
    public Button startButton;
    public Button stopButton;
    public Button clearButton;
    public Button randomButton;
    public Slider speedSlider;
    public TMP_Text speedText;

    [Header("References")]
    public GameBoard gameBoard;

    private void Start()
    {
        InitializeControlUI();
        UpdateUI();
    }

    private void InitializeControlUI()
    {
        startButton.onClick.AddListener(() => gameBoard.StartSimulation());
        stopButton.onClick.AddListener(() => gameBoard.StopSimulation());
        clearButton.onClick.AddListener(() => gameBoard.ClearBoard());
        randomButton.onClick.AddListener(() => gameBoard.SetRandomPattern());

        speedSlider.minValue = 1f;
        speedSlider.maxValue = 30f;
        speedSlider.value = 10f;
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        
        UpdateSpeedText(10f);
    }

    private void OnSpeedChanged(float value)
    {
        gameBoard.SetUpdateSpeed(value);
        UpdateSpeedText(value);
    }

    private void UpdateSpeedText(float value)
    {
        speedText.text = $"Speed: {value:F0}";
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isRunning = gameBoard.IsRunning;

        startButton.interactable = !isRunning;
        stopButton.interactable = isRunning;
    }
}