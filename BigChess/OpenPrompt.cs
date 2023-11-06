using System;
using System.IO;
using ExplogineCore;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Gui;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace BigChess;

public class OpenPrompt : Prompt, IEarlyDrawHook
{
    private readonly Gui _gui;
    private readonly IRuntime _runtime;
    private readonly Widget _widget;
    private Action<SerializedBoard>? _bufferedCallback;
    private ScrollableArea _scrollableArea;

    public OpenPrompt(IRuntime runtime)
    {
        _runtime = runtime;
        _widget = new Widget(WindowRectangle(), Depth.Middle);
        _gui = new Gui();
        _scrollableArea = new ScrollableArea(_widget.Size, _widget.Size.ToRectangleF(), Depth.Front);
    }

    public override bool IsOpen => _bufferedCallback != null;

    public void EarlyDraw(Painter painter)
    {
        _gui.PrepareCanvases(painter, Constants.Theme);

        Client.Graphics.PushCanvas(_widget.Canvas);
        painter.BeginSpriteBatch(_scrollableArea.CanvasToScreen);
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

    private void Cancel()
    {
        _bufferedCallback = null;
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

    public void Request(Action<SerializedBoard> whenDone)
    {
        _bufferedCallback = whenDone;
        Refresh();
    }

    private void Refresh()
    {
        _gui.Clear();
        var buttonSize = new Vector2(500, 100);
        var boundaries = new RectangleF();

        var files = OpenPrompt.ScenariosFolder.GetFilesAt(".");
        var buttonRect = new RectangleF(Vector2.Zero, buttonSize);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            _gui.Button(buttonRect, fileInfo.Name, Depth.Middle, () => { OpenLevel(file); });
            buttonRect = buttonRect.Moved(new Vector2(0, buttonSize.Y + 20));
        }

        boundaries = RectangleF.Union(boundaries, buttonRect);
        _scrollableArea = new ScrollableArea(_widget.Size, boundaries, Depth.Front + 10);
    }

    private static IFileSystem ScenariosFolder => Client.Debug.RepoFileSystem.GetDirectory("Scenarios");

    private void OpenLevel(string path)
    {
        var json = ScenariosFolder.ReadFile(path);
        var result = JsonConvert.DeserializeObject<SerializedBoard>(json);

        if (result != null)
        {
            _bufferedCallback?.Invoke(result);
        }
        else
        {
            Client.Debug.LogWarning($"Failed to read {path}");
        }

        _bufferedCallback = null;
    }
}
