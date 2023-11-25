using System;
using System.Collections.Generic;
using System.Linq;
using ChessCommon;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExplogineMonoGame.Layout;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BigChess;

public class PromotionPrompt : Prompt
{
    private const int ButtonSize = 128;
    private readonly Assets _assets;
    private readonly bool _canBeClosed;
    private readonly ChessGameState _gameState;
    private readonly LayoutArrangement _layout;

    /// <summary>
    ///     Names of all pieces that can be promoted to
    /// </summary>
    private readonly List<string> _pieceNames;

    private readonly IRuntime _runtime;
    private Action<PieceType>? _bufferedCallback;
    private PieceType? _hoveredButton;
    private PieceType? _primedButton;

    public PromotionPrompt(ChessGameState gameState, IRuntime runtime, Assets assets,
        bool canBeClosed, List<string> pieceNames)
    {
        _pieceNames = pieceNames;
        _gameState = gameState;
        _runtime = runtime;
        _assets = assets;
        _canBeClosed = canBeClosed;

        var root = new LayoutBuilder(new Style {PaddingBetweenElements = 10, Alignment = Alignment.Center});

        for (var i = 0; i < _pieceNames.Count; i++)
        {
            var name = _pieceNames[i];
            root.Add(L.FixedElement(name, PromotionPrompt.ButtonSize, PromotionPrompt.ButtonSize));

            if (i != _pieceNames.Count - 1)
            {
                root.Add(L.FillBoth());
            }
        }

        _layout = root.Bake(
            new Vector2(PromotionPrompt.ButtonSize * (_pieceNames.Count + 1), PromotionPrompt.ButtonSize)
                .ToRectangleF());
    }

    public override bool IsOpen => _bufferedCallback != null;

    protected override void DrawInternal(Painter painter)
    {
        painter.BeginSpriteBatch();
        foreach (var name in _pieceNames)
        {
            var itemRectangle = GetLayoutRectangle(name);
            var itemType = NameToEnum(name);

            var inflateAmount = new Vector2();

            if (_primedButton == itemType && _hoveredButton == itemType)
            {
                inflateAmount = -new Vector2(PromotionPrompt.ButtonSize) / 32f;
            }

            itemRectangle = itemRectangle.Inflated(inflateAmount);

            painter.DrawRectangle(LayoutRootRectangle().Inflated(50, 50),
                new DrawSettings {Depth = Depth.Back, Color = Color.DarkGreen.DesaturatedBy(0.5f)});
            painter.DrawRectangle(itemRectangle,
                new DrawSettings
                {
                    Depth = Depth.Middle, Color = _hoveredButton == itemType ? Color.White : Color.Azure.DimmedBy(0.25f)
                });

            if (itemType.HasValue)
            {
                var frame = Constants.FrameIndex(itemType.Value, _gameState.CurrentTurn);

                _assets.GetAsset<SpriteSheet>("Pieces").DrawFrameAsRectangle(painter, frame,
                    itemRectangle,
                    new DrawSettings {Depth = Depth.Middle - 50});
            }
        }

        painter.EndSpriteBatch();
    }

    public override void Cancel()
    {
        _bufferedCallback = null;
    }

    public override void Update(float dt)
    {
    }

    protected override void UpdateInputInternal(ConsumableInput input, HitTestStack overlayLayer)
    {
        if (BackgroundHover && input.Mouse.WasAnyButtonPressedOrReleased() && _canBeClosed)
        {
            _bufferedCallback = null;
        }

        foreach (var name in _pieceNames)
        {
            var rectangle = GetLayoutRectangle(name);
            overlayLayer.AddZone(rectangle, Depth.Middle,
                () => { _hoveredButton = null; },
                () =>
                {
                    var type = NameToEnum(name);
                    if (!Client.Input.Mouse.IsAnyButtonDown() || _primedButton == type)
                    {
                        _hoveredButton = type;
                    }
                });
        }

        if (input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            if (_primedButton != null && _primedButton == _hoveredButton)
            {
                Execute(_primedButton.Value);
            }

            _primedButton = null;
        }

        if (_hoveredButton != null && input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            _primedButton = _hoveredButton;
        }

        var keys = new[] {Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9};
        for (var i = 0; i < keys.Length; i++)
        {
            var key = keys[i];

            if (input.Keyboard.GetButton(key, true).WasPressed)
            {
                if (_pieceNames.IsValidIndex(i))
                {
                    var pieceType = NameToEnum(_pieceNames[i]);

                    if (pieceType.HasValue)
                    {
                        Execute(pieceType.Value);
                    }
                }
            }
        }
    }

    private void Execute(PieceType value)
    {
        _bufferedCallback?.Invoke(value);
        _bufferedCallback = null;
    }

    private PieceType? NameToEnum(string targetName)
    {
        // this is so stupid, if we support custom pieces, PieceType will stop being an enum and a lot of this goes away
        var i = 0;
        foreach (var pieceName in Enum.GetNames<PieceType>())
        {
            if (pieceName == targetName)
            {
                break;
            }

            i++;
        }

        var values = Enum.GetValues<PieceType>().ToList();
        if (values.IsValidIndex(i))
        {
            return values[i];
        }

        return null;
    }

    private RectangleF GetLayoutRectangle(string name)
    {
        var rootRect = LayoutRootRectangle();
        return _layout.FindElement(name).Rectangle.Moved(rootRect.TopLeft);
    }

    private RectangleF LayoutRootRectangle()
    {
        return RectangleF.FromSizeAlignedWithin(_runtime.Window.RenderResolution.ToRectangleF(),
            _layout.UsedSpace.Size, Alignment.Center);
    }

    public void Request(Action<PieceType> onResponse)
    {
        _bufferedCallback = onResponse;
    }
}
