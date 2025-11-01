using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable enable

public class BoardAttackIconManager : MonoBehaviour
{
    private WaitForSeconds _explanationTextDelay;

    [SerializeField] private BoardAttackIconInfo[] _iconInfos;

    [Space(30)]
    [SerializeField] private GameObject _rootPanel;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _detailsText;
    [SerializeField] private TextMeshProUGUI _explanationText;

    [Space(30)]
    [SerializeField] private UnityEvent? OnShown;
    [SerializeField] private UnityEvent? OnHidden;

    [Space(30)]
    [SerializeField] private float _explanationTextDisplayTimeSeconds;


    private void Awake()
    {
#if DEBUG
        var notRepresentedTypes = new HashSet<BoardAttackType>(GameFacts.BoardAttackTypeValues);

        foreach (var icon in _iconInfos)
        {
            notRepresentedTypes.Remove(icon.RepresentedAttackType);
        }

        if (notRepresentedTypes.Any())
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"The attack types ");
                
            foreach (var type in notRepresentedTypes)
            {
                stringBuilder.Append($"{type}, ");
            }

            stringBuilder.Append("do not have infos in the board attack icon manager");
            throw new System.Exception(stringBuilder.ToString());
        }
#endif
        _explanationTextDelay = new WaitForSeconds(_explanationTextDisplayTimeSeconds);
    }

    public void Show(BoardAttack attack)
    {
        _rootPanel.SetActive(true);
        var info = _iconInfos.First(i => i.RepresentedAttackType == attack.Type);
        _image.sprite = info.Sprite;
        _nameText.text = info.Name;
        _detailsText.text = BoardAttackIconInfo.GetDetails(attack);
        _explanationText.text = info.Explanation;
        OnShown?.Invoke();
    }
        
    public void Hide()
    {
        OnHidden?.Invoke();
    }

    public void ShowExplanation()
        => StartCoroutine(TemporarilyShowExplanation());
        
    public IEnumerator TemporarilyShowExplanation()
    {
        _explanationText.gameObject.SetActive(true);
        yield return _explanationTextDelay;
        _explanationText.gameObject.SetActive(false);
    }
}