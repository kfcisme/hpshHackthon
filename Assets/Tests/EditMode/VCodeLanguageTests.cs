using System.Collections.Generic;
using GlitchCompiler.Anomalies;
using GlitchCompiler.Data;
using GlitchCompiler.Level;
using GlitchCompiler.Rendering;
using GlitchCompiler.VCode;
using NUnit.Framework;
using UnityEngine;

namespace GlitchCompiler.Tests
{
    public sealed class VCodeLanguageTests
    {
        [Test]
        public void ParsesAndExecutesRecursiveFunction()
        {
            const string code = "FUNC line(n) { IF(n <= 0) { MOVE(1); } ELSE { line(n - 1); } } line(2);";

            var parsed = new VCodeParser().Parse(code);

            Assert.That(parsed.Success, Is.True);
            var result = new VCodeInterpreter().Execute(parsed.Program);
            Assert.That(result.Success, Is.True);
            Assert.That(result.DrawCommands.Count, Is.EqualTo(1));
            Assert.That(result.DrawCommands[0].Type, Is.EqualTo(DrawCommandType.Move));
        }

        [Test]
        public void TurnsPlayerInstructionsIntoDrawCommands()
        {
            const string code = "COLOR(\"#FF0000\"); WIDTH(4); MOVE(100); TURN(90); CIRCLE(20); RECT(40, 30);";

            var parsed = new VCodeParser().Parse(code);
            var result = new VCodeInterpreter().Execute(parsed.Program);

            Assert.That(parsed.Success, Is.True);
            Assert.That(result.Success, Is.True);
            Assert.That(result.DrawCommands, Has.Count.EqualTo(6));
            Assert.That(result.DrawCommands[0].Type, Is.EqualTo(DrawCommandType.Color));
            Assert.That(result.DrawCommands[1].Type, Is.EqualTo(DrawCommandType.Width));
            Assert.That(result.DrawCommands[2].Type, Is.EqualTo(DrawCommandType.Move));
            Assert.That(result.DrawCommands[2].A, Is.EqualTo(100));
            Assert.That(result.DrawCommands[3].Type, Is.EqualTo(DrawCommandType.Turn));
            Assert.That(result.DrawCommands[3].A, Is.EqualTo(90));
            Assert.That(result.DrawCommands[4].Type, Is.EqualTo(DrawCommandType.Circle));
            Assert.That(result.DrawCommands[5].Type, Is.EqualTo(DrawCommandType.Rect));
            Assert.That(result.DrawCommands[5].A, Is.EqualTo(40));
            Assert.That(result.DrawCommands[5].B, Is.EqualTo(30));
        }

        [Test]
        public void AllowsVariablesAndLoopsInDrawingInstructions()
        {
            const string code = "LET side = 25; COLOR(\"blue\"); LOOP(4) { MOVE(side); TURN(90); }";

            var parsed = new VCodeParser().Parse(code);
            var result = new VCodeInterpreter().Execute(parsed.Program);

            Assert.That(parsed.Success, Is.True);
            Assert.That(result.Success, Is.True);
            Assert.That(result.DrawCommands, Has.Count.EqualTo(9));
            Assert.That(result.DrawCommands[0].Type, Is.EqualTo(DrawCommandType.Color));
            Assert.That(result.DrawCommands[1].A, Is.EqualTo(25));
            Assert.That(result.DrawCommands[8].Type, Is.EqualTo(DrawCommandType.Turn));
        }

        [TestCase("MOVE();")]
        [TestCase("RECT(10);")]
        [TestCase("COLOR(123);")]
        [TestCase("WIDTH(0);")]
        [TestCase("CIRCLE(-1);")]
        [TestCase("SHIELD(1);")]
        public void RejectsInvalidCommandArguments(string code)
        {
            var parsed = new VCodeParser().Parse(code);
            var result = new VCodeInterpreter().Execute(parsed.Program);

            Assert.That(parsed.Success, Is.True);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Diagnostics, Is.Not.Empty);
        }

