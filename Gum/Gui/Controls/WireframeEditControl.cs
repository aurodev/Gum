﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FlatRedBall.AnimationEditorForms.Controls
{
    public partial class WireframeEditControl : UserControl
    {
        #region Fields

        List<int> mAvailableZoomLevels = new List<int>();

        #endregion

        #region Events

        public event EventHandler ZoomChanged;

        #endregion

        #region Properties

        public List<int> AvailableZoomLevels
        {
            get
            {
                return mAvailableZoomLevels;
            }
        }

        public int PercentageValue
        {
            get
            {
                return int.Parse(ComboBox.Text.Substring(0, ComboBox.Text.Length - 1));
            }
            set
            {
                ComboBox.Text = value.ToString() + "%";
            }
        }

        int CurrentZoomIndex
        {
            get
            {
                return mAvailableZoomLevels.IndexOf(PercentageValue);
            }
        }

        #endregion

        public WireframeEditControl()
        {
            InitializeComponent();

            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            mAvailableZoomLevels.Add(1600);
            mAvailableZoomLevels.Add(1200);
            mAvailableZoomLevels.Add(1000);
            mAvailableZoomLevels.Add(800);
            mAvailableZoomLevels.Add(700);
            mAvailableZoomLevels.Add(600);
            mAvailableZoomLevels.Add(500);
            mAvailableZoomLevels.Add(400);
            mAvailableZoomLevels.Add(350);
            mAvailableZoomLevels.Add(300);
            mAvailableZoomLevels.Add(250);
            mAvailableZoomLevels.Add(200);
            mAvailableZoomLevels.Add(175);
            mAvailableZoomLevels.Add(150);
            mAvailableZoomLevels.Add(125);
            mAvailableZoomLevels.Add(100);
            mAvailableZoomLevels.Add(87);
            mAvailableZoomLevels.Add(75);
            mAvailableZoomLevels.Add(63);
            mAvailableZoomLevels.Add(50);
            mAvailableZoomLevels.Add(33);

            mAvailableZoomLevels.Add(25);

            foreach (var value in mAvailableZoomLevels)
            {
                ComboBox.Items.Add(value.ToString() + "%");
            }

            ComboBox.Text = "100%";

        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ZoomChanged != null)
            {
                ZoomChanged(this, null);
            }
        }

        public void ZoomOut()
        {
            int index = CurrentZoomIndex;

            if (index < mAvailableZoomLevels.Count - 1)
            {
                index++;
                ComboBox.SelectedIndex = index;
            }
        }

        public void ZoomIn()
        {
            int index = CurrentZoomIndex;

            if (index > 0)
            {
                index--;
                ComboBox.SelectedIndex = index;

            }

        }
    }
}
