
using System.Text.RegularExpressions;

namespace ModdingAPI;
public static class Util
{
    private static readonly Regex patternJP = new(@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]");
    public static bool IsUsingJapanese(string text) => patternJP.IsMatch(text);

    public class RandomSeed : IDisposable
    {
        private readonly UnityEngine.Random.State state;
        public RandomSeed(int? seed)
        {
            state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed ?? TimeToSeed());
        }
        public RandomSeed() : this(null) { }
        private static int TimeToSeed()
        {
            var time = DateTime.Now;
            return ( // range: 0-1,382,399,999
                (
                    (time.Day * 24 + time.Hour) * 60 + time.Minute
                ) * 60 + time.Second
            ) * 500 + time.Millisecond / 2;
            //return time.Minute * 60000 + time.Second * 1000 + time.Millisecond; // range: 0-3,599,999
        }
        ~RandomSeed() => Dispose(false);
        public void Dispose() => Dispose(true);
        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            UnityEngine.Random.state = state;
        }
    }
}

