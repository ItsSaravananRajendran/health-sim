using System.Collections.Generic;

namespace PlateCheck.Core
{
    public enum Phase { Intro, Assess, Act, Feedback, Debrief }

    /// <summary>
    /// The scenario as a state machine. Every transition is a method that returns false and
    /// leaves the state unchanged when it is not legal in the current phase.
    /// </summary>
    public sealed class ScenarioState
    {
        readonly Ratios target;
        readonly List<PlacedItem> plate = new List<PlacedItem>();
        readonly List<PlacedItem> tray = new List<PlacedItem>();
        readonly List<string> startingPlateIds = new List<string>();
        readonly HashSet<string> inspectedInAssess = new HashSet<string>();

        public Phase Phase { get; private set; } = Phase.Intro;
        public IReadOnlyList<PlacedItem> Plate => plate;
        public IReadOnlyList<PlacedItem> Tray => tray;
        public IReadOnlyList<string> StartingPlateIds => startingPlateIds;
        public Quality? ChosenQuality { get; private set; }

        /// <summary>Set when a response is chosen; null before that.</summary>
        public ScoreResult Result { get; private set; }

        /// <summary>True once the plate can no longer be changed.</summary>
        public bool PlateLocked => Phase != Phase.Act;

        public ScenarioState() : this(Ratios.MyPlate) { }

        public ScenarioState(Ratios target)
        {
            this.target = target;
        }

        /// <summary>Intro to Assess, with the variant's starting plate and tray.</summary>
        public bool Start(IReadOnlyList<PlacedItem> startingPlate, IReadOnlyList<PlacedItem> startingTray)
        {
            if (Phase != Phase.Intro) return false;
            plate.AddRange(startingPlate);
            tray.AddRange(startingTray);
            foreach (var item in startingPlate) startingPlateIds.Add(item.id);
            Phase = Phase.Assess;
            return true;
        }

        /// <summary>
        /// Legal in Assess and Act. Only inspections of starting plate items made during
        /// Assess count toward the inspection bonus.
        /// </summary>
        public bool Inspect(string id)
        {
            if (Phase != Phase.Assess && Phase != Phase.Act) return false;
            if (IndexOf(plate, id) < 0 && IndexOf(tray, id) < 0) return false;
            if (Phase == Phase.Assess && startingPlateIds.Contains(id)) inspectedInAssess.Add(id);
            return true;
        }

        public bool WasInspectedInAssess(string id) => inspectedInAssess.Contains(id);

        public bool InspectedAllStartingItems => MissedInspections().Count == 0;

        public List<string> MissedInspections()
        {
            var missed = new List<string>();
            foreach (var id in startingPlateIds)
                if (!inspectedInAssess.Contains(id)) missed.Add(id);
            return missed;
        }

        /// <summary>Assess to Act ("Ready to help").</summary>
        public bool ReadyToHelp()
        {
            if (Phase != Phase.Assess) return false;
            Phase = Phase.Act;
            return true;
        }

        /// <summary>Move an item from the tray to the plate area. Only legal in Act.</summary>
        public bool MoveToPlate(string id) => Move(id, tray, plate);

        /// <summary>Move an item from the plate area to the tray. Only legal in Act.</summary>
        public bool MoveToTray(string id) => Move(id, plate, tray);

        bool Move(string id, List<PlacedItem> from, List<PlacedItem> to)
        {
            if (PlateLocked) return false;
            int index = IndexOf(from, id);
            if (index < 0) return false;
            to.Add(from[index]);
            from.RemoveAt(index);
            return true;
        }

        public bool IsOnPlate(string id) => IndexOf(plate, id) >= 0;

        public BucketShares CurrentBreakdown() => Scoring.Breakdown(plate, target);

        /// <summary>Act to Feedback. Locks the plate and scores it as it stands now.</summary>
        public bool ChooseResponse(Quality quality)
        {
            if (Phase != Phase.Act) return false;
            ChosenQuality = quality;
            Result = new ScoreResult(new List<PlacedItem>(plate), quality, MissedInspections(), target);
            Phase = Phase.Feedback;
            return true;
        }

        /// <summary>Feedback to Debrief.</summary>
        public bool Continue()
        {
            if (Phase != Phase.Feedback) return false;
            Phase = Phase.Debrief;
            return true;
        }

        /// <summary>Back to Intro with everything cleared. Legal from Debrief (Try Again or Quit).</summary>
        public bool Reset()
        {
            if (Phase != Phase.Debrief) return false;
            plate.Clear();
            tray.Clear();
            startingPlateIds.Clear();
            inspectedInAssess.Clear();
            ChosenQuality = null;
            Result = null;
            Phase = Phase.Intro;
            return true;
        }

        static int IndexOf(List<PlacedItem> items, string id)
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i].id == id) return i;
            return -1;
        }
    }
}
