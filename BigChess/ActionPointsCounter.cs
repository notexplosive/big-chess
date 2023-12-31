﻿using System;
using ChessCommon;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Layout;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ActionPointsCounter : AnimatedObject
{
    private readonly ChessGameState _gameState;
    private readonly IRuntime _runtime;
    private readonly BoardData _boardData;
    private readonly SequenceTween _tween = new();
    private LayoutArrangement _layout = null!;
    private Cell[] _tweenableCells = null!;
    private PieceColor _currentColor;

    private class Cell
    {
        public TweenableFloat FillScale = new(1);
    }
    
    public ActionPointsCounter(ChessGameState gameState, IRuntime runtime, BoardData boardData)
    {
        _gameState = gameState;
        _runtime = runtime;
        _boardData = boardData;
        _currentColor = _gameState.CurrentTurn;
        
        boardData.Changed += RebuildCellsArray;
        gameState.ActionCompleted += OnActionCompleted;
        gameState.TurnChanged += OnTurnChanged;
        
        RebuildCellsArray(boardData);
    }

    private void RebuildCellsArray(BoardData boardData)
    {
        _tweenableCells = new Cell[boardData.NumberOfActionPoints];
        
        for (var index = 0; index < _tweenableCells.Length; index++)
        {
            _tweenableCells[index] = new();
        }
        
        _layout = ComputeLayout();
    }

    private void OnTurnChanged(PieceColor newColor)
    {
        _tween.Add(new CallbackTween(() =>
        {
            _currentColor = newColor;
        }));
        
        foreach (var cell in _tweenableCells)
        {
            _tween.Add(cell.FillScale.TweenTo(1.1f, 0.05f, Ease.CubicFastSlow));
            _tween.Add(cell.FillScale.TweenTo(1f, 0.1f, Ease.CubicSlowFast));
        }
    }

    private void OnActionCompleted()
    {
        var cell = _tweenableCells[_gameState.PendingActionPoints-1];
        _tween.Add(cell.FillScale.TweenTo(1.1f, 0.05f, Ease.CubicFastSlow));
        _tween.Add(cell.FillScale.TweenTo(0f, 0.25f, Ease.CubicSlowFast));
    }

    public override void DrawScaled(Painter painter)
    {
        if (_boardData.NumberOfActionPoints <= 1)
        {
            return;
        }

        var fillColor = Constants.PieceColorToRgb(_currentColor);
        var lineColor = Constants.PieceColorToRgb(Constants.OppositeColor(_currentColor));
        var cellIndex = 0;
        foreach (var layoutCell in _layout.FindRectangles("Cell"))
        {
            var cell = _tweenableCells[cellIndex];
            painter.DrawRectangle(RectangleF.FromCenterAndSize(layoutCell.Center, layoutCell.Size * cell.FillScale), new DrawSettings
            {
                Depth = Depth.Middle,
                Color = fillColor
            });
            painter.DrawLineRectangle(RectangleF.FromCenterAndSize(layoutCell.Center, layoutCell.Size * MathF.Max(cell.FillScale, 1f)), new LineDrawSettings
            {
                Depth = Depth.Middle - 100,
                Thickness = 5f,
                Color = lineColor
            });
            cellIndex++;
        }
    }

    private LayoutArrangement ComputeLayout()
    {
        var size = _runtime.Window.RenderResolution.ToVector2() / 2;
        var screenRectangle = RectangleF.InflateFrom(size, size.X - 50, size.Y - 50);
        var cellSize = 80;
        var barWidth = cellSize * _boardData.NumberOfActionPoints;
        var barPosition = new Vector2(screenRectangle.X, screenRectangle.Y);
        var barRectangle = new RectangleF(barPosition, new Vector2(screenRectangle.Width, cellSize));

        barRectangle = barRectangle.Inflated(20, 0);

        var root = new LayoutBuilder(new Style{Alignment = Alignment.Center, Orientation = Orientation.Horizontal});

        for (int i = 0; i < _boardData.NumberOfActionPoints; i++)
        {
            root.Add(L.FillVertical("Cell", cellSize));
            if (i < _boardData.NumberOfActionPoints - 1)
            {
                root.Add(L.FixedElement(20,0));
            }
        }

        return root.Bake(barRectangle);
    }

    public override void DrawUnscaled(Painter painter, Matrix canvasToScreen)
    {
        
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);

        if (_tween.IsDone())
        {
            _tween.Clear();
        }
    }
}
