
using ModdingAPI;

namespace IMoreExpressions;

public interface IMoreExpressionsApi
{
    void ShowEmotion(Characters ch, Expression type);
    void ToggleEmotion(Characters ch, Expression type);
    void ToggleEmotion(Characters ch, string name);
    void ShowEmotion(Characters ch, string name);
    void ResetEmotion(Characters ch);
}

