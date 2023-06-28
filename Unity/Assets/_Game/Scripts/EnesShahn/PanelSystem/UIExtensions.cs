using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace EnesShahn.PanelSystem
{
    public static class UIExtensions
    {
        public static TweenerCore<float, float, FloatOptions> DOFade(this CanvasGroup cg, float endAlpha, float duration)
        {
            return DOTween.To(() => cg.alpha, (a) => cg.alpha = a, endAlpha, duration);
        }
        public static TweenerCore<float, float, FloatOptions> DOFade(this Text txt, float endAlpha, float duration)
        {
            return DOTween.To(() => { return txt.color.a; }, (a) => { txt.color = GetColor(txt.color, a); }, endAlpha, duration);
        }
        public static TweenerCore<float, float, FloatOptions> DOFill(this Image img, float duration)
        {
            float t = 0;
            float startFill = img.fillAmount;
            return DOTween.To(() => t, (newT) =>
            {
                t = newT;
                img.fillAmount = Mathf.Lerp(startFill, 1f, t);
            }, 1f, duration);
        }

        private static Color GetColor(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}