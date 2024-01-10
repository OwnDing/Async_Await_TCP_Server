using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Blank_TCP_Server.Language
{
    internal class LanguageMgr
    {
    }
    public class LanguageItem
    {
        private const string RootNodeName = "language";
        private const string StringsNodeName = "strings";
        private const string FormsNodeName = "forms";

        private XmlNode stringsNode;
        private XmlNode formsNode;

        private string id;
        private string name;
        private string localeName;

        public LanguageItem(string fileName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            var root = xmlDoc.DocumentElement;
            if (root != null)
            {
                this.id = GetAttributeValue(root, "Id");
                this.name = GetAttributeValue(root, "Name");
                this.localeName = GetAttributeValue(root, "LocaleName");

                this.stringsNode = root.SelectSingleNode(StringsNodeName);
                this.formsNode = root.SelectSingleNode(FormsNodeName);
            }
        }

        public string Id
        {
            get { return this.id; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public string LocaleName
        {
            get { return this.localeName; }
        }

        private void ApplyLanguage(Control control, XmlNode formNode, ToolTip toolTip = null)
        {
            if (control == null || control.Name == "")
            {
                return;
            }
            var ctrlNode = formNode.SelectSingleNode(control.Name);
            if (ctrlNode != null)
            {
                control.Text = GetAttributeValue(ctrlNode, "Text");
                string tips = GetAttributeValue(ctrlNode, "ToolTip");
                if (!string.IsNullOrEmpty(tips) && toolTip != null)
                    toolTip.SetToolTip(control, tips);
            }
            foreach (Control ctrl in control.Controls)
                ApplyLanguage(ctrl, formNode, toolTip);
            // 菜单项，特别遍历
            if (control is ToolStrip)
            {
                foreach (ToolStripItem toolItem in (control as ToolStrip).Items)
                    ApplyLanguage(toolItem, formNode);
            }
        }

        private void ApplyLanguage(ToolStripItem menuItem, XmlNode formNode)
        {
            if (string.IsNullOrEmpty(menuItem.Name))
                return;

            var itemNode = formNode.SelectSingleNode(menuItem.Name);
            if (itemNode != null)
            {
                menuItem.Text = GetAttributeValue(itemNode, "Text");
                menuItem.ToolTipText = GetAttributeValue(itemNode, "ToolTip");
            }
            if (menuItem is ToolStripDropDownItem)
            {
                foreach (ToolStripItem item in (menuItem as ToolStripDropDownItem).DropDownItems)
                    ApplyLanguage(item, formNode);
            }
        }

        public bool LoadFormLanguage(Form form)
        {
            if (form == null || formsNode == null || !formsNode.HasChildNodes || formsNode.SelectSingleNode(form.Name) == null)
                return false;

            // 创建ToolTip控件， 以支持ToolTip显示
            var toolTip = new ToolTip();
            var formNode = formsNode.SelectSingleNode(form.Name);
            form.Text = GetAttributeValue(formNode, "Text");
            foreach (Control ctrl in form.Controls)
                ApplyLanguage(ctrl, formNode, toolTip);
            return true;
        }

        private string GetAttributeValue(XmlNode xmlNode, string attrName)
        {
            if (xmlNode.Attributes != null && xmlNode.Attributes[attrName] != null)
                return xmlNode.Attributes[attrName].Value;
            return string.Empty;
        }

        public string GetText(string textID, string defaultText = "")
        {
            if (stringsNode == null || !stringsNode.HasChildNodes)
                return defaultText;

            foreach (XmlNode node in stringsNode.ChildNodes)
                if (node.Name.Equals(textID))
                    return node.InnerText;

            return defaultText;
        }
    }

    public class LanguageList : List<LanguageItem>
    {
    }

    // 多语言管理器
    public static class ML
    {
        private static LanguageItem activeLanguage;
        private static LanguageList languages;

        // 最初调用
        public static int LoadLanguages(string searchPattern, string defaultLanguageId = "")
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");
            return LoadLanguages(path, searchPattern, defaultLanguageId);
        }

        public static int LoadLanguages(string path, string searchPattern, string defaultLanguageId = "")
        {
            languages = new LanguageList();
            if (!Directory.Exists(path))
                return 0;

            var files = Directory.GetFiles(path, searchPattern);
            foreach (string file in files)
                languages.Add(new LanguageItem(file));
            if (!string.IsNullOrEmpty(defaultLanguageId))
                LoadLanguageById(defaultLanguageId);

            return languages.Count;
        }

        public static string ActiveLanguageId
        {
            get { return (activeLanguage != null) ? activeLanguage.Id : string.Empty; }
        }

        public static string[] LanguageLocalNames
        {
            get
            {
                if (languages == null || languages.Count == 0)
                    return new string[0];
                var names = new string[languages.Count];
                for (int i = 0; i <= languages.Count - 1; i++)
                    names[i] = languages[i].LocaleName;
                return names;
            }
        }

        public static LanguageItem ActiveLanguage
        {
            get { return activeLanguage; }
        }

        public static LanguageList Languages
        {
            get { return languages; }
        }

        public static bool LoadFormLanguage(Form form)
        {
            return (ActiveLanguage != null) ? ActiveLanguage.LoadFormLanguage(form) : false;
        }

        public static string GetText(string textId, string defaultText = "")
        {
            return (ActiveLanguage != null) ? ActiveLanguage.GetText(textId, defaultText) : defaultText;
        }

        public static bool LoadLanguageById(string id)
        {
            foreach (var language in Languages)
            {
                if (language.Id.Equals(id))
                {
                    activeLanguage = language;
                    return true;
                }
            }

            return false;
        }

        public static bool LoadLanguageByIndex(int index)
        {
            if (index < 0 || index > languages.Count - 1)
                return false;

            activeLanguage = languages[index];
            return true;
        }
    }
}
