using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework.Input;

namespace BigChess;

public class HotReloadCartridge : MultiCartridge
{
    public HotReloadCartridge(IRuntime runtime, params Cartridge[] startingCartridges) : base(runtime, startingCartridges)
    {
    }

    protected override void BeforeUpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Keyboard.Modifiers.Control && input.Keyboard.GetButton(Keys.R, true).WasPressed)
        {

            CurrentCartridge?.Unload();
            
            RegenerateCurrentCartridge();
            
            if (CurrentCartridge is IHotReloadable hotReloadable)
            {
                hotReloadable.OnHotReload();
            }
        }
    }
}