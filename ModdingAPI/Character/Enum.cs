
namespace ModdingAPI;

// Some of names of characters are based on https://ashorthike.fandom.com/wiki/Characters

/// <summary>NPCs and the player</summary>
public enum UniqueCharacters
{
    /// <summary>the player's character</summary>
    Claire,
    /// <summary>Claire's aunt</summary>
    AuntMay,
    /// <summary>a white ram wearing a blue and gray shirt and a sun hat who sells golden feathers and a hat</summary>
    RangerJon,
    /// <summary>a teal lizard wearing a two-tone blue shirt who tells Claire some rumors</summary>
    RumorGuy,
    /// <summary>a small blue bird who tell Claire how to glide</summary>
    GlideKid,
    /// <summary>a small purple bird who ask Claire to bring 15 shells</summary>
    Jen,
    /// <summary>a warlus who lend Claire a fishing rod and teach how to fish</summary>
    Bill,
    /// <summary>a dog at Outlook Point who gives Claire a treasure map (is this dog Jim?)</summary>
    OutlookPointGuy,
    /// <summary>a blue bird with a mohawk and black shirt who sells goldren feathers</summary>
    ToughBird,
    /// <summary>a brown deer with a pink and cyan shirt who sells a sunhat</summary>
    SunhatDeer,
    /// <summary>a black toucan who tell how to dive</summary>
    DiveKid,
    /// <summary>a green lizard "running is my life"</summary>
    RunningLizard,
    /// <summary>a gray dog, Shirley's Point Guy's nephew "i hate leg days"</summary>
    RunningNephew,
    /// <summary>a orange rabbit "it's dangerous to go slow!"</summary>
    RunningRabbit,
    /// <summary>a  who refuse Claire's participation of the race "SPEED FIRST!!"</summary>
    RunningGoat,
    /// <summary>a frog wearing an aqua and orange shirt and sunglasses who talks about breakfast</summary>
    BreakfastKid,
    /// <summary>a gray cat who has lost their permit</summary>
    Camper,
    /// <summary>a frog who give Claire a shovel</summary>
    ShovelKid,
    /// <summary>an gray fox who lend Claire a compass</summary>
    CompassFox,
    /// <summary>a turtle wieh a red headband</summary>
    Taylor,
    /// <summary>a white rabbit wearing a blue and purple shirt who gives Claire a pair of running shoes.</summary>
    Sue,
    /// <summary>Watch Guy, a goat wearing a teal and green shirt, finding thier watch.</summary>
    WatchGoat,
    /// <summary>a red bird with a light red tuft of hair wearing an aqua shirt</summary>
    Julie,
    /// <summary>a Beachstickball player, a dark green bird wearing a blue shirt</summary>
    BeachstickballKid,
    /// <summary>a Parkour racer, a brown eagle with a red and pink shirt</summary>
    Avery,
    /// <summary>Shirley's Point Guy, a dark brown dog wearing a dark blue and black shirt</summary>
    HydrationDog,


    /// <summary>a deer who hires out a boat</summary>
    DadBoatDeer,
    /// <summary>a deer who goes boating with Claire</summary>
    KidBoatDeer,
    /// <summary>a white seagull who buy fish from Claire</summary>
    ShipWorker,
    /// <summary>a polar bear who is writing a book</summary>
    Charlie,
    /// <summary>a squirrel of the rock climbing club</summary>
    Tim,
    /// <summary>a rhino of the rock climbing club</summary>
    ClimbingRhino,
    /// <summary>an orange fox who take a picture at the top of Hawk Peak</summary>
    PictureFox,
    /// <summary>Painting Kid, a raccoon wearing a two-tone blue shirt</summary>
    Artist,
}

