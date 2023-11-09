using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Gui;
using ExplogineMonoGame.Rails;
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
        painter.DrawStringAtPosition(Client.Assets.GetFont("engine/logo-font", 100), _title, Vector2.Zero, new DrawSettings());
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

    protected record ButtonTemplate(string Name, Action Execute);

    protected void GenerateButtons(List<ButtonTemplate> items)
    {
        var buttonSize = new Vector2(500, 100);
        var boundaries = new RectangleF();
        var buttonRect = new RectangleF(Vector2.Zero, buttonSize);

        _gui.Clear();

        foreach (var item in items)
        {
            buttonRect = buttonRect.Moved(new Vector2(0, buttonSize.Y + 20));
            _gui.Button(buttonRect, item.Name, Depth.Middle, item.Execute);
        }

        boundaries = RectangleF.Union(boundaries, buttonRect);
        _scrollableArea = new ScrollableArea(_widget.Size, boundaries, Depth.Front + 10);
    }

    public abstract void Cancel();
}
