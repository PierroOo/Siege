using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetGameState(GameState.Intro);
    }

    public enum PlayerType { Assailant, Defender, Neutral}
    public Color assailantColor;
    public Color defenderColor;
    public Color neutralColor;
    static public Color GetTeamColor(PlayerType team)
    {
        switch (team)
        {
            case PlayerType.Assailant:
                return instance.assailantColor;
            case PlayerType.Defender:
                return instance.defenderColor;
            case PlayerType.Neutral:
            default:
                return instance.neutralColor;
        }
    }
    public int assailantLayer;
    public int defenderLayer;
    public int neutralLayer;
    static public int GetTeamLayer(PlayerType team)
    {
        switch (team)
        {
            case PlayerType.Assailant:
                return instance.assailantLayer;
            case PlayerType.Defender:
                return instance.defenderLayer;
            case PlayerType.Neutral:
            default:
                return instance.neutralLayer;
        }
    }
    public LayerMask assailantEnnemisMask;
    public LayerMask defenderEnnemisMask;
    public LayerMask neutralEnnemisMask;
    static public LayerMask GetTeamEnnemisLayer(PlayerType team)
    {
        switch (team)
        {
            case PlayerType.Assailant:
                return instance.assailantEnnemisMask;
            case PlayerType.Defender:
                return instance.defenderEnnemisMask;
            case PlayerType.Neutral:
            default:
                return instance.neutralEnnemisMask;
        }
    }
    public LayerMask assailantCollisionMask;
    public LayerMask defenderCollisionMask;
    public LayerMask neutralCollisionMask;
    static public LayerMask GetTeamCollisionLayer(PlayerType team)
    {
        switch (team)
        {
            case PlayerType.Assailant:
                return instance.assailantCollisionMask;
            case PlayerType.Defender:
                return instance.defenderCollisionMask;
            case PlayerType.Neutral:
            default:
                return instance.neutralCollisionMask;
        }
    }


    public enum GameState {Intro, Battle, Outro }

    public PlayerType turn = PlayerType.Neutral;
    public GameState state = GameState.Intro;

    public int maxAssailants = 8;

    int roundCount = 0;

    public delegate void OnGameStateChanged(GameState lastState,GameState newState);
    public static event OnGameStateChanged onGameStateChanged;
    public void StartGame()
    {
        if (state != GameState.Intro)
            return;

        SetGameState(GameState.Battle);
    }

    void SetGameState(GameState nextState)
    {
        GameState lastState = state;
        state = nextState;
        
        switch (state)
        {
            case GameState.Battle:
                roundCount = 0;
                turn = PlayerType.Neutral;
                Time.timeScale = 0f;
                foreach(BaseAgent assailant in BaseAgent.assailants)
                    assailant.Kill();
                foreach (BaseAgent defender in BaseAgent.defenders)
                    defender.Kill();

                if (onGameStateChanged != null)
                    onGameStateChanged.Invoke(lastState, nextState);
                NextPhase();
                break;
            case GameState.Outro:
                Time.timeScale = 0f;
                if (onGameStateChanged != null)
                    onGameStateChanged.Invoke(lastState, nextState);
                break;
            case GameState.Intro:
                Time.timeScale = 0f;
                if (onGameStateChanged != null)
                    onGameStateChanged.Invoke(lastState, nextState);
                break;
        }
    }

    public void EndTurn()
    {
        if (state != GameState.Battle)
            return;

        if ( turn != PlayerType.Neutral)
            NextPhase();
    }

    public delegate void OnNextTurn(PlayerType player, int roundCount);
    //public static event OnNextTurn onPreNextTurn;
    public static event OnNextTurn onNextTurn;
    public delegate void OnNextRound(int roundCount);
    public static event OnNextRound onNextRound;
    void NextPhase()
    {
        switch (turn)
        {
            case PlayerType.Assailant:
                turn = PlayerType.Defender;
                UIMessagesManager.DrawMessage("Defender Turn");
                break;
            case PlayerType.Defender:
                turn = PlayerType.Neutral;
                UIMessagesManager.DrawMessage("Action !");
                StopAllCoroutines();
                StartCoroutine(NeutralSequence());
                break;

            case PlayerType.Neutral:
            default:
                turn = PlayerType.Assailant;
                roundCount++;
                if (onNextRound != null)
                    onNextRound.Invoke(roundCount);

                foreach (BaseAgent assailant in BaseAgent.assailants)
                    assailant.Heal(1);
                foreach (BaseAgent defender in BaseAgent.defenders)
                    defender.Heal(1);

                UIMessagesManager.DrawMessage("Assailant Turn");
                break;
        }

        //if (onPreNextTurn != null)
        //    onPreNextTurn(turn, roundCount);

        if (onNextTurn != null)
            onNextTurn(turn, roundCount);

        CheckGameState();
    }

    public float actionTime = 5f;
    public IEnumerator NeutralSequence()
    {
        Time.timeScale = 1f;
        yield return new WaitForSeconds(actionTime);
        Time.timeScale = 0f;
        NextPhase();
    }

    public void CheckGameState()
    {
        if (roundCount < 2) //HOTFIX
            return;

        if(roundCount == 11)
        {
            UIMessagesManager.DrawMessage("Defender win !");
            EndGame();
        }

        if (BaseAgent.defenders.Count == 0)
        {
            UIMessagesManager.DrawMessage("Assailant Win !");
            EndGame();
        }
    }

    void EndGame()
    {
        StopAllCoroutines();
        StartCoroutine(OutroSequence());
    }

    public IEnumerator OutroSequence()
    {
        SetGameState(GameState.Outro);
        yield return new WaitForSecondsRealtime(3f);
        SetGameState(GameState.Intro);
    }
}
