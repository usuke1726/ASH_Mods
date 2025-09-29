
namespace ModdingAPI;

// Some of names of characters are based on https://ashorthike.fandom.com/wiki/Characters
public enum Characters
{
    /// <summary>the player's character</summary>
    Claire,
    /// <summary>Claire's aunt</summary>
    AuntMay,
    /// <summary>a white ram wearing a blue and gray shirt and a sun hat who sells golden feathers and a hat</summary>
    RangerJon,
    /// <summary>a deer who hires out a boat</summary>
    DadBoatDeer,
    /// <summary>a deer who goes boating with Claire</summary>
    KidBoatDeer,
    /// <summary>a white seagull who buy fish from Claire</summary>
    ShipWorker,
    /// <summary>a teal lizard wearing a two-tone blue shirt who tells Claire some rumors</summary>
    RumorGuy,
    /// <summary>a polar bear who is writing a book</summary>
    Charlie,
    /// <summary>a squirrel of the rock climbing club</summary>
    Tim,
    /// <summary>a rhino of the rock climbing club</summary>
    ClimbingRhino,
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
    /// <summary>an orange fox who take a picture at the top of Hawk Peak</summary>
    PictureFox,
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
    /// <summary>Painting Kid, a raccoon wearing a two-tone blue shirt</summary>
    Artist,
    /// <summary>Shirley's Point Guy, a dark brown dog wearing a dark blue and black shirt</summary>
    HydrationDog,
}

public static class CharactersParser
{
    public static bool TryParse(string name, out Characters characters)
    {
        return Enum.TryParse(name, true, out characters);
    }
}

