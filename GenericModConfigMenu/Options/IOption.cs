
using ModdingAPI;
namespace GenericModConfigMenu.Options;

internal interface IOption
{
    bool Unsaved { get; set; }
    bool Enabled { get; }
    SubmenuItemEntry MenuItem(OptionsMenu? topMenu, AbstractMenu? modMenu);
}

