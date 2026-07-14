using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlitchCompiler.Rendering
{
    /// <summary>
    /// Executes drawing commands into an in-memory pixel buffer. Keeping this
    /// class independent from MonoBehaviour makes rendering deterministic and
    /// directly testable in EditMode.
    /// </summary>
    public sealed class TurtleRasterizer
    {
        private const int MaxCircleSegments = 4096;
        private const int OutLeft = 1;
        private const int OutRight = 2;
        private const int OutBottom = 4;
        private const int OutTop = 8;

        private readonly Color32[] pixels;
        private TurtleState state;

        public TurtleRasterizer(int resolution)
        {
            if (resolution < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(resolution));
            }

            Resolution = resolution;
            pixels = new Color32[resolution * resolution];
        }

        public int Resolution { get; }
        public Color32[] Pixels => pixels;
        public TurtleState State => state;

        // V-Code uses Cartesian coordinates. (0, 0) is the geometric center
        // of the canvas; on an even-sized 64x64 texture it lies between the
        // four central pixel centers.
        private float CanvasCenter => (Resolution - 1) * 0.5f;

        public void Render(IReadOnlyList<DrawCommand> commands)
        {
            Array.Clear(pixels, 0, pixels.Length);
            state = TurtleState.CreateDefault(Resolution);

            if (commands == null)
            {
                return;
            }

            foreach (var command in commands)
            {
                Apply(command);
            }
        }

        private void Apply(DrawCommand command)
        {
            switch (command.Type)
            {
                case DrawCommandType.Color:
                    state.Color = command.Color;
                    break;

                case DrawCommandType.Width:
                    state.Width = Mathf.Max(1, Mathf.RoundToInt(command.A));
                    break;

                case DrawCommandType.Turn:
                    state.Angle = Mathf.Repeat(state.Angle + command.A, 360f);
                    break;

                case DrawCommandType.Move:
                    var next = state.Position + Direction(state.Angle) * command.A;
                    DrawLine(state.Position, next, state.Color, state.Width);
                    state.Position = next;
                    break;

                case DrawCommandType.Circle:
                    DrawCircle(state.Position, command.A, state.Color, state.Width);
                    break;

                case DrawCommandType.Rect:
                    DrawRectangle(state.Position, command.A, command.B, state.Color, state.Width);
                    break;
            }
        }

        private static Vector2 Direction(float angle) => new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad));

        private void DrawRectangle(Vector2 origin, float width, float height, Color color, int strokeWidth)
        {
            var a = origin;
            var b = a + new Vector2(width, 0);
            var c = b + new Vector2(0, height);
            var d = a + new Vector2(0, height);
            DrawLine(a, b, color, strokeWidth);
            DrawLine(b, c, color, strokeWidth);
            DrawLine(c, d, color, strokeWidth);
            DrawLine(d, a, color, strokeWidth);
        }

        private void DrawCircle(Vector2 center, float radius, Color color, int strokeWidth)
        {
            radius = Mathf.Abs(radius);
            var brushRadius = strokeWidth * 0.5f;
            if (!CircleCanTouchCanvas(center, radius, brushRadius))
            {
                return;
            }

            var segments = Mathf.Clamp(Mathf.CeilToInt(2f * Mathf.PI * radius), 12, MaxCircleSegments);
            var previous = center + new Vector2(radius, 0);
            for (var index = 1; index <= segments; index++)
            {
                var angle = index * Mathf.PI * 2f / segments;
                var point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                DrawLine(previous, point, color, strokeWidth);
                previous = point;
            }
        }

        private bool CircleCanTouchCanvas(Vector2 center, float radius, float brushRadius)
        {
            var min = -CanvasCenter;
            var max = CanvasCenter;
            var closest = new Vector2(
                Mathf.Clamp(center.x, min, max),
                Mathf.Clamp(center.y, min, max));
            var farthest = new Vector2(
                center.x < 0f ? max : min,
                center.y < 0f ? max : min);
            var minimumDistance = Vector2.Distance(center, closest);
            var maximumDistance = Vector2.Distance(center, farthest);
            return radius + brushRadius >= minimumDistance && radius - brushRadius <= maximumDistance;
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, int strokeWidth)
        {
            var brushRadius = strokeWidth * 0.5f;
            if (!TryClipLine(ref start, ref end, brushRadius))
            {
                return;
            }

            var steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(start, end) * 2f));
            for (var index = 0; index <= steps; index++)
            {
                DrawBrush(Vector2.Lerp(start, end, index / (float)steps), brushRadius, color);
            }
        }

        private bool TryClipLine(ref Vector2 start, ref Vector2 end, float margin)
        {
            var min = -CanvasCenter - margin;
            var max = CanvasCenter + margin;
            var startCode = OutCode(start, min, max);
            var endCode = OutCode(end, min, max);

            for (var attempts = 0; attempts < 8; attempts++)
            {
                if ((startCode | endCode) == 0)
                {
                    return true;
                }

                if ((startCode & endCode) != 0)
                {
                    return false;
                }

                var outsideCode = startCode != 0 ? startCode : endCode;
                var delta = end - start;
                Vector2 intersection;
                if ((outsideCode & OutTop) != 0)
                {
                    intersection = new Vector2(start.x + delta.x * (max - start.y) / delta.y, max);
                }
                else if ((outsideCode & OutBottom) != 0)
                {
                    intersection = new Vector2(start.x + delta.x * (min - start.y) / delta.y, min);
                }
                else if ((outsideCode & OutRight) != 0)
                {
                    intersection = new Vector2(max, start.y + delta.y * (max - start.x) / delta.x);
                }
                else
                {
                    intersection = new Vector2(min, start.y + delta.y * (min - start.x) / delta.x);
                }

                if (outsideCode == startCode)
                {
                    start = intersection;
                    startCode = OutCode(start, min, max);
                }
                else
                {
                    end = intersection;
                    endCode = OutCode(end, min, max);
                }
            }

            return false;
        }

        private static int OutCode(Vector2 point, float min, float max)
        {
            var code = 0;
            if (point.x < min) code |= OutLeft;
            else if (point.x > max) code |= OutRight;
            if (point.y < min) code |= OutBottom;
            else if (point.y > max) code |= OutTop;
            return code;
        }

        private void DrawBrush(Vector2 center, float radius, Color color)
        {
            center += new Vector2(CanvasCenter, CanvasCenter);
            var minX = Mathf.Max(0, Mathf.CeilToInt(center.x - radius));
            var maxX = Mathf.Min(Resolution - 1, Mathf.FloorToInt(center.x + radius));
            var minY = Mathf.Max(0, Mathf.CeilToInt(center.y - radius));
            var maxY = Mathf.Min(Resolution - 1, Mathf.FloorToInt(center.y + radius));
            var radiusSquared = radius * radius;
            var color32 = (Color32)color;

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var offsetX = x - center.x;
                    var offsetY = y - center.y;
                    if (offsetX * offsetX + offsetY * offsetY <= radiusSquared)
                    {
                        pixels[y * Resolution + x] = color32;
                    }
                }
            }
        }
    }
}
