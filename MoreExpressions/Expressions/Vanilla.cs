
namespace MoreExpressions.Expressions;

internal abstract class VanillaEmotion(Emotion e) : CustomExpression()
{
    private readonly Emotion emotion = e;
    protected override void Update(CharacterObject obj) { }
    internal override void Activate(CharacterObject obj) => obj.ShowVanillaEmotion(emotion);
    internal override void Deactivate(CharacterObject obj) { }
}

internal class Happy() : VanillaEmotion(Emotion.Happy) { }
internal class Surprise() : VanillaEmotion(Emotion.Surprise) { }
internal class EyesClosed() : VanillaEmotion(Emotion.EyesClosed) { }

