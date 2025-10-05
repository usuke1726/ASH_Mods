
using SideStory.Dialogue.Actions;
using SideStory.System;

namespace SideStory.Dialogue.Data;

internal class NewGame : DataEntry
{
    private static readonly string newGameNode = "NewGame.StartedFirstNode";
    internal static void Setup()
    {
        NodeSelector.RegisterNode(new([
                new TagAction(newGameNode, true),
                new WaitAction(2.0f),
                new LineAction("node.newgame01", Player),
                new LineAction("node.newgame02", Player),
                new LineAction("node.newgame03", Player),
                new LineAction("node.newgame04", Player),
                new WaitAction(1.5f),
                new LineAction("node.newgame05", Player),
                new LineAction("node.newgame06", Player),
                new LineAction("node.newgame07", Player),
                new LineAction("node.newgame08", Player),
                new WaitAction(1.5f),
                new LineAction("node.newgame09", Player),
                new LineAction("node.newgame10", Player),
                new LineAction("node.newgame11", Player),
                new LineAction("node.newgame12", Player),
                new LineAction("node.newgame13", Player),
                new WaitAction(2.0f),
                new LineAction("node.newgame14", Player),
                new LineAction("node.newgame15", Player),
                new NodeCompleteAction(),
            ],
            condition: () => State.IsNewGame && !STags.GetBool(newGameNode),
            priority: int.MaxValue
        ));
    }
}

