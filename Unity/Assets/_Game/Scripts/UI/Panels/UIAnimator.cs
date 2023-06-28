using System;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;

public class UIAnimator : MonoBehaviour
{
    [SerializeField] private List<Transform> _uiIItemsToAnimate;
    [SerializeField] private float _animationTime = 0.2f;

    private Sequence _sequence;

    public void ShowUIItems(TweenCallback onComplete = null)
    {
        if (_sequence != null)
            _sequence.Kill();
        _sequence = DOTween.Sequence();
        foreach (var uiItemToAnimate in _uiIItemsToAnimate)
        {
            _sequence.Join(uiItemToAnimate.DOScale(Vector3.one, _animationTime).SetUpdate(true).From(Vector3.zero).SetEase(Ease.InOutQuad));
        }
        _sequence.AppendInterval(_animationTime);
        _sequence.AppendCallback(onComplete);

    }
    public void HideUIItems(TweenCallback onComplete = null)
    {
        if (_sequence != null)
            _sequence.Kill();
        _sequence = DOTween.Sequence();
        foreach (var uiItemToAnimate in _uiIItemsToAnimate)
        {
            _sequence.Join(uiItemToAnimate.DOScale(Vector3.zero, _animationTime).SetUpdate(true).From(Vector3.one).SetEase(Ease.InOutQuad));
        }
        _sequence.AppendInterval(_animationTime);
        _sequence.AppendCallback(onComplete);
    }
}
