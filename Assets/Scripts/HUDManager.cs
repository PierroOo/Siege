using TMPro;
using UnityEngine;
using static GameManager;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance { get; private set; }

    public TMP_Text playerTurnText;
    public TMP_Text roundCountText;
    public TMP_Text selectedText;
    public TMP_Text resultText;
    public GameObject groupIntro;
    public GameObject groupBattle;
    public GameObject groupOutro;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        GameManager.onNextTurn += OnNextTurn;
        GameManager.onGameStateChanged += OnGameStateChanged;
    }
    private void OnDestroy()
    {
        GameManager.onNextTurn -= OnNextTurn;
        GameManager.onGameStateChanged -= OnGameStateChanged;
    }

    private void Start()
    {
        playerTurnText.text = "-";
        roundCountText.text = "-";
    }

    // Update is called once per frame
    public void OnNextTurn(PlayerType player, int roundCount)
    {
        playerTurnText.text = "It's the "+ player.ToString() + " turn to play";
        roundCountText.text = "Round number " + roundCount.ToString();
    }

    public void OnGameStateChanged(GameManager.GameState lastGameState, GameManager.GameState newGameState)
    {
        groupIntro.SetActive(newGameState == GameState.Intro);
        groupBattle.SetActive(newGameState == GameState.Battle);
        groupOutro.SetActive(newGameState == GameState.Outro);

        if (newGameState == GameState.Outro)
        {
            resultText.text = BaseAgent.defenders.Count == 0 ? "Assailant Win !" : "Defender win !";
        }
    }

    public void DrawSelection(BaseEntity selectedEntity)
    {
        selectedText.gameObject.SetActive(true);
        string text = selectedEntity.name;

        BaseAgent agent = selectedEntity as BaseAgent;
        if (agent != null)
        {
            text += "\n HP: "+agent.healthPoints+"/"+agent.maxHealthPoints;
            text += "\n ATT: " + agent.attack + "/" + agent.maxAttack;
            text += "\n ACT: ";
            if (agent.behaviour == BaseAgent.AgentBehaviour.noTarget)
                text += "Waiting";
            else if (agent.agentTarget != null)
                text += " Attack " + agent.agentTarget.name;
            else
                text += "Move";
        }

        selectedText.text = text;
    }
    public void HideSelection()
    {
        selectedText.gameObject.SetActive(false);
    }
}
