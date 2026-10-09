using TMPro;
using UnityEngine;

public class UIMessagesManager : MonoBehaviour
{
    public static UIMessagesManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public TMP_Text messageText;
    public Animator messageAnimator;

    // Update is called once per frame
    static public void DrawMessage(string text)
    {
        Instance.m_DrawMessage(text);
    }
    private void m_DrawMessage(string text)
    {
        messageText.text = text;
        messageAnimator.Play("Message",0);
    }
}
