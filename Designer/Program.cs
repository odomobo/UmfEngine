using Microsoft.VisualBasic;
using NLog;
using SDL;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using UMFE.Framework;
using UmfEngine;

namespace Designer
{
    internal enum ProgramState
    {
        Normal,
        DraggingVertex,
    }

    internal class Program
    {
        // We're just gonna use 60 FPS because every computer can handle it, and I can't be bothered to separate game logic from rendering.
        // We're also disabling vsync, because in pathological situations, it can cause extreme lag.
        public const double TargetFps = 60;
        public static readonly TimeSpan TargetFrameTime = TimeSpan.FromSeconds(1 / TargetFps);

        private static Logger Logger;
        private static void ConfigureNLog()
        {
            LogManager.Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToConsole();
            });
            Logger = LogManager.GetCurrentClassLogger();
        }

        public static List<List<Vector2>> UndoHistory = new List<List<Vector2>>();
        public static ProgramState State = ProgramState.Normal;
        public static int VertexDragIndex;

        static void Main(string[] args)
        {
            ConfigureNLog();

            var engineConfig = new EngineConfiguration
            {
                HasFixedAspectRatio = true,
                FixedAspectRatio = 1.0f,
                DefaultResolution = new Vector2(1200, 1200),
                DefaultCursorVisible = true,
                DefaultVSync = false,
                DefaultFullscreen = false,
                ScreenSizeInUnits = 12,
            };

            using var engine = new Engine(engineConfig);

            var shape = new List<Vector2>
            {
                new Vector2(-0.5f, -0.5f),
                new Vector2( 0.5f, -0.5f),
                new Vector2( 0.5f,  0.5f),
                new Vector2(-0.5f,  0.5f),
            };

            var mouseCoordsText = new TextBlock(null, "", default, 0.4f);

            while (true)
            {
                // center the camera on origin
                var c = engine.GetCamera();
                c = c.GetTranslated(new Vector2(-6, -6));

                var input = engine.GetInput();
                if (input.ShuttingDown || input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_ESCAPE))
                    break;

                if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_F))
                    engine.ToggleFullscreen();

                Vector2 mousePositionClamped = GetMousePositionClamped(c, input);
                var mousePosition = input.GetMousePosition(c);

                var instructions = @"Left click to add or move vertex.
