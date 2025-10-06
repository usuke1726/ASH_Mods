
using SideStory.Dialogue;
using SideStory.System;

namespace SideStory.NodeData;

internal class NewGame : NodeEntry
{
    private static readonly string newGameNode = "NewGame.StartedFirstNode";
    protected override Node[] Nodes => [
        new([
            tag(newGameNode, true),
            wait(2.0f),
            line("node.newgame01", Player),
            line("node.newgame02", Player),
            line("node.newgame03", Player),
            line("node.newgame04", Player),
            wait(1.5f),
            line("node.newgame05", Player),
            line("node.newgame06", Player),
            line("node.newgame07", Player),
            line("node.newgame08", Player),
            wait(1.5f),
            line("node.newgame09", Player),
            line("node.newgame10", Player),
            line("node.newgame11", Player),
            line("node.newgame12", Player),
            line("node.newgame13", Player),
            wait(1.5f),
            line("node.newgame14", Player),
            line("node.newgame15", Player),
            end(),
        ],
            condition: () => State.IsNewGame && !STags.GetBool(newGameNode),
            priority: int.MaxValue),
    ];
}

