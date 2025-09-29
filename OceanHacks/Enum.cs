
namespace OceanHacks;

partial class OceanHacks
{
    public static class Tags
    {
        public static readonly string Color = "_Color";
        public static readonly string DistortTex = "_DistortTex";
        public static readonly string RippleSpeed = "_RippleSpeed";
        public static readonly string MainTex = "_MainTex";
        public static readonly string EdgeColorR = "_EdgeColorR";
        public static readonly string EdgeColorG = "_EdgeColorG";
        public static readonly string EdgeColorB = "_EdgeColorB";
        public static readonly string EdgeColorA = "_EdgeColorA";
        public static readonly string EdgeDistortStrength = "_EdgeDistortStrength";
        public static readonly string FoamColor = "_FoamColor";
        public static readonly string FoamThickness = "_FoamThickness";
        public static readonly string BehindFoamCutoff = "_BehindFoamCutoff";
        public static readonly string FoamDistortStrength = "_FoamDistortStrength";
        public static readonly string WhiteFoamCutoff = "_WhiteFoamCutoff";
        public static readonly string WhiteFoamColor = "_WhiteFoamColor";
        public static readonly string RippleTex = "_RippleTex";
        public static readonly string RippleAlphaCutoff = "_RippleAlphaCutoff";
        public static readonly string DistortStrength = "_DistortStrength";
        public static readonly string WaveHeight = "_WaveHeight";
        public static readonly string WaveSpeed = "_WaveSpeed";
        public static readonly string WaveLengthX = "_WaveLengthX";
        public static readonly string WaveLengthY = "_WaveLengthY";
    }
    public enum ColorRegion
    {
        /// <summary>
        /// normal ocean color, the 3rd darkest blue
        /// </summary>
        Normal,

        /// <summary>
        /// outside of border, the darkest blue
        /// </summary>
        OutsideBorder,

        /// <summary>
        /// just inside of border, the 2nd darkest blue
        /// </summary>
        InsideBorder,

        /// <summary>
        /// whitish light blue, around Orange Islands
        /// </summary>
        AroundIslands,

        /// <summary>
        /// light blue, but more bluish than AroundIslands
        /// </summary>
        Coast,

        /// <summary>
        /// foam (light blue at the border with land, and mottled pattern on the ocean)
        /// </summary>
        Foam,

        /// <summary>
        /// the white line drawn at the border with land
        /// </summary>
        WhiteFoam,
    }
    public static string TagFromRegion(ColorRegion region) => region switch
    {
        ColorRegion.Normal => Tags.Color,
        ColorRegion.InsideBorder => Tags.EdgeColorR,
        ColorRegion.OutsideBorder => Tags.EdgeColorA,
        ColorRegion.AroundIslands => Tags.EdgeColorB,
        ColorRegion.Coast => Tags.EdgeColorG,
        ColorRegion.Foam => Tags.FoamColor,
        ColorRegion.WhiteFoam => Tags.WhiteFoamColor,
        _ => throw new Exception()
    };
}

