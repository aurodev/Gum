﻿using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Managers;
using Gum.Plugins;
using Gum.ToolStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gum.PropertyGridHelpers
{
    public class VariableInCategoryPropagationLogic : Singleton<VariableInCategoryPropagationLogic> 
    {
        public void PropagateVariablesInCategory(string memberName)
        {
            var currentCategory = SelectedState.Self.SelectedStateCategorySave;
            /////////////////////Early Out//////////////////////////
            if (currentCategory == null)
            {
                return;
            }
            ///////////////////End Early Out////////////////////////

            var defaultState = SelectedState.Self.SelectedElement.DefaultState;
            var defaultVariable = defaultState.GetVariableSave(memberName);
            if (defaultVariable == null)
            {
                defaultVariable = defaultState.GetVariableRecursive(memberName);
            }
            var defaultVariableList = defaultState.GetVariableListSave(memberName);
            if(defaultVariableList == null && defaultVariable == null)
            {
                defaultVariableList = defaultState.GetVariableListRecursive(memberName);
            }

            // If the user is setting a variable that is a categorized state, the
            // default may be null. If so, then we need to select the first value
            // as the default so that values are always set:
            if(defaultVariable != null && defaultVariable.Value == null)
            {
                var variableContainer = SelectedState.Self.SelectedElement;


                var sourceObjectName = VariableSave.GetSourceObject(memberName);

                if(!string.IsNullOrEmpty(sourceObjectName))
                {
                    var nos = variableContainer.GetInstance(sourceObjectName);

                    if(nos != null)
                    {
                        variableContainer = ObjectFinder.Self.GetElementSave(nos);
                    }
                }

                ElementSave categoryContainer;
                StateSaveCategory category;
                var isState = defaultVariable.IsState(variableContainer, out categoryContainer, out category);

                if(isState)
                {
                    // we're going to assign a value on the variable, but we don't want to modify the original one so, 
                    // let's clone it:
                    defaultVariable = defaultVariable.Clone();
                    if(category != null)
                    {
                        defaultVariable.Value = category.States.FirstOrDefault()?.Name;
                    }
                    else
                    {
                        defaultVariable.Value = variableContainer.DefaultState?.Name;

                    }

                }
            }
            // variable lists cannot be states, so no need to do anything here:
            if(defaultVariableList != null && defaultVariableList.ValueAsIList == null)
            {
                // do nothing...
            }

            var defaultValue = defaultVariable?.Value ?? defaultVariableList?.ValueAsIList;

            foreach (var state in currentCategory.States)
            {

                if(defaultVariable != null)
                {
                    var existingVariable = state.GetVariableSave(memberName);
                    if (existingVariable == null)
                    {
                        if(defaultVariable != null)
                        {
                            VariableSave newVariable = defaultVariable.Clone();
                            newVariable.Value = defaultValue;
                            newVariable.SetsValue = true;
                            newVariable.Name = memberName;

                            state.Variables.Add(newVariable);

                            GumCommands.Self.GuiCommands.PrintOutput(
                                $"Adding {memberName} to {currentCategory.Name}/{state.Name}");
                        }
                    }
                    else if (existingVariable.SetsValue == false)
                    {
                        existingVariable.SetsValue = true;
                    }
                }
                else if(defaultVariableList != null)
                {
                    var existingVariableList = state.GetVariableListSave(memberName);
                    if(existingVariableList == null)
                    {
                        if(defaultVariableList != null)
                        {
                            var newVariableList = defaultVariableList.Clone();
                            // handled by clone:
                            //newVariableList.ValueAsIList = defaultVariableList.ValueAsIList;
                            newVariableList.Name = memberName;
                            state.VariableLists.Add(newVariableList);
                            GumCommands.Self.GuiCommands.PrintOutput(
                                $"Adding {memberName} to {currentCategory.Name}/{state.Name}");
                        }
                    }
                }
            }
        }


        public void AskRemoveVariableFromAllStatesInCategory(string variableName, StateSaveCategory stateCategory)
        {
            string message =
                $"Are you sure you want to remove {variableName} from all states in {stateCategory.Name}? The following states will be impacted:\n";

            foreach (var state in stateCategory.States)
            {
                message += $"\n{state.Name}";
            }

            var result = MessageBox.Show(message, "Remove Variables?", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                foreach (var state in stateCategory.States)
                {
                    var foundVariable = state.Variables.FirstOrDefault(item => item.Name == variableName);

                    if (foundVariable != null)
                    {
                        state.Variables.Remove(foundVariable);
                    }
                    else
                    {
                        // it's probably a list:
                        var foundVariableList = state.VariableLists.FirstOrDefault(item => item.Name == variableName);
                        if(foundVariableList != null)
                        {
                            state.VariableLists.Remove(foundVariableList);
                        }
                    }
                }

                // save everything
                GumCommands.Self.FileCommands.TryAutoSaveCurrentElement();
                GumCommands.Self.GuiCommands.RefreshStateTreeView();
                // no selection has changed, but we want to force refresh here because we know
                // we really need a refresh - something was removed.
                GumCommands.Self.GuiCommands.RefreshPropertyGrid(force:true);

                PluginManager.Self.VariableRemovedFromCategory(variableName, stateCategory);
            }
        }

    }
}
