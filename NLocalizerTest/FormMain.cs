using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NLocalizer;
using NLocalizerTest.Properties;

namespace NLocalizerTest
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            Translator.Translation.CurrentLanguage = Settings.Default.Language;
            Translator.ReportErrors = true;
            Translator.Translate(this);
            FillLanguagesMenu();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Messages.TestMessage, Messages.TestCaption, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void FillLanguagesMenu()
        {
            foreach (KeyValuePair<string, TranslationClasses> lang in Translator.Translation)
            {
                if (lang.Key.Equals("Neutral")) continue;
                bool found = false;
                foreach (object item in languageToolStripMenuItem.DropDownItems)
                {
                    var toolStripMenuItem = item as ToolStripMenuItem;
                    if (toolStripMenuItem != null &&
                        toolStripMenuItem.Text == lang.Key)
                    {
                        found = true;
                        if (lang.Key.Equals(Settings.Default.Language))
                        {
                            toolStripMenuItem.Checked = true;
                        }
                    }
                }
                if (found == false)
                {
                    ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)languageToolStripMenuItem.DropDownItems.Add(lang.Key);

                    if (lang.Key.Equals(Settings.Default.Language))
                    {
                        toolStripMenuItem.Checked = true;
                    }
                }
            }
        }

        private void languageToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (Translator.Translation.Exists(e.ClickedItem.Text))
            {
                try
                {
                    Translator.Translation.CurrentLanguage = e.ClickedItem.Text;
                    Settings.Default.Language = Translator.Translation.CurrentLanguage;
                    Translator.Translate(this);
                    Settings.Default.Save();
                    foreach (var dropDownItem in languageToolStripMenuItem.DropDownItems)
                    {
                        ((ToolStripMenuItem)dropDownItem).Checked = dropDownItem.Equals(e.ClickedItem);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
