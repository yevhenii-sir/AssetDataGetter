using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace RoomAssetGetData
{
    public partial class Asset_data : Form
    {
        public class AssetData
        {
            public Asset[] assets { get; set; }
            public bool visible { get; set; }
            public int depth { get; set; }
            public bool userdefinedDepth { get; set; }
            public bool inheritLayerDepth { get; set; }
            public bool inheritLayerSettings { get; set; }
            public int gridX { get; set; }
            public int gridY { get; set; }
            public object[] layers { get; set; }
            public bool hierarchyFrozen { get; set; }
            public string resourceVersion { get; set; }
            public string name { get; set; }
            public object[] tags { get; set; }
            public string resourceType { get; set; }
        }

        public class Asset
        {
            public Spriteid spriteid { get; set; }
            public float headPosition { get; set; }
            public float rotation { get; set; }
            public float scaleX { get; set; }
            public float scaleY { get; set; }
            public float animationSpeed { get; set; }
            public long colour { get; set; }
            public object inheritedItemId { get; set; }
            public bool frozen { get; set; }
            public bool ignore { get; set; }
            public bool inheritItemSettings { get; set; }
            public float x { get; set; }
            public float y { get; set; }
            public string resourceVersion { get; set; }
            public string name { get; set; }
            public object[] tags { get; set; }
            public string resourceType { get; set; }
        }

        public class Spriteid
        {
            public string name { get; set; }
            public string path { get; set; }
        }

        public Asset_data()
        {
            InitializeComponent();

            treeView1.Nodes.Add(Fill_Tree(typeof(AssetData)));
            treeView1.ExpandAll();

            if (File.Exists("previous.ini"))
            {
                using (StreamReader streamReader = new StreamReader("previous.ini"))
                {
                    string tempString = null;
                    while ((tempString = streamReader.ReadLine()) != null)
                    {
                        listBox1.Items.Add(tempString);
                    }
                }
            }
        }

        public TreeNode Fill_Tree(Type type)
        {
            TreeNode tempTree = new TreeNode(type.Name.ToLower());

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (!property.PropertyType.IsPrimitive && property.PropertyType.Name != "String" && property.PropertyType.Name != "Object[]" && property.PropertyType.Name != "Object")
                {
                    tempTree.Nodes.Add(Fill_Tree(property.PropertyType.IsArray ? property.PropertyType.GetElementType() : property.PropertyType));
                } else
                {
                    tempTree.Nodes.Add(property.Name);
                }
            }
            
            return tempTree;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            try
            {
                AssetData rootobject = JsonConvert.DeserializeObject<AssetData>(textBox1.Text);

                Asset[] asset = rootobject.assets;

                foreach (Asset asset1 in asset)
                {
                    string tempString = "";

                    foreach (string item in listBox1.Items)
                    {
                        if (item.StartsWith("$$$"))
                        {
                            string forActionString = item;
                            string action = "";

                            if (item.Contains("/"))
                            {
                                action = item.Substring(item.IndexOf('/') + 1);

                                forActionString = item.Remove(item.IndexOf('/'));
                            }

                            var split_string = forActionString.Trim('$').Split(".");

                            if (split_string.Length == 4)
                            {
                                dynamic spriteid = asset1.GetType().GetProperty(split_string[2]).GetValue(asset1);
                                tempString += ActionWithString(spriteid.GetType().GetProperty(split_string[3]).GetValue(spriteid).ToString(), action);
                            }
                            else if (split_string.Length == 3)
                            {
                                tempString += ActionWithString(asset1.GetType().GetProperty(split_string[2]).GetValue(asset1).ToString(), action);
                            } else
                            {
                                var a = rootobject.GetType().GetProperty(split_string[1]).GetValue(rootobject).ToString();
                                tempString += ActionWithString(a, action);
                            }
                        }
                        else
                        {
                            tempString += item;
                        }
                    }

                    textBox2.Text += tempString + "\r\n";
                }
            } catch (Exception exc)
            {
                textBox2.Text = exc.Message;
            }
        }

        public string ActionWithString(string text, string command)
        {
            var commandType = Regex.Match(command, @"^\D*").Value;
            var commandNum = Regex.Match(command, @"\d*$").Value;

            if (String.IsNullOrEmpty(commandNum)) return text;
            try
            {
                return commandType switch
                {
                    "r" => text.Substring(Convert.ToInt32(commandNum)),
                    "rb" => text.Remove(Convert.ToInt32(commandNum)),
                    "+" => (Convert.ToDouble(text) + Convert.ToDouble(commandNum)).ToString(),
                    "-" => (Convert.ToDouble(text) - Convert.ToDouble(commandNum)).ToString(),
                    "/" => (Convert.ToDouble(text) / Convert.ToDouble(commandNum)).ToString(),
                    "*" => (Convert.ToDouble(text) * Convert.ToDouble(commandNum)).ToString(),
                    _ => text
                };
            } catch
            {
                throw new Exception("An error on the operation processing function (r, rb, +, -, /, *)");
            }
            
        }

        private void Asset_data_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.Add("$$$" + treeView1.SelectedNode.FullPath.Replace("\\", "."));
                SaveList();
            } catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox3.Text);
            textBox3.Text = "";

            SaveList();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 1)
            {
                (listBox1.Items[listBox1.SelectedIndex - 1], listBox1.Items[listBox1.SelectedIndex]) = (listBox1.Items[listBox1.SelectedIndex], listBox1.Items[listBox1.SelectedIndex - 1]);
            }
            listBox1.SetSelected(Math.Clamp(listBox1.SelectedIndex - 1, 0, listBox1.Items.Count - 1), true);
            SaveList();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex <= (listBox1.Items.Count - 2))
            {
                (listBox1.Items[listBox1.SelectedIndex + 1], listBox1.Items[listBox1.SelectedIndex]) = (listBox1.Items[listBox1.SelectedIndex], listBox1.Items[listBox1.SelectedIndex + 1]);
            }
            listBox1.SetSelected(Math.Clamp(listBox1.SelectedIndex + 1, 0, listBox1.Items.Count - 1), true);
            SaveList();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items[listBox1.SelectedIndex] = textBox3.Text;
                SaveList();
            }
            catch { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                SaveList();
            } catch { }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox3.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
            } catch { }
            
        }

        public void SaveList()
        {
            using (StreamWriter streamWriter = new StreamWriter(new FileStream("previous.ini", FileMode.Create)))
            {
                foreach (var text in listBox1.Items)
                {
                    streamWriter.WriteLine(text);
                }
            }
        }

        private void Asset_data_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveList();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
    }
}
