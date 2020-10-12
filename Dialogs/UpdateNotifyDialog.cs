﻿using System;
using System.Windows.Forms;

namespace WebMConverter.Dialogs
{
    public partial class UpdateNotifyDialog : Form
    {
        private string _changelog;
        private bool _loadednotes;

        public UpdateNotifyDialog(string newVersion, string changelog)
        {
            _changelog = changelog;

            InitializeComponent();

            label1.Text = string.Format(label1.Text, "WebM for Lazys");
        }

        void panel1_Resize(object sender, EventArgs e)
        {
            label1.Left = (panel1.ClientSize.Width - label1.Width) / 2;
            label2.Left = (panel1.ClientSize.Width - label2.Width) / 2;
        }

        void boxReleaseNotes_CheckedChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start($"https://argorar.github.io/WebMConverter/#changelog");
            //ReleaseNotesPanel.Visible = boxReleaseNotes.Checked;

            //if (_loadednotes) return;

            //ReleaseNotes.DocumentText = _changelog;
            //_loadednotes = true;
        }
    }
}