        [TestCase("MOVE(1)")]
        [TestCase("LOOP(2) { MOVE(1);")]
        [TestCase("@")]
        public void RejectsMalformedSyntax(string code)
        {
            var result = new VCodeParser().Parse(code);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Diagnostics, Is.Not.Empty);
        }

        [TestCase("MOVE(missing);")]
        [TestCase("MOVE(1 / 0);")]
        [TestCase("FUNC line(length) { MOVE(length); } line();")]
        public void RejectsRuntimeFailures(string code)
        {
            var parsed = new VCodeParser().Parse(code);
            var result = new VCodeInterpreter().Execute(parsed.Program);

            Assert.That(parsed.Success, Is.True);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Diagnostics, Is.Not.Empty);
        }

        [Test]
        public void RejectsAnEmptyLoopThatExceedsTheInstructionLimit()
        {
            var parsed = new VCodeParser().Parse("LOOP(10001) { }");
            var result = new VCodeInterpreter().Execute(parsed.Program);

            Assert.That(parsed.Success, Is.True);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Diagnostics, Is.Not.Empty);
        }

        [Test]
        public void EmitsSystemCommands()
        {
            var parsed = new VCodeParser().Parse("SHIELD(true); SYSTEM.RESET();");
            var result = new VCodeInterpreter().Execute(parsed.Program);

            Assert.That(parsed.Success, Is.True);
            Assert.That(result.Success, Is.True);
            Assert.That(result.SystemCommands, Has.Count.EqualTo(2));
            Assert.That(result.SystemCommands[0].Type, Is.EqualTo(SystemCommandType.Shield));
            Assert.That(result.SystemCommands[0].BoolValue, Is.True);
            Assert.That(result.SystemCommands[1].Type, Is.EqualTo(SystemCommandType.Reset));
        }

        [Test]
        public void RejectsUnterminatedString()
        {
            var result = new VCodeParser().Parse("COLOR(\"#FFFFFF);");

            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void CentersTheTurtleForAnyCanvasResolution()
        {
            var rasterizer = new TurtleRasterizer(300);

            rasterizer.Render(new List<DrawCommand>());

            Assert.That(rasterizer.State.Position.x, Is.EqualTo(0));
            Assert.That(rasterizer.State.Position.y, Is.EqualTo(0));
        }

        [Test]
        public void RasterizesShapesAndClipsLinesOutsideTheCanvas()
        {
            var rasterizer = new TurtleRasterizer(64);
            var commands = new List<DrawCommand>
            {
                new DrawCommand(DrawCommandType.Color, color: UnityEngine.Color.white),
                new DrawCommand(DrawCommandType.Width, 1),
                new DrawCommand(DrawCommandType.Move, 10),
                new DrawCommand(DrawCommandType.Circle, 6),
                new DrawCommand(DrawCommandType.Move, 10_000),
                new DrawCommand(DrawCommandType.Turn, 180),
                new DrawCommand(DrawCommandType.Move, 10_000)
            };

            Assert.DoesNotThrow(() => rasterizer.Render(commands));
            Assert.That(rasterizer.Pixels[32 * 64 + 42].a, Is.GreaterThan(0));
            Assert.That(rasterizer.Pixels[32 * 64 + 48].a, Is.GreaterThan(0));
            Assert.That(rasterizer.State.Position.x, Is.EqualTo(10).Within(0.01f));
            Assert.That(rasterizer.State.Position.y, Is.EqualTo(0).Within(0.01f));
        }

        [Test]
        public void PenalizesPixelsDrawnOutsideTheTarget()
        {
            var target = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var rendered = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            try
            {
                target.SetPixel(0, 0, Color.white);
                target.Apply();
                rendered.SetPixel(0, 0, Color.white);
                rendered.SetPixel(1, 0, Color.white);
                rendered.Apply();

                Assert.That(PixelMatchEvaluator.Evaluate(rendered, target), Is.EqualTo(50f));
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(rendered);
            }
        }

        [Test]
        public void SyntaxShiftCanTriggerForLowercaseTurn()
        {
            var code = "turn(90);";
            var anomaly = new SyntaxShiftAnomaly();
            var context = new AnomalyContext
            {
                ReadCode = () => code,
                WriteCode = value => code = value,
                ShowOverlay = (_, __) => { },
                HideOverlay = () => { }
            };

            Assert.That(anomaly.CanTrigger(context), Is.True);
            anomaly.OnTrigger(context);
            Assert.That(code, Is.EqualTo("BURN(90);"));
            Assert.That(anomaly.CheckResolved(), Is.False);
        }

        [Test]
        public void DoesNotCompleteAConfigurationWithoutATarget()
        {
            var gameObject = new GameObject("CompletionEvaluatorTest");
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            try
            {
                level.PassPercentage = 1f;
                var evaluator = gameObject.AddComponent<LevelCompletionEvaluator>();

                Assert.That(evaluator.IsComplete(level, 100f), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(level);
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
