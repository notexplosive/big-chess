using System;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BigChess;

public class SavePrompt : Prompt, IEarlyDrawHook
{
    private readonly IRuntime _runtime;
    private readonly Lazy<TextInputWidget> _textInputWidget;
    private Action<string>? _bufferedCallback;
    private bool _hasInitialized;

    public SavePrompt(IRuntime runtime)
    {
        _runtime = runtime;
        _textInputWidget = new Lazy<TextInputWidget>(() => new TextInputWidget(Vector2.Zero, new Point(100, 100),
            new IndirectFont("engine/logo-font", 64),
            new TextInputWidget.Settings
                {Depth = Depth.Front, IsSingleLine = true, Selector = new AlwaysSelected(), ShowScrollbar = false}));
    }

    public override bool IsOpen => _bufferedCallback != null;

    public void EarlyDraw(Painter painter)
    {
        _textInputWidget.Value.PrepareDraw(painter, Constants.Theme);
    }

    protected override void UpdateInputInternal(ConsumableInput input, HitTestStack overlayLayer)
    {
        _textInputWidget.Value.UpdateInput(input, overlayLayer);

        if (input.Keyboard.GetButton(Keys.Escape, true).WasPressed)
        {
            Cancel();
        }
    }

    public override void Update(float dt)
    {
        var height = (int) _textInputWidget.Value.Font.GetFont().Height + 1;
        var position = new Vector2(0, _runtime.Window.RenderResolution.Y / 2f - _textInputWidget.Value.Size.Y / 2f);
        var size = new Vector2(_runtime.Window.RenderResolution.X, height);

        _textInputWidget.Value.OutputRectangle = new RectangleF(position, size).Inflated(-50, 0);
        _textInputWidget.Value.RenderResolution = _textInputWidget.Value.Size;
    }

    protected override void DrawInternal(Painter painter)
    {
        painter.BeginSpriteBatch();
        painter.DrawRectangle(_runtime.Window.RenderResolution.ToRectangleF(),
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(0.5f)});
        painter.EndSpriteBatch();

        painter.BeginSpriteBatch();
        painter.DrawStringAtPosition(Client.Assets.GetFont("engine/logo-font", 50),
            "Name this scenario (press enter when done)", _textInputWidget.Value.Position - new Vector2(0, 64),
            new DrawSettings());
        _textInputWidget.Value.Draw(painter);
        painter.EndSpriteBatch();
    }

    public void Request(Action<string> whenDone)
    {
        _bufferedCallback = whenDone;

        if (!_hasInitialized)
        {
            // we have to bend over backwards because TextInputWidget doesn't work pre cartridge startup
            _textInputWidget.Value.Submitted += Submit;
            _hasInitialized = true;
        }
    }

    private void Submit()
    {
        var text = _textInputWidget.Value.Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            _bufferedCallback?.Invoke(text);
        }

        _bufferedCallback = null;
    }

    public void Cancel()
    {
        _bufferedCallback = null;
    }
}
