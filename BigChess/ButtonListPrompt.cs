using System;
using System.Collections.Generic;
using ChessCommon;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Gui;
using ExplogineMonoGame.Rails;
using ExTween;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BigChess;

public abstract class ButtonListPrompt : Prompt, IEarlyDrawHook
{
    private readonly Gui _gui;
    private readonly IRuntime _runtime;
    private readonly string _title;
    private readonly Widget _widget;
    private ScrollableArea _scrollableArea;

    protected ButtonListPrompt(IRuntime runtime, string title)
    {
        _runtime = runtime;
        _title = title;
        _widget = new Widget(WindowRectangle(), Depth.Middle);
        _gui = new Gui();
        _scrollableArea = new ScrollableArea(_widget.Size, _widget.Size.ToRectangleF(), Depth.Front);
    }

    public void EarlyDraw(Painter painter)
    {
        _gui.PrepareCanvases(painter, Constants.Theme);

        Client.Graphics.PushCanvas(_widget.Canvas);
        painter.BeginSpriteBatch(_scrollableArea.CanvasToScreen);
        painter.DrawStringAtPosition(Client.Assets.GetFont("engine/logo-font", 100), _title, Vector2.Zero,
            new DrawSettings());
        _gui.Draw(painter, Constants.Theme);
        painter.EndSpriteBatch();
        Client.Graphics.PopCanvas();
    }

    private RectangleF WindowRectangle()
    {
        return _runtime.Window.RenderResolution.ToRectangleF().Inflated(-50, -50);
    }

    protected override void UpdateInputInternal(ConsumableInput input, HitTestStack overlayLayer)
    {
        var widgetLayer = overlayLayer.AddLayer(_scrollableArea.ScreenToCanvas * _widget.ScreenToCanvas, Depth.Middle,
            _widget.OutputRectangle);
        _gui.UpdateInput(input, widgetLayer);

        if (input.Keyboard.GetButton(Keys.Escape, true).WasPressed)
        {
            Cancel();
        }

        _scrollableArea.DoScrollWithMouseWheel(input);
        input.Mouse.ConsumeScrollDelta();
    }

    public override void Update(float dt)
    {
        _widget.ResizeCanvas(WindowRectangle().Size.ToPoint());
    }

    protected override void DrawInternal(Painter painter)
    {
        painter.BeginSpriteBatch();
        _widget.Draw(painter);
        painter.EndSpriteBatch();
    }

    protected void GenerateButtons(List<IEditorOption> items)
    {
        var buttonSize = new Vector2(500, 100);
        var boundaries = new RectangleF();
        var buttonRect = new RectangleF(Vector2.Zero, buttonSize);

        _gui.Clear();

        foreach (var item in items)
        {
            buttonRect = buttonRect.Moved(new Vector2(0, buttonSize.Y + 20));

            item.AddToGui(_gui, buttonRect);
        }

        boundaries = RectangleF.Union(boundaries, buttonRect);
        _scrollableArea = new ScrollableArea(_widget.Size, boundaries, Depth.Front + 10);
    }

    protected interface IEditorOption
    {
        void AddToGui(Gui gui, RectangleF buttonRect);
    }

    protected record ButtonTemplate(string Name, Action Execute) : IEditorOption
    {
        public void AddToGui(Gui gui, RectangleF buttonRect)
        {
            gui.Button(buttonRect, Name, Depth.Middle, Execute);
        }
    }

    protected record SliderTemplate(Func<int, string> Label, TweenableInt SliderValue) : IEditorOption
    {
        public void AddToGui(Gui gui, RectangleF buttonRect)
        {
            var controlSize = new Vector2(buttonRect.Height);
            gui.Button(RectangleF.FromSizeAlignedWithin(buttonRect, controlSize, Alignment.CenterLeft), "--",
                Depth.Middle, () => SliderValue.Value--);

            var labelRect = new RectangleF(buttonRect.Location + new Vector2(controlSize.X, 0),
                new Vector2(buttonRect.Width - controlSize.X * 2, buttonRect.Height));
            gui.DynamicLabel(labelRect, Depth.Middle,
                (painter, theme, rectangle, depth) =>
                {
                    painter.DrawStringWithinRectangle(theme.Font, Label(SliderValue.Value), rectangle, Alignment.Center,
                        new DrawSettings {Depth = depth});
                });

            gui.Button(RectangleF.FromSizeAlignedWithin(buttonRect, controlSize, Alignment.CenterRight), "++",
                Depth.Middle, () => SliderValue.Value++);
        }
    }
}
