
namespace CoinChallenge;

public interface ICoinChallenge
{
    bool ForceFinished { get; }
    bool UseTransitionStart { get; }
    bool UseTransitionEnd { get; }
    int? AllowedFeathers { get; }
    float? ArrowHintTime { get; }
    List<CollectOnTouch> Coins { get; }
    void Setup();
    void Cleanup();
}

