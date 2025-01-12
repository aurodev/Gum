﻿using Gum.Managers;
using Gum.PropertyGridHelpers;
using Gum.ToolStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Plugins.AlignmentButtons
{
    static class CommonControlLogic
    {
        static bool SelectionInheritsFromText()
        {
            if(SelectedState.Self.SelectedInstance != null)
            {
                return ObjectFinder.Self.GetRootStandardElementSave(SelectedState.Self.SelectedInstance)?.Name == "Text";
            }
            return false;
        }
        public static void SetXValues(global::RenderingLibrary.Graphics.HorizontalAlignment alignment, PositionUnitType xUnits)
        {
            SetAndCallReact("X", 0.0f, "float");
            SetAndCallReact("X Origin", alignment, "HorizontalAlignment");
            SetAndCallReact("X Units", xUnits, typeof(Gum.Managers.PositionUnitType).Name);

            if (SelectionInheritsFromText())
            {
                SetAndCallReact("HorizontalAlignment", alignment, "HorizontalAlignment");
            }

        }

        public static void SetYValues(global::RenderingLibrary.Graphics.VerticalAlignment alignment, PositionUnitType yUnits)
        {
            var state = SelectedState.Self.SelectedStateSave;

            SetAndCallReact("Y", 0.0f, "float");
            SetAndCallReact("Y Origin", alignment, typeof(global::RenderingLibrary.Graphics.VerticalAlignment).Name);
            SetAndCallReact("Y Units", yUnits, typeof(PositionUnitType).Name);

            if (SelectionInheritsFromText())
            {
                SetAndCallReact("VerticalAlignment", alignment, "VerticalAlignment");
            }

        }

        public static void SetAndCallReact(string unqualified, object value, string typeName)
        {
            var instance = SelectedState.Self.SelectedInstance;
            string GetVariablePrefix()
            {
                string prefixInternal = "";
                if (instance != null)
                {
                    prefixInternal = instance.Name + ".";
                }
                return prefixInternal;
            }
            var state = SelectedState.Self.SelectedStateSave;
            string prefix = GetVariablePrefix();


            var oldValue = state.GetValue(prefix + unqualified);
            state.SetValue(prefix + unqualified, value, typeName);

            // do this so the SetVariableLogic doesn't attempt to hold the object in-place which causes all kinds of weirdness
            RecordSetVariablePersistPositions();
            SetVariableLogic.Self.ReactToPropertyValueChanged(unqualified, oldValue, SelectedState.Self.SelectedElement, instance, refresh: false);
            ResumeSetVariablePersistOptions();
        }

        public static void RefreshAndSave()
        {
            GumCommands.Self.GuiCommands.RefreshPropertyGrid(force: true);
            GumCommands.Self.WireframeCommands.Refresh();
            GumCommands.Self.FileCommands.TryAutoSaveCurrentElement();
        }

        static bool StoredAttemptToPersistPositionsOnUnitChanges;
        private static void RecordSetVariablePersistPositions()
        {
            StoredAttemptToPersistPositionsOnUnitChanges = SetVariableLogic.Self.AttemptToPersistPositionsOnUnitChanges;
            SetVariableLogic.Self.AttemptToPersistPositionsOnUnitChanges = false;
        }

        private static void ResumeSetVariablePersistOptions()
        {
            SetVariableLogic.Self.AttemptToPersistPositionsOnUnitChanges = StoredAttemptToPersistPositionsOnUnitChanges;
        }
    }
}
