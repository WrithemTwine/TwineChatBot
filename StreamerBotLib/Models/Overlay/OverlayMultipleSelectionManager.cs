using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;
using StreamerBotLib.Systems.Overlay.Models;

using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.Models.Overlay
{
    public static class OverlayMultipleSelectionManager
    {
        private static List<MultiActionSelector> _multiActionSelectors = [];

        private static OverlayMultiSelectionTypes FindSelectionMethod()
        {
            if (OptionFlags.MediaOverlayUseElimRandom)
            {
                return OverlayMultiSelectionTypes.EliminationSelection;
            }
            else if (OptionFlags.MediaOverlayUseStraightRandom)
            {
                return OverlayMultiSelectionTypes.RandomSelection;
            }
            else if (OptionFlags.MediaOverlayUseWeightedRandom)
            {
                return OverlayMultiSelectionTypes.WeightedSelection;
            }
            else //if (OptionFlags.MediaOverlayUseStraightSelection)
            {
                return OverlayMultiSelectionTypes.StraightSelection;
            }
        }

        public static OverlayActionType Selection(List<OverlayActionType> overlayActionTypes)
        {
            _multiActionSelectors.UniqueAdd(new MultiActionSelector(overlayActionTypes)); // only add if not already present
            return _multiActionSelectors.Find(m => m.Equals(overlayActionTypes)).ProcessSelection(FindSelectionMethod());
        }

        public static void SyncOverlayActionItems(List<OverlayActionType> overlayActionTypes)
        {
            var foundSelector = _multiActionSelectors.Find(m => m.Equals(overlayActionTypes));
            foundSelector?.SyncOverlayActionItems(overlayActionTypes);
        }

        /*
    <system:String
        x:Key="Options_MediaOverlay_UseMultiple_StraightSelection">Select each alert in order.</system:String>
    <system:String
        x:Key="Options_MediaOverlay_UseMultiple_StraightRandom">Randomly select alert to use each time.</system:String>
    <system:String
        x:Key="Options_MediaOverlay_UseMultiple_WeightedRandom">Randomly select alert, with increasing probability of unselected items.</system:String>
    <system:String
        x:Key="Options_MediaOverlay_UseMultiple_EliminationRandom">Randomly select alert from the remaining unselected items.</system:String>
        */

        private class MultiActionSelector(List<OverlayActionType> overlayActionTypes) : IEqualityComparer<MultiActionSelector>
        {
            private Random randomselector = new();

            private int _straightselectioncounter = -1; // selection counter does +1 % count, start at -1 for initial 0 selection
            private List<OverlayActionItem> _EliminationSelection = [];

            public OverlayTypes OverlayTypes { get; set; } = overlayActionTypes.FirstOrDefault()?.OverlayType ?? OverlayTypes.None;
            public string Action { get; set; } = overlayActionTypes.FirstOrDefault()?.ActionValue ?? string.Empty;

            public List<OverlayActionItem> OverlayActionItems { get; set; } = [.. (from a in overlayActionTypes select new OverlayActionItem(a))];

            /// <summary>
            /// Syncs the OverlayActionItems list with the provided list of OverlayActionTypes to ensure selection value counters are maintained.
            /// </summary>
            /// <param name="overlayActionTypes">The overlay action list to synchronize to the current list, in case the user changed the items.</param>
            public void SyncOverlayActionItems(List<OverlayActionType> overlayActionTypes)
            {
                // make a new list of items to check and add extra items
                List<OverlayActionItem> temp = [.. (from a in overlayActionTypes select new OverlayActionItem(a))];
                // find out which items to remove
                List<OverlayActionItem> toRemove = OverlayActionItems.Where(oai => !temp.Any(t => t.OverlayActionType.Equals(oai.OverlayActionType))).ToList();
                // remove all items that don't have the same property content
                OverlayActionItems.RemoveAll(toRemove.Contains);
                // remove all items from temp that are already in OverlayActionItems
                temp.RemoveAll(OverlayActionItems.Contains);
                // add the temp items for the new list
                OverlayActionItems.AddRange(temp);
            }

            public OverlayActionType ProcessSelection(OverlayMultiSelectionTypes selectionTypes)
            {
                OverlayActionType item = null;
                if (selectionTypes == OverlayMultiSelectionTypes.RandomSelection)
                {
                    int x = randomselector.Next(0, OverlayActionItems.Count);
                    item = OverlayActionItems[x].OverlayActionType;
                    OverlayActionItems[x].Selections++;
                }
                else if (selectionTypes == OverlayMultiSelectionTypes.StraightSelection)
                {
                    _straightselectioncounter = (_straightselectioncounter + 1) % OverlayActionItems.Count;
                    item = OverlayActionItems[_straightselectioncounter].OverlayActionType;
                    OverlayActionItems[_straightselectioncounter].Selections++;
                }
                else if (selectionTypes == OverlayMultiSelectionTypes.EliminationSelection)
                {
                    if (_EliminationSelection.Count == 0)
                    {
                        _EliminationSelection.AddRange(OverlayActionItems);
                    }

                    int x = randomselector.Next(0, _EliminationSelection.Count);
                    item = _EliminationSelection[x].OverlayActionType;
                    _EliminationSelection[x].Selections++;
                    _EliminationSelection.RemoveAt(x);
                }
                else if (selectionTypes == OverlayMultiSelectionTypes.WeightedSelection)
                {
                    // create a weighted list of items based on the number of selections
                    const int weightedinterval = 10;
                    List<OverlayActionItem> weightedList = [];
                    foreach (var (overlayActionItem, weight) in from overlayActionItem in OverlayActionItems
                                                                let weight = Math.Max(1, weightedinterval - overlayActionItem.Selections)// weight is 10 minus the number of selections, minimum 1
                                                                select (overlayActionItem, weight))
                    {
                        for (int i = 0; i < weight; i++) // add more of the same items the fewer times it's been selected; more weight, more chances to be picked
                        {
                            weightedList.Add(overlayActionItem);
                        }
                    }

                    int x = randomselector.Next(0, weightedList.Count);
                    item = weightedList[x].OverlayActionType;
                    weightedList[x].Selections++;

                    // reset list if all selections exceed the weighted selection interval, since the list would be no longer weighted
                    if (OverlayActionItems.Min(a => a.Selections) >= weightedinterval)
                    {
                        foreach (var a in OverlayActionItems)
                        {
                            a.Selections = 0;
                        }
                    }
                }

                return item;
            }

            public bool Equals(MultiActionSelector x, MultiActionSelector y)
            {
                if (x is null || y is null) return false;
                return x.OverlayTypes == y.OverlayTypes && x.Action == y.Action;
            }
            public bool Equals(List<OverlayActionType> x)
            {
                var first = overlayActionTypes.FirstOrDefault();
                if (x is null || first is null) return false;

                return first.OverlayType == OverlayTypes && first.ActionValue == Action;
            }

            public int GetHashCode([DisallowNull] MultiActionSelector obj)
            {
                return (OverlayTypes.ToString() + Action).GetHashCode();
            }
        }

        private class OverlayActionItem(OverlayActionType overlayActionType)
        {
            public OverlayActionType OverlayActionType { get; } = overlayActionType;
            public int Selections { get; set; } = 0;
        }
    }
}
