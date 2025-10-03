
using System.Text.RegularExpressions;
using UnityEngine;

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

    public static Texture2D EditableTexture(Texture2D texture)
    {
        var renderTexture = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );
        Graphics.Blit(texture, renderTexture);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        var editableTexture = new Texture2D(texture.width, texture.height);
        editableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        editableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return editableTexture;
    }
}

