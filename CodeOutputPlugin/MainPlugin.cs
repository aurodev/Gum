﻿using CodeOutputPlugin.Manager;
using Gum;
using Gum.DataTypes;
using Gum.Plugins;
using Gum.Plugins.BaseClasses;
using Gum.ToolStates;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeOutputPlugin
{
    [Export(typeof(PluginBase))]
    public class MainPlugin : PluginBase
    {
        #region Fields/Properties

        public override string FriendlyName => "Code Output Plugin";

        public override Version Version => new Version(1, 0);

        Views.CodeWindow control;
        ViewModels.CodeWindowViewModel viewModel;

        #endregion

        public override bool ShutDown(PluginShutDownReason shutDownReason)
        {
            return true;
        }

        public override void StartUp()
        {
            AssignEvents();

            var item = this.AddMenuItem("Plugins", "View Code");
            item.Click += HandleViewCodeClicked;


        }

        private void AssignEvents()
        {
            this.InstanceSelected += HandleInstanceSelected;
            this.VariableSet += HandleVariableSet;
            this.StateWindowTreeNodeSelected += HandleStateSelected;
        }

        private void HandleStateSelected(TreeNode obj)
        {
            if (control != null)
            {
                RefreshCodeDisplay();
            }
        }

        private void HandleInstanceSelected(ElementSave arg1, InstanceSave instance)
        {
            if(control != null)
            {
                RefreshCodeDisplay();
            }
        }

        private void HandleVariableSet(ElementSave arg1, InstanceSave instance, string arg3, object arg4)
        {
            if (control != null)
            {
                RefreshCodeDisplay();
            }
        }

        private void HandleViewCodeClicked(object sender, EventArgs e)
        {

            if (control == null)
            {
                CreateControl();
            }

            GumCommands.Self.GuiCommands.ShowControl(control);

            RefreshCodeDisplay();
        }

        private void RefreshCodeDisplay()
        {
            var instance = SelectedState.Self.SelectedInstance;
            var selectedElement = SelectedState.Self.SelectedElement;

            switch(viewModel.WhatToView)
            {
                case ViewModels.WhatToView.SelectedElement:

                    if (instance != null)
                    {
                        string gumCode = CodeGenerator.GetCodeForInstance(instance, VisualApi.Gum);
                        string xamarinFormsCode = CodeGenerator.GetCodeForInstance(instance, VisualApi.XamarinForms);
                        viewModel.Code = $"//Gum Code:\n{gumCode}\n\n//Xamarin Forms Code:\n{xamarinFormsCode}";
                    }
                    else if(selectedElement != null)
                    {
                        string gumCode = CodeGenerator.GetCodeForElement(selectedElement, VisualApi.Gum);
                        viewModel.Code = $"//Code for {selectedElement.ToString()}\n{gumCode}";
                    }
                    break;
                case ViewModels.WhatToView.SelectedState:
                    var state = SelectedState.Self.SelectedStateSave;

                    if (state != null && selectedElement != null)
                    {
                        string gumCode = CodeGenerator.GetCodeForState(selectedElement, state, VisualApi.Gum);
                        viewModel.Code = $"//State Code for {state.Name ?? "Default"}:\n{gumCode}";
                    }
                    break;
            }

        }

        private void CreateControl()
        {
            control = new Views.CodeWindow();

            viewModel = new ViewModels.CodeWindowViewModel();
            viewModel.PropertyChanged += (not, used) => RefreshCodeDisplay();
            control.DataContext = viewModel;

            GumCommands.Self.GuiCommands.AddControl(control, "Code", TabLocation.Right);

        }


    }
}