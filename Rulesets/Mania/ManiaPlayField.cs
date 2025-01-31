using System.Linq;
using Rubicon.Core.Chart;
using Rubicon.Core.Data;
using Rubicon.Core.Meta;
using Rubicon.Core.Settings;

namespace Rubicon.Core.Rulesets.Mania;

/// <summary>
/// A <see cref="PlayField"/> class with Mania-related gameplay incorporated. Also the main mode for Rubicon Engine.
/// </summary>
[GlobalClass] public partial class ManiaPlayField : PlayField
{
    /// <summary>
    /// The max score this instance can get.
    /// </summary>
    [Export] public int MaxScore = 1000000;

    [Export] public ManiaNoteSkin NoteSkin;

    /// <summary>
    /// Readies this PlayField for Mania gameplay!
    /// </summary>
    /// <param name="meta">The song meta</param>
    /// <param name="chart">The chart loaded</param>
    /// <param name="targetIndex">The index to play in <see cref="SongMeta.PlayableCharts"/>.</param>
    public override void Setup(SongMeta meta, RubiChart chart, int targetIndex)
    {
        string noteSkinName = meta.NoteSkin;
        string noteSkinPath = PathUtility.GetResourcePath($"res://Resources/UI/Styles/{noteSkinName}/Mania");
        if (string.IsNullOrWhiteSpace(noteSkinPath))
        {
            string defaultSkin = ProjectSettings.GetSetting("rubicon/rulesets/mania/default_note_skin").AsString();
            GD.PrintErr($"Mania Note Skin Path: {noteSkinName} does not exist. Defaulting to {defaultSkin}");
            noteSkinPath = PathUtility.GetResourcePath($"res://Resources/UI/Styles/{defaultSkin}/Mania");
        }

        NoteSkin = ResourceLoader.LoadThreadedGet(noteSkinPath) as ManiaNoteSkin;
        ManiaNoteFactory maniaFactory = new ManiaNoteFactory();
        maniaFactory.NoteSkin = NoteSkin;
        Factory = maniaFactory;
        
        base.Setup(meta, chart, targetIndex);
        
        Name = "Mania PlayField";
        for (int i = 0; i < BarLines.Length; i++)
            BarLines[i].MoveToFront();
    }
    
    /// <inheritdoc/>
    public override void UpdateOptions()
    {
        //BarLineContainer.
        //BarLineContainer.Position = new Vector2(0f, UserSettings.DownScroll ? -120f : 120f);

        for (int i = 0; i < BarLines.Length; i++)
        {
            if (BarLines[i] is ManiaBarLine maniaBarLine)
                maniaBarLine.SetDirectionAngle(!UserSettings.Gameplay.DownScroll ? Mathf.Pi / 2f : -Mathf.Pi / 2f);

            BarLines[i].AnchorTop = BarLines[i].AnchorBottom = UserSettings.Gameplay.DownScroll ? 1f : 0f;
            BarLines[i].OffsetTop = BarLines[i].OffsetBottom = UserSettings.Gameplay.DownScroll ? -140f : 140f;
            //BarLines[i].SetAnchorsPreset(barLinePreset, true);
        }
    }

    /// <inheritdoc />
    public override void UpdateStatistics()
    {
        // Score
        if (ScoreTracker.PerfectHits == ScoreTracker.NoteCount) ScoreTracker.Score = MaxScore;
        else
        {
            float baseNoteValue = ((float)MaxScore / ScoreTracker.NoteCount) * 0.35f;
            float baseScore = (baseNoteValue * ScoreTracker.PerfectHits) + (baseNoteValue * (ScoreTracker.GreatHits * 0.9375f)) + (baseNoteValue * (ScoreTracker.GoodHits * 0.625f)) + (baseNoteValue * (ScoreTracker.OkayHits * 0.3125f)) + (baseNoteValue * (ScoreTracker.BadHits * 0.15625f));
            float bonusScore = Mathf.Sqrt(((float)ScoreTracker.HighestCombo / ScoreTracker.NoteCount)) * MaxScore * 0.65f; 
            ScoreTracker.Score = (int)Math.Floor(baseScore + bonusScore);
        }
        
        // Accuracy
        long hitNotes = ScoreTracker.PerfectHits + ScoreTracker.GreatHits + ScoreTracker.GoodHits + ScoreTracker.OkayHits + ScoreTracker.BadHits + ScoreTracker.Misses;
        ScoreTracker.Accuracy = ScoreTracker.PerfectHits == ScoreTracker.NoteCount
            ? 100f
            : ((ScoreTracker.PerfectHits + (ScoreTracker.GreatHits * 0.95f) + (ScoreTracker.GoodHits * 0.65f) + (ScoreTracker.OkayHits * 0.3f) + (ScoreTracker.BadHits + 0.15f)) /
               hitNotes) * 100f;
        
        // Rank
        if (ScoreTracker.Score >= MaxScore)
            ScoreTracker.Rank = ScoreRank.P;
        else if (ScoreTracker.Score >= 975000)
            ScoreTracker.Rank = ScoreRank.Sss;
        else if (ScoreTracker.Score >= 950000)
            ScoreTracker.Rank = ScoreRank.Ss;
        else if (ScoreTracker.Score >= 900000)
            ScoreTracker.Rank = ScoreRank.S;
        else if (ScoreTracker.Score >= 800000)
            ScoreTracker.Rank = ScoreRank.A;
        else if (ScoreTracker.Score >= 700000)
            ScoreTracker.Rank = ScoreRank.B;
        else if (ScoreTracker.Score >= 600000)
            ScoreTracker.Rank = ScoreRank.C;
        else
            ScoreTracker.Rank = ScoreRank.D;
        
        // Clear Rank
        if (ScoreTracker.Misses > 0)
            ScoreTracker.Clear = ClearRank.Clear;
        else if (ScoreTracker.GoodHits + ScoreTracker.OkayHits + ScoreTracker.BadHits > 0)
            ScoreTracker.Clear = ClearRank.FullCombo;
        else if (ScoreTracker.GreatHits > 0)
            ScoreTracker.Clear = ClearRank.GreatFullCombo;
        else
            ScoreTracker.Clear = ClearRank.Perfect;
    }

    public override void UpdateHealth(HitType hit)
    {
        int healthAddition = 0;
        switch (hit)
        {
            case HitType.Perfect:
                healthAddition = 3;
                break;
            case HitType.Great:
                healthAddition = 2;
                break;
            case HitType.Good:
                healthAddition = 1;
                break;
            case HitType.Okay:
                healthAddition = -1;
                break;
            case HitType.Bad:
                healthAddition = -2;
                break;
            case HitType.Miss:
                healthAddition = -5 - (int)ScoreTracker.MissStreak * 4;
                break;
        }

        int predictedHealth = Health + healthAddition;
        if (predictedHealth > MaxHealth)
            healthAddition -= predictedHealth % MaxHealth;

        Health += healthAddition;
        if (Health < 0)
            Health = 0;
    }

    /// <inheritdoc />
    public override bool GetFailCondition() => Health <= 0;

    public override BarLine CreateBarLine(IndividualChart chart, int index)
    {
        ManiaBarLine barLine = new ManiaBarLine();
        barLine.Setup(chart, NoteSkin, Chart.ScrollSpeed);
        barLine.Name = "Mania Bar Line " + index;
            
        // Using Council positioning for now, sorry :/
        //curBarLine.Position = new Vector2(i * 720f - (chart.Charts.Length - 1) * 720f / 2f, 0f);
        barLine.AnchorLeft = barLine.AnchorRight = ((index * 0.5f) - (Chart.Charts.Length - 1) * 0.5f / 2f) + 0.5f;
        return barLine;
    }
}