/// <summary><inheritdoc cref="UniqueCharacters"/> (variation by object)</summary>
public enum Characters
{
    /// <inheritdoc cref="UniqueCharacters.Claire"/>
    Claire = UniqueCharacters.Claire,
    /// <inheritdoc cref="UniqueCharacters.AuntMay"/>
    AuntMay = UniqueCharacters.AuntMay,
    /// <inheritdoc cref="UniqueCharacters.RangerJon"/>
    RangerJon = UniqueCharacters.RangerJon,
    /// <inheritdoc cref="UniqueCharacters.RumorGuy"/>
    RumorGuy = UniqueCharacters.RumorGuy,
    /// <inheritdoc cref="UniqueCharacters.GlideKid"/>
    GlideKid = UniqueCharacters.GlideKid,
    /// <inheritdoc cref="UniqueCharacters.Jen"/>
    Jen = UniqueCharacters.Jen,
    /// <inheritdoc cref="UniqueCharacters.Bill"/>
    Bill = UniqueCharacters.Bill,
    /// <inheritdoc cref="UniqueCharacters.OutlookPointGuy"/>
    OutlookPointGuy = UniqueCharacters.OutlookPointGuy,
    /// <inheritdoc cref="UniqueCharacters.ToughBird"/>
    ToughBird = UniqueCharacters.ToughBird,
    /// <inheritdoc cref="UniqueCharacters.SunhatDeer"/>
    SunhatDeer = UniqueCharacters.SunhatDeer,
    /// <inheritdoc cref="UniqueCharacters.DiveKid"/>
    DiveKid = UniqueCharacters.DiveKid,
    /// <inheritdoc cref="UniqueCharacters.RunningLizard"/>
    RunningLizard = UniqueCharacters.RunningLizard,
    /// <inheritdoc cref="UniqueCharacters.RunningNephew"/>
    RunningNephew = UniqueCharacters.RunningNephew,
    /// <inheritdoc cref="UniqueCharacters.RunningRabbit"/>
    RunningRabbit = UniqueCharacters.RunningRabbit,
    /// <inheritdoc cref="UniqueCharacters.RunningGoat"/>
    RunningGoat = UniqueCharacters.RunningGoat,
    /// <inheritdoc cref="UniqueCharacters.BreakfastKid"/>
    BreakfastKid = UniqueCharacters.BreakfastKid,
    /// <inheritdoc cref="UniqueCharacters.Camper"/>
    Camper = UniqueCharacters.Camper,
    /// <inheritdoc cref="UniqueCharacters.ShovelKid"/>
    ShovelKid = UniqueCharacters.ShovelKid,
    /// <inheritdoc cref="UniqueCharacters.CompassFox"/>
    CompassFox = UniqueCharacters.CompassFox,
    /// <inheritdoc cref="UniqueCharacters.Taylor"/>
    Taylor = UniqueCharacters.Taylor,
    /// <inheritdoc cref="UniqueCharacters.Sue"/>
    Sue = UniqueCharacters.Sue,
    /// <inheritdoc cref="UniqueCharacters.WatchGoat"/>
    WatchGoat = UniqueCharacters.WatchGoat,
    /// <inheritdoc cref="UniqueCharacters.Julie"/>
    Julie = UniqueCharacters.Julie,
    /// <inheritdoc cref="UniqueCharacters.BeachstickballKid"/>
    BeachstickballKid = UniqueCharacters.BeachstickballKid,
    /// <inheritdoc cref="UniqueCharacters.Avery"/>
    Avery = UniqueCharacters.Avery,
    /// <inheritdoc cref="UniqueCharacters.HydrationDog"/>
    HydrationDog = UniqueCharacters.HydrationDog,


    /// <summary><inheritdoc cref="UniqueCharacters.DadBoatDeer"/> (reading a book)</summary>
    DadBoatDeer1,
    /// <summary><inheritdoc cref="UniqueCharacters.DadBoatDeer"/> (standing on the dock)</summary>
    DadBoatDeer2,
    /// <summary><inheritdoc cref="UniqueCharacters.KidBoatDeer"/> (lying down)</summary>
    KidBoatDeer1,
    /// <summary><inheritdoc cref="UniqueCharacters.KidBoatDeer"/> (on the boat)</summary>
    KidBoatDeer2,
    /// <summary><inheritdoc cref="UniqueCharacters.KidBoatDeer"/> (reading a book)</summary>
    KidBoatDeer3,
    /// <summary><inheritdoc cref="UniqueCharacters.ShipWorker"/> (on the ship)</summary>
    ShipWorker1,
    /// <summary><inheritdoc cref="UniqueCharacters.ShipWorker"/> (on the dock)</summary>
    ShipWorker2,
    /// <summary><inheritdoc cref="UniqueCharacters.Charlie"/> (walking)</summary>
    Charlie1,
    /// <summary><inheritdoc cref="UniqueCharacters.Charlie"/> (sitting in front of his house)</summary>
    Charlie2,
    /// <summary><inheritdoc cref="UniqueCharacters.Tim"/> (at Sid Beach)</summary>
    Tim1,
    /// <summary><inheritdoc cref="UniqueCharacters.Tim"/> (climbing the bluff near ToughBird)</summary>
    Tim2,
    /// <summary><inheritdoc cref="UniqueCharacters.Tim"/> (sitting in front of Charlie's house)</summary>
    Tim3,
    /// <summary><inheritdoc cref="UniqueCharacters.ClimbingRhino"/> (at Sid Beach)</summary>
    ClimbingRhino1,
    /// <summary><inheritdoc cref="UniqueCharacters.ClimbingRhino"/> (on the bluff near ToughBird)</summary>
    ClimbingRhino2,
    /// <summary><inheritdoc cref="UniqueCharacters.ClimbingRhino"/> (sitting in front of Charlie's house)</summary>
    ClimbingRhino3,
    /// <summary><inheritdoc cref="UniqueCharacters.PictureFox"/> (at the foot of the mountain)</summary>
    PictureFox1,
    /// <summary><inheritdoc cref="UniqueCharacters.PictureFox"/> (at near the top of mountain)</summary>
    PictureFox2,
    /// <summary><inheritdoc cref="UniqueCharacters.Artist"/> (painting a coastline)</summary>
    Artist1,
    /// <summary><inheritdoc cref="UniqueCharacters.Artist"/> (painting a lighthouse)</summary>
    Artist2,
    /// <summary><inheritdoc cref="UniqueCharacters.Artist"/> (painting a river)</summary>
    Artist3,
    /// <summary><inheritdoc cref="UniqueCharacters.Artist"/> (painting a graveyard)</summary>
    Artist4,
    /// <summary><inheritdoc cref="UniqueCharacters.Artist"/> (painting a landscape at outlook point)</summary>
    Artist5,
    /// <summary><inheritdoc cref="UniqueCharacters.Artist"/> (painting Ranger Jon)</summary>
    Artist6,
}