Right click to remove vertex.
Z to undo.
P to dump vertexes to stdout.";

                mouseCoordsText.SetText($"{mousePositionClamped.X: 0.00;-0.00; 0.00}, {mousePositionClamped.Y: 0.00;-0.00; 0.00}\n" + instructions);

                // logic here

                // undo
                if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_Z))
                {
                    if (UndoHistory.Any())
                    {
                        shape = UndoHistory.Last();
                        UndoHistory.RemoveAt(UndoHistory.Count - 1);
                    }
                }

                var (closestVertex, closestVertexIndex) = FindClosestVertexToMouse(shape, mousePosition);
                var (closestEdgeIndex, closestEdgeBegin, closestEdgeEnd) = FindClosestEdgeToMouse(shape, mousePosition);

                // if there's a closest vertex, then we can't have a closest edge
                if (closestVertexIndex != null)
                {
                    closestEdgeIndex = null;
                    closestEdgeBegin = null;
                    closestEdgeEnd = null;
                }

                var tmpState = State; // make a copy because it might change
                if (tmpState == ProgramState.Normal)
                {

                    // if there's a closest edge and we've clicked, then we need to split, and possibly start dragging the new vertex
                    if (closestEdgeIndex != null && input.WasMouseButtonPressed(MouseButton.Left))
                    {
                        Split(ref shape, closestEdgeIndex.Value); // this adds a copy to the history
                        State = ProgramState.DraggingVertex;
                        VertexDragIndex = closestEdgeIndex.Value + 1;
                    }
                    // if there's a closest vertex and we've clicked, start dragging it
                    else if (closestVertex != null && input.WasMouseButtonPressed(MouseButton.Left))
                    {
                        State = ProgramState.DraggingVertex;
                        VertexDragIndex = closestVertexIndex.Value;
                        UndoHistory.Add(shape.ToList()); // add a copy to the history
                    } 
                    // if there's a closest vertex and we've right clicked, delete it... but only if there are at least 3 vertexes
                    else if (closestVertex != null && input.WasMouseButtonPressed(MouseButton.Right) && shape.Count > 3)
                    {
                        UndoHistory.Add(shape.ToList()); // add a copy to the history
                        shape.RemoveAt(closestVertexIndex.Value);
                    }
                    else if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_P))
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("new Vector2[] {");
                        foreach (var vertex in shape)
                        {
                            sb.AppendLine($"    new Vector2({vertex.X}f, {vertex.Y}f),");
                        }
                        sb.AppendLine("}");
                        Console.Write(sb.ToString());
                    }
                    else if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_LEFT))
                    {
                        Shift(ref shape, new Vector2(-0.1f, 0));
                    }
                    else if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_RIGHT))
                    {
                        Shift(ref shape, new Vector2(0.1f, 0));
                    }
                    else if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_UP))
                    {
                        Shift(ref shape, new Vector2(0, -0.1f));
                    }
                    else if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_DOWN))
                    {
                        Shift(ref shape, new Vector2(0, 0.1f));
                    }
                    else if (input.WasKeyPressed(SDL_Scancode.SDL_SCANCODE_L))
                    {
                        ShapeLoader.LoadShape(ref shape);
                        // TODO: implement loading from data?????
                    }
                }
                else // DraggingVertex
                {
                    if (!input.IsMouseButtonDown(MouseButton.Left))
                    {
                        State = ProgramState.Normal;
                    }
                    else
                    {
                        shape[VertexDragIndex] = mousePositionClamped;
                    }
                }

                // draw calls here

                var t = new AffineTransformation();
                engine.ClearScreen();

                DrawGridLines(engine, c);
                //DrawMousePosition(engine, c, input, t, mousePositionClamped);

                // draw shape
                engine.DrawLinesClosed(c, t, 0.03f, Color.Red, shape.ToArray());

                // draw vertexes
                foreach (var vertex in shape)
                {
                    engine.DrawCircle(c, t, 0.05f, Color.DarkOrange, vertex);
                }

                // draw selection for closest vertex
                if (closestVertex != null)
                    engine.DrawCircle(c, t, 0.05f, Color.Yellow, closestVertex.Value);

                if (closestEdgeBegin != null)
                    engine.DrawLine(c, t, 0.05f, Color.Yellow, closestEdgeBegin.Value, closestEdgeEnd.Value);

                // draw ui
                var uiCamera = engine.GetCamera();
                mouseCoordsText.Draw(engine, uiCamera);

                engine.CompleteFrame(TargetFrameTime);
            }
        }

        private static void Shift(ref List<Vector2> shape, Vector2 adjust)
        {
            // make a copy of shape, because we're about to modify it!
            UndoHistory.Add(shape.ToList());

            for (int i = 0; i < shape.Count; i++)
            {
                shape[i] = shape[i] + adjust;
            }
        }

        private static void Split(ref List<Vector2> shape, int closestEdgeIndex)
        {
            // make a copy of shape, because we're about to modify it!
            UndoHistory.Add(shape.ToList());

            var beginVertex = shape[closestEdgeIndex];
            var endVertex = shape[(closestEdgeIndex + 1) % shape.Count];

            var midpointVertex = beginVertex + (endVertex - beginVertex) / 2;
            shape.Insert(closestEdgeIndex+1, midpointVertex);
        }

        private static (int? closestEdgeIndex, Vector2? closestEdgeBegin, Vector2? closestEdgeEnd) FindClosestEdgeToMouse(List<Vector2> shape, Vector2 mousePosition)
        {
            // find closest edge to mouse

            int? closestEdgeIndex = null;
            Vector2? closestEdgeBegin = default;
            Vector2? closestEdgeEnd = default;
            float closestEdgeDistance = float.MaxValue;

            for (int i = 0; i < shape.Count; i++)
            {
                var currentEdgeBegin = shape[i];
                var currentEdgeEnd = shape[(i + 1) % shape.Count];

                var currentEdgeDistance =
                    (currentEdgeBegin - mousePosition).Length()
                    + (currentEdgeEnd - mousePosition).Length()
                    - (currentEdgeBegin - currentEdgeEnd).Length();

                if (currentEdgeDistance < closestEdgeDistance)
                {
                    closestEdgeIndex = i;
                    closestEdgeBegin = currentEdgeBegin;
                    closestEdgeEnd = currentEdgeEnd;
                    closestEdgeDistance = currentEdgeDistance;
                }
            }

            if (closestEdgeDistance > 0.1f)
            {
                closestEdgeIndex = null;
                closestEdgeBegin = null;
                closestEdgeEnd = null;
            }

            return (closestEdgeIndex, closestEdgeBegin, closestEdgeEnd);
        }

        private static (Vector2? closestVertex, int? closestVertexIndex) FindClosestVertexToMouse(List<Vector2> shape, Vector2 mousePosition)
        {
            // find closest vertex index to mouse
            int? closestVertexIndex = null;
            Vector2? closestVertex = default;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < shape.Count; i++)
            {
                var currentVertex = shape[i];
                var currentDistance = (currentVertex - mousePosition).Length();
                if (currentDistance < closestDistance)
                {
                    closestVertexIndex = i;
                    closestVertex = currentVertex;
                    closestDistance = currentDistance;
                }
            }

            if (closestDistance > 0.1f)
            {
                closestVertexIndex = null;
                closestVertex = null;
            }
            return (closestVertex, closestVertexIndex);
        }

        private static void DrawMousePosition(Engine engine, Camera c, Input input, AffineTransformation t, Vector2 mousePosition)
        {
            engine.DrawCircle(c, t, 0.05f, Color.LightGray, mousePosition);
        }

        private static Vector2 GetMousePositionClamped(Camera c, Input input)
        {
            var mousePosition = input.GetMousePosition(c);

            // clamp to nearest gridpoint, unless lalt is pressed
            if (!input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LALT))
                mousePosition = new Vector2(MathF.Round(mousePosition.X * 10) / 10, MathF.Round(mousePosition.Y * 10) / 10);
            return mousePosition;
        }

        private const int MinScreenRange = -6;
        private const int MaxScreenRange = 6;
        private static void DrawGridLines(Engine engine, Camera c)
        {
            AffineTransformation t = new AffineTransformation();
            // draw main grid lines
            for (int i = MinScreenRange; i <= MaxScreenRange; i++)
            {
                var gridLineColor = Color.FromArgb(70, Color.DarkGray);
                engine.DrawLine(c, t, 0, gridLineColor, i, MinScreenRange, i, MaxScreenRange);
                engine.DrawLine(c, t, 0, gridLineColor, MinScreenRange, i, MaxScreenRange, i);
            }

            // draw sub grid lines
            for (float i = MinScreenRange; i <= MaxScreenRange; i += 0.1f)
            {
                var gridLineColor = Color.FromArgb(70, Color.DarkSlateGray);
                engine.DrawLine(c, t, 0, gridLineColor, i, MinScreenRange, i, MaxScreenRange);
                engine.DrawLine(c, t, 0, gridLineColor, MinScreenRange, i, MaxScreenRange, i);
            }

            // draw origin grid lines
            var originGridLineColor = Color.FromArgb(50, Color.DarkGray);
            engine.DrawLine(c, t, 0, originGridLineColor, 0, MinScreenRange, 0, MaxScreenRange);
            engine.DrawLine(c, t, 0, originGridLineColor, MinScreenRange, 0, MaxScreenRange, 0);
        }
    }
}
