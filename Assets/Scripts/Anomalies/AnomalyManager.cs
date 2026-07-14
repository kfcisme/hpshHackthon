using System;
using System.Collections.Generic;
using GlitchCompiler.Core;
using GlitchCompiler.Data;
using GlitchCompiler.Level;
using GlitchCompiler.VCode;
using UnityEngine;
namespace GlitchCompiler.Anomalies
{
    public sealed class AnomalyManager : MonoBehaviour
    {
        private readonly Dictionary<AnomalyType, IAnomaly> catalog = new Dictionary<AnomalyType, IAnomaly>();
        private readonly HashSet<AnomalyType> triggeredOnce = new HashSet<AnomalyType>();
        private IAnomaly active;
        private float nextTrigger;
        private bool shield;
        private bool reset;
        private bool isPaused;
        private AnomalyContext context;

        public bool HasActiveAnomaly => active != null;

        public void Initialize(AnomalyContext value)
        {
            CleanupActive();
            context = value ?? throw new ArgumentNullException(nameof(value));
            catalog.Clear();
            triggeredOnce.Clear();
            nextTrigger = 0f;
            shield = false;
            reset = false;
            isPaused = false;
            catalog[AnomalyType.GhostComment] = new GhostCommentAnomaly();
            catalog[AnomalyType.SyntaxShift] = new SyntaxShiftAnomaly();
            catalog[AnomalyType.CanvasMask] = new CanvasMaskAnomaly();
            catalog[AnomalyType.ControlInversion] = new ControlInversionAnomaly();
            context.ShieldEnabled = () => shield;
            context.ResetReceived = () => reset;
        }

        public void AcceptSystemCommands(IEnumerable<SystemCommand> commands)
        {
            shield = false;
            reset = false;
            if (commands == null) return;
            foreach (var command in commands)
            {
                if (command.Type == SystemCommandType.Shield) shield = command.BoolValue;
                if (command.Type == SystemCommandType.Reset) reset = true;
            }
        }

        public void SetPaused(bool value) => isPaused = value;

        public void EndSession()
        {
            isPaused = true;
            CleanupActive();
        }

        public void TryTrigger(IReadOnlyList<AnomalyRule> rules, LevelPhase phase)
        {
            if (isPaused || context == null || rules == null || active != null || Time.time < nextTrigger) return;
            foreach (var rule in rules)
            {
                if (rule == null || !rule.Enabled || (rule.TriggerOnce && triggeredOnce.Contains(rule.Type))) continue;
                if ((int)rule.EarliestPhase > (int)phase || UnityEngine.Random.value > rule.TriggerChance) continue;
                if (!catalog.TryGetValue(rule.Type, out active)) continue;
                context.ActiveRule = rule;
                active.OnTrigger(context);
                if (rule.TriggerOnce) triggeredOnce.Add(rule.Type);
                nextTrigger = Time.time + Mathf.Max(0f, rule.CooldownSeconds);
                ApplicationBootstrap.Events?.Publish(new AnomalyTriggered(active.Id));
                break;
            }
        }

        private void Update()
        {
            if (isPaused || active == null || !active.CheckResolved()) return;
            var resolved = active;
            active.OnResolve();
            if (context.ActiveRule != null) context.AddTime(context.ActiveRule.ResolveBonusSeconds);
            context.ActiveRule = null;
            active = null;
            ApplicationBootstrap.Events?.Publish(new AnomalyResolved(resolved.Id));
        }

        private void OnDestroy() => CleanupActive();

        private void CleanupActive()
        {
            if (active == null) return;
            active.OnCleanup();
            active = null;
            if (context != null) context.ActiveRule = null;
        }
    }
}