public static class CharacterUtil
{
    public static bool TryParse(string name, out Characters characters)
    {
        return Enum.TryParse(name, true, out characters);
    }
    public static bool TryParse(string name, out UniqueCharacters characters)
    {
        if (Enum.TryParse(name, true, out characters)) return true;
        if (Enum.TryParse<Characters>(name, true, out var ch))
        {
            characters = ToUniqueCharacter(ch);
            return true;
        }
        return false;
    }
    public static bool IsSameCharacter(UniqueCharacters ch1, UniqueCharacters ch2) => ch1 == ch2;
    public static bool IsSameCharacter(UniqueCharacters ch1, Characters ch2) => IsSameCharacter(ch1, ToUniqueCharacter(ch2));
    public static bool IsSameCharacter(Characters ch1, UniqueCharacters ch2) => IsSameCharacter(ToUniqueCharacter(ch1), ch2);
    public static bool IsSameCharacter(Characters ch1, Characters ch2) => IsSameCharacter(ToUniqueCharacter(ch1), ToUniqueCharacter(ch2));
    public static Characters ToCharacter(UniqueCharacters ch) => ch switch
    {
        UniqueCharacters.DadBoatDeer => Characters.DadBoatDeer2,
        UniqueCharacters.KidBoatDeer => Characters.KidBoatDeer2,
        UniqueCharacters.ShipWorker => Characters.ShipWorker2,
        UniqueCharacters.Charlie => Characters.Charlie1,
        UniqueCharacters.Tim => Characters.Tim1,
        UniqueCharacters.ClimbingRhino => Characters.ClimbingRhino1,
        UniqueCharacters.PictureFox => Characters.PictureFox1,
        UniqueCharacters.Artist => Characters.Artist1,
        _ => (Characters)ch
    };
    public static UniqueCharacters ToUniqueCharacter(Characters ch) => ch switch
    {
        Characters.DadBoatDeer1 or Characters.DadBoatDeer2 => UniqueCharacters.DadBoatDeer,
        Characters.KidBoatDeer1
            or Characters.KidBoatDeer2
            or Characters.KidBoatDeer3
            => UniqueCharacters.KidBoatDeer,
        Characters.ShipWorker1 or Characters.ShipWorker2 => UniqueCharacters.ShipWorker,
        Characters.Charlie1 or Characters.Charlie2 => UniqueCharacters.Charlie,
        Characters.Tim1
            or Characters.Tim2
            or Characters.Tim3
            => UniqueCharacters.Tim,
        Characters.ClimbingRhino1
            or Characters.ClimbingRhino2
            or Characters.ClimbingRhino3
            => UniqueCharacters.ClimbingRhino,
        Characters.PictureFox1 or Characters.PictureFox2 => UniqueCharacters.PictureFox,
        Characters.Artist1
            or Characters.Artist2
            or Characters.Artist3
            or Characters.Artist4
            or Characters.Artist5
            or Characters.Artist6
            => UniqueCharacters.Artist,
        _ => (UniqueCharacters)ch
    };
}

