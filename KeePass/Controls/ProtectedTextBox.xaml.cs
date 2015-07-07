﻿using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Devices;

namespace KeePass.Controls
{
    public partial class ProtectedTextBox
    {
        public static DependencyProperty IsProtectedProperty = DependencyProperty
            .Register("IsProtected", typeof(bool), typeof(ProtectedTextBox),
                new PropertyMetadata(true, OnIsProtectedChanged));

        public static DependencyProperty MonoSpacedProperty = DependencyProperty
            .Register("MonoSpaced", typeof(bool), typeof(ProtectedTextBox),
                new PropertyMetadata(false, OnMonoSpacedChanged));

        public static DependencyProperty TextProperty = DependencyProperty
            .Register("Text", typeof(string), typeof(ProtectedTextBox), new PropertyMetadata(null, OnTextChanged));

        public static DependencyProperty HeaderProperty = DependencyProperty
            .Register("Header", typeof(string), typeof(ProtectedTextBox), new PropertyMetadata(null, OnHeaderChanged));

        /// <summary>
        /// 
        /// </summary>
        public event Action<object, string> TextValueUpdated;

        public bool IsProtected
        {
            get { return (bool)GetValue(IsProtectedProperty); }
            set { SetValue(IsProtectedProperty, value); }
        }

        public bool MonoSpaced
        {
            get { return (bool)GetValue(MonoSpacedProperty); }
            set { SetValue(MonoSpacedProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ProtectedTextBox()
        {
            InitializeComponent();
            UpdateProtectState();

            txtMask.DataContext = this;
            DependencyPropertyChangedEventArgs p = new DependencyPropertyChangedEventArgs();
            txtPassword.DataContext = this;
        }

        //public virtual void OnTextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{

        //    TextChanged(this, e);
        //}

        public void SelectAll()
        {
            txtPassword.SelectAll();
        }

        /// <summary>
        /// Raises the <see cref="TextChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="TextChangedEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnTextChanged(TextChangedEventArgs e)
        {
            if (TextValueUpdated != null)
            {
                TextValueUpdated(this, txtPassword.Text);
            }
        }

        private static void OnIsProtectedChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var protect = (ProtectedTextBox)d;
            protect.UpdateProtectState();
        }

        private static void OnMonoSpacedChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var protect = (ProtectedTextBox)d;

            if (protect.MonoSpaced)
            {
                protect.txtPassword.FontFamily =
                    new FontFamily("Courier New");
            }
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var protect = d as ProtectedTextBox;

            if (protect != null)
            {
                protect.txtMask.Header = e.NewValue as string;
                protect.txtPassword.Header = e.NewValue as string;
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var protect = d as ProtectedTextBox;

            if (protect != null)
            {
                protect.txtMask.Password = (e.NewValue ?? e.OldValue) as string;
                protect.txtPassword.Text = (e.NewValue ?? e.OldValue) as string;
                if (protect.TextValueUpdated != null)
                    protect.TextValueUpdated.Invoke(protect, e.NewValue as string);
            }
        }

        private void UpdateProtectState()
        {
            if (!IsProtected)
            {
                txtMask.Opacity = 0;
                txtPassword.Opacity = 1;

                return;
            }

            var focused = FocusManager.GetFocusedElement();
            var editing = ReferenceEquals(focused, txtPassword);

            txtMask.Opacity = editing ? 0 : 1;
            txtPassword.Opacity = editing ? 1 : 0;
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            txtPassword.Opacity = 1;
            UpdateProtectState();
        }

        private void TxtPassword_OnDoubleTap(object sender, GestureEventArgs e)
        {
            SelectAll();
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsProtected)
                txtPassword.Opacity = 0;

            UpdateProtectState();
        }

        private void txtPassword_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            OnTextChanged(e);
        }

        private void TxtPassword_OnActionIconTapped(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPassword.Text))
            {
                //Clipboard.SetText(txtPassword.Text);

                Encoding utf8 = Encoding.UTF8;
                var pwlength = txtPassword.Text.Length + 1;
                byte[] bytes = utf8.GetBytes(txtPassword.Text + "\0");
                string content = utf8.GetString(bytes, 0, bytes.Length);
                Clipboard.SetText(content);
                VibrateController.Default.Start(TimeSpan.FromMilliseconds(30));
            }
        }

    }

}