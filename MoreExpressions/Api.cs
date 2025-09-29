
using IMoreExpressions;
using ModdingAPI;

namespace MoreExpressions;

internal class Api : IMoreExpressionsApi
{
    internal static Api instance = new();
    public void ResetEmotion(Characters ch) => CustomExpression.ResetEmotion(ch);
    public void ShowEmotion(Characters ch, Expression type) => CustomExpression.ShowEmotion(ch, type);
    public void ShowEmotion(Characters ch, string name) => CustomExpression.ShowEmotion(ch, name);
    public void ToggleEmotion(Characters ch, Expression type) => CustomExpression.ToggleEmotion(ch, type);
    public void ToggleEmotion(Characters ch, string name) => CustomExpression.ToggleEmotion(ch, name);
}

