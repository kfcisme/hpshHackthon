using GlitchCompiler.Anomalies;
using GlitchCompiler.Core;
using GlitchCompiler.Data;
using GlitchCompiler.Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlitchCompiler.UI
{
    /// <summary>
    /// Scene-only adapter that connects the Level session to its UI. It owns no
    /// game rules; it supplies UI callbacks to anomalies and renders EventBus
    /// messages for the player.
    /// </summary>
    public sealed class LevelScenePresenter : MonoBehaviour
    {
        [SerializeField] private LevelSessionController session;
        [SerializeField] private LevelTimer timer;
        [SerializeField] private IDEEditorController editor;
        [SerializeField] private HudController hud;
        [SerializeField] private ResultsPanelController results;
        [SerializeField] private AnomalyOverlayController anomalyOverlay;
        [SerializeField] private VCodeErrorPanel errors;
        [SerializeField] private TMP_Text tutorial;
        [SerializeField] private RawImage targetPreview;

        private bool subscribed;

        private void OnEnable() => Subscribe();

        private void Start()
        {
            Subscribe();
            ConfigureAnomalies();
            ApplyLevelPresentation(session == null ? null : session.CurrentLevel);
        }

        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (subscribed || ApplicationBootstrap.Events == null) return;
            subscribed = true;
            ApplicationBootstrap.Events.Subscribe<LevelStarted>(OnLevelStarted);
            ApplicationBootstrap.Events.Subscribe<TimerChanged>(OnTimerChanged);
            ApplicationBootstrap.Events.Subscribe<MatchChanged>(OnMatchChanged);
            ApplicationBootstrap.Events.Subscribe<CompilationFinished>(OnCompilationFinished);
            ApplicationBootstrap.Events.Subscribe<LevelFinished>(OnLevelFinished);
        }

        private void Unsubscribe()
        {
            if (!subscribed || ApplicationBootstrap.Events == null) return;
            ApplicationBootstrap.Events.Unsubscribe<LevelStarted>(OnLevelStarted);
            ApplicationBootstrap.Events.Unsubscribe<TimerChanged>(OnTimerChanged);
            ApplicationBootstrap.Events.Unsubscribe<MatchChanged>(OnMatchChanged);
            ApplicationBootstrap.Events.Unsubscribe<CompilationFinished>(OnCompilationFinished);
            ApplicationBootstrap.Events.Unsubscribe<LevelFinished>(OnLevelFinished);
            subscribed = false;
        }

        private void ConfigureAnomalies()
        {
            if (session == null) return;
            session.ConfigureAnomalyContext(new AnomalyContext
            {
                ReadCode = () => editor == null ? string.Empty : editor.Code,
                WriteCode = code =>
                {
                    if (editor == null) return;
                    editor.Code = code;
                    editor.NotifyChanged();
                },
                ShowOverlay = (title, instruction) => anomalyOverlay?.Show(title, instruction),
                HideOverlay = () => anomalyOverlay?.Hide(),
                AddTime = seconds => timer?.Add(seconds),
                SetTimerMultiplier = multiplier => timer?.SetMultiplier(multiplier)
            });
        }

        private void OnLevelStarted(LevelStarted message) => ApplyLevelPresentation(session == null ? null : session.CurrentLevel);

        private void ApplyLevelPresentation(LevelDefinition level)
        {
            if (level == null) return;
            if (editor != null)
            {
                editor.Code = level.StarterCode ?? string.Empty;
                editor.NotifyChanged();
            }

            if (tutorial != null) tutorial.text = level.Tutorial ?? string.Empty;
            if (targetPreview != null) targetPreview.texture = level.TargetImage;
            hud?.SetMatch(0f);
            results?.Hide();
            anomalyOverlay?.Hide();
        }

        private void OnTimerChanged(TimerChanged message) => hud?.SetTimer(message.Remaining);
        private void OnMatchChanged(MatchChanged message) => hud?.SetMatch(message.Percentage);
        private void OnCompilationFinished(CompilationFinished message) => errors?.Show(message.Diagnostics);

        private void OnLevelFinished(LevelFinished message)
        {
            var finalMatch = message.Success || session == null ? message.Match : session.CurrentMatch;
            results?.Show(message.Success, finalMatch);
        }
    }
}
