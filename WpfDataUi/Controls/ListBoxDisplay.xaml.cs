﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfDataUi.DataTypes;

namespace WpfDataUi.Controls
{
    /// <summary>
    /// Interaction logic for ListBoxDisplay.xaml
    /// </summary>
    public partial class ListBoxDisplay : UserControl, IDataUi
    {
        InstanceMember mInstanceMember;
        public InstanceMember InstanceMember
        { 
            get => mInstanceMember; 
            set
            {
                bool instanceMemberChanged = mInstanceMember != value;
                if (mInstanceMember != null && instanceMemberChanged)
                {
                    mInstanceMember.PropertyChanged -= HandlePropertyChange;
                }
                mInstanceMember = value;
                if (mInstanceMember != null && instanceMemberChanged)
                {
                    mInstanceMember.PropertyChanged += HandlePropertyChange;
                }
                Refresh();

            }
        }

        public ListBoxDisplay()
        {
            InitializeComponent();
        }

        public bool SuppressSettingProperty { get; set; }

       static SolidColorBrush DefaultValueBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 255, 180));
       static SolidColorBrush CustomValueBackground = System.Windows.Media.Brushes.White;

        public void Refresh(bool forceRefreshEvenIfFocused = false)
        {
            bool canRefresh = true;

            if(canRefresh)
            {
                SuppressSettingProperty = true;

                //mTextBoxLogic.RefreshDisplay();

                this.Label.Text = InstanceMember.DisplayName;
                this.RefreshContextMenu(ListBox.ContextMenu);
                //this.RefreshContextMenu(StackPanel.ContextMenu);

                this.ListBox.Background = InstanceMember.IsDefault ? DefaultValueBackground : CustomValueBackground;

                //HintTextBlock.Visibility = !string.IsNullOrEmpty(InstanceMember?.DetailText) ? Visibility.Visible : Visibility.Collapsed;
                //HintTextBlock.Text = InstanceMember?.DetailText;
                TrySetValueOnUi(InstanceMember?.Value);
                //RefreshIsEnabled();

                SuppressSettingProperty = false;
            }
        }

        public ApplyValueResult TryGetValueOnUi(out object result)
        {
            // todo - need to make this more flexible, but for now let's just support strings:
            if(InstanceMember?.PropertyType == typeof(List<string>))
            {
                var value = new List<string>();

                foreach(var item in ListBox.Items)
                {
                    value.Add(item?.ToString());
                }

                result = value;

                return ApplyValueResult.Success;

            }
            else
            {
                result = null;
                return ApplyValueResult.NotSupported;
            }
        }

        public ApplyValueResult TrySetValueOnUi(object value)
        {
            if(value is List<string> valueAsList)
            {
                var newList = new List<string>();
                newList.AddRange(valueAsList);
                ListBox.ItemsSource = newList;
            }
            else
            {
                // todo - we may want to clone the list here too to prevent unintentional editing of the underlying list
                ListBox.ItemsSource = value as IEnumerable;
            }
            return ApplyValueResult.Success;
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            NewEntryListBox.Visibility = Visibility.Visible;
            NewTextBox.Focus();
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            var isCtrlDown =
                (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));

            if (e.Key == Key.Delete)
            {
                var selectedItem = ListBox.SelectedIndex;

                if(selectedItem > -1)
                {
                    var listToRemoveFrom = ListBox.ItemsSource as IList;

                    if(ListBox.SelectedIndex < listToRemoveFrom.Count)
                    {
                        listToRemoveFrom.RemoveAt(ListBox.SelectedIndex);
                    }
                }
                this.TrySetValueOnInstance();

                TryDoManualRefresh();
            }
            else if(e.Key == Key.C && isCtrlDown)
            {
                var selectedItem = ListBox.SelectedItem as string;

                if(!string.IsNullOrEmpty(selectedItem))
                {
                    Clipboard.SetText(selectedItem);
                }
            }
            else if(e.Key == Key.V && isCtrlDown)
            {
                var text = Clipboard.GetText();

                if(!string.IsNullOrEmpty(text))
                {
                    HandleAddTextItem(text);
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            HandleAddTextItem(NewTextBox.Text);
        }

        private void HandleAddTextItem(string text)
        {
            var listToAddTo = ListBox.ItemsSource as IList;
            if (listToAddTo != null)
            {
                listToAddTo.Add(text);
            }
            NewTextBox.Text = null;
            NewEntryListBox.Visibility = Visibility.Collapsed;
            this.TrySetValueOnInstance();

            TryDoManualRefresh();
        }

        private void TryDoManualRefresh()
        {
            var itemSourceList = ListBox.ItemsSource as IList;

            var needsManualRefresh = !(itemSourceList is INotifyCollectionChanged);
            if (needsManualRefresh)
            {
                ListBox.ItemsSource = null;
                TrySetValueOnUi(InstanceMember?.Value);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            HandleCancelItem();
        }

        private void HandleCancelItem()
        {
            NewTextBox.Text = null;
            NewEntryListBox.Visibility = Visibility.Collapsed;
        }

        private void NewTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                e.Handled = true;
                HandleAddTextItem(NewTextBox.Text);

            }
            else if(e.Key == Key.Escape)
            {
                e.Handled = true;
                HandleCancelItem();
            }
        }

        private void HandlePropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InstanceMember.Value))
            {
                this.Refresh();

            }
        }
    }
}
