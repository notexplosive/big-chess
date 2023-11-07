using System.Collections.Generic;
using ExplogineMonoGame;
using Microsoft.Xna.Framework;

namespace BigChess;

public class AnimatedObjectCollection
{
    private readonly List<AnimatedObject> _content = new();

    public T Add<T>(T result) where T : AnimatedObject
    {
        _content.Add(result);
        return result;
    }

    public void UpdateAll(float dt)
    {
        foreach (var item in _content)
        {
            item.Update(dt);
        }

        for (var i = _content.Count - 1; i >= 0; i--)
        {
            var item = _content[i];
            if (item.ShouldDestroy)
            {
                _content.Remove(item);
            }
        }
    }

    public void DrawAll(Painter painter, Matrix canvasToScreen)
    {
        painter.BeginSpriteBatch(canvasToScreen);
        foreach (var item in _content)
        {
            if (item.Visible)
            {
                item.DrawScaled(painter);
            }
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch();
        foreach (var item in _content)
        {
            if (item.Visible)
            {
                item.DrawUnscaled(painter, canvasToScreen);
            }
        }

        painter.EndSpriteBatch();
    }
}
