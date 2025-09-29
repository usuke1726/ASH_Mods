
using ModdingAPI;
namespace GenericModConfigMenu.Options;

internal interface IOption
{
    bool Unsaved { get; set; }
    SubmenuItemEntry MenuItem(OptionsMenu? topMenu, AbstractMenu? modMenu);
}

