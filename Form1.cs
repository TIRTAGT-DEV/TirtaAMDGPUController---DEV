using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TirtaAMDGPUController
{
    public partial class Form1 : Form
    {
        private static string[] AllowedGPU = { "460", "470", "480", "560", "570", "580", "590" };
        private static List<string> PickedGPU = new List<string>();
        private static int GPUAmount = 0;

        class TirtaGPU
        {
            public string Name = "NO GPU";
            public string index = "-1";
            public bool Picked = false;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void TestDLL()
        {
            
        }

        private void RefreshList()
        {
            GPUAmount = 0;
            PickedGPU.Clear();
            RegistryKey localMachineKey = Registry.LocalMachine;
            RegistryKey softwareKey = localMachineKey.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}");
            var cardFolders = softwareKey.GetSubKeyNames();
            int tmp;
            checkedListBox1.Items.Clear();
            foreach (var cardFolder in cardFolders)
            {
                if (int.TryParse(cardFolder, out tmp))
                {
                    RegistryKey cardRegistry = null;
                    try
                    {
                        cardRegistry = softwareKey.OpenSubKey(cardFolder);
                    }
                    catch (Exception) { }

                    if (cardRegistry != null)
                    {
                        var GPUName = cardRegistry.GetValue("DriverDesc");
                        var KMD_EnableInternalLargePage = cardRegistry.GetValue("KMD_EnableInternalLargePage");
                        if (KMD_EnableInternalLargePage == null || KMD_EnableInternalLargePage.ToString() != "2")
                        {
                            checkedListBox1.Items.Add("GPU " + cardFolder + " " + GPUName + " ( Graphics ! )", false);
                        }
                        else
                        {
                            checkedListBox1.Items.Add("GPU " + cardFolder + " " + GPUName + " ( Compute ! )", false);
                        }
                        GPUAmount++;
                    }
                }
            }
            checkedListBox1.Refresh();
        }
        private void ListRefresher_DoWork(object sender, DoWorkEventArgs e)
        {
            
        }

        private void ListRefresher_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ListRefresher.RunWorkerAsync();
            try
            {
                RefreshList();
            }
            catch (Exception pfff)
            {
                MessageBox.Show("We had an Exceptions : \n" + pfff);
                Application.Exit();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SwitchToGraphics();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SwitchToCompute();
        }

        private void SwitchToGraphics()
        {
            MessageBox.Show("WARNING All selected GPU on this system will get switched to GRAPHICS including your iGPU or your Nvidia ! \n This could crash the driver and you might need to reinstall it. \n \n For IGPU, You need to reinstall the whole OS. \n\n I'm not responsible if you ignore this warning, Do it at your own risk !", "Switch To Graphics Warning");
            string selectedGPU = "";
            foreach (string item in PickedGPU)
            {
                selectedGPU += (item + "\n");
            }
            MessageBox.Show("Last Warning, Kill the whole exe via task manager if you want to cancel the switch.  \n These GPU will be switched into graphics mode : \n" + selectedGPU, "Switch To Graphics Warning");
            string cardString = "Switching results per GPU : ";
            Dictionary<string, string> results = new Dictionary<string, string>();
            RegistryKey localMachineKey = Registry.LocalMachine;
            RegistryKey softwareKey = localMachineKey.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}");
            var cardFolders = softwareKey.GetSubKeyNames();
            int tmp;
            foreach (var cardFolder in cardFolders)
            {
                if (int.TryParse(cardFolder, out tmp))
                {
                    RegistryKey cardRegistry = null;
                    try
                    {
                        cardRegistry = softwareKey.OpenSubKey(cardFolder, true);
                    }
                    catch (Exception) { results.Add(cardFolder, "Permission issue :("); cardRegistry = null; }
                    if (cardRegistry != null)
                    {
                        
                        var GPUName = cardRegistry.GetValue("DriverDesc");
                        if (PickedGPU.Any(a => a.Contains(cardFolder)))
                        {
                            /** Switch all to graphics mode */
                            try
                            {
                                cardRegistry.DeleteValue("KMD_EnableInternalLargePage");
                                results.Add(cardFolder, "Success");
                            }
                            catch (Exception ex)
                            {
                                results.Add(cardFolder, "Error: " + ex.Message);
                            }
                        }
                        else
                        {
                            results.Add(cardFolder, "Ignored, not selected...");
                        }
                    }
                }
            }
            foreach (var result in results)
            {
                cardString += "\n" + result.Key + ": " + result.Value;
            }
            cardString += "\n" + "Please restart the system now to apply the modes :)";
            MessageBox.Show(cardString, "Graphics Time !", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SwitchToCompute()
        {
            MessageBox.Show("WARNING All selected GPU on this system will get switched to COMPUTE including your iGPU or your Nvidia ! \n This could crash the driver and you might need to reinstall it. \n \n For IGPU, You need to reinstall the whole OS. \n\n I'm not responsible if you ignore this warning, Do it at your own risk !", "Switch To Compute Warning");
            string selectedGPU = "";
            foreach (string item in PickedGPU)
            {
                selectedGPU += (item + "\n");
            }
            MessageBox.Show("Last Warning, Kill the whole exe via task manager if you want to cancel the switch. \n These GPU will be switched into compute mode : \n" + selectedGPU, "Switch To Compute Warning");
            string cardString = "Switching results per GPU : ";
            Dictionary<string, string> results = new Dictionary<string, string>();
            RegistryKey localMachineKey = Registry.LocalMachine;
            RegistryKey softwareKey = localMachineKey.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}");
            var cardFolders = softwareKey.GetSubKeyNames();
            int tmp;
            foreach (var cardFolder in cardFolders)
            {
                if (int.TryParse(cardFolder, out tmp))
                {
                    RegistryKey cardRegistry = null;
                    try
                    {
                        cardRegistry = softwareKey.OpenSubKey(cardFolder, true);
                    }
                    catch (Exception) { results.Add(cardFolder, "Permission issue :("); cardRegistry = null; }
                    if (cardRegistry != null)
                    {
                        var GPUName = cardRegistry.GetValue("DriverDesc");
                        if (PickedGPU.Any(a => a.Contains(cardFolder)))
                        {
                            /** Put it into Compute */
                            try { 
                                cardRegistry.SetValue("KMD_EnableInternalLargePage", "2", RegistryValueKind.DWord); 
                                results.Add(cardFolder, "Success"); 
                            }
                            catch (Exception ex) { 
                                results.Add(cardFolder, "Error: " + ex.Message);
                            }
                        }
                        else
                        {
                            results.Add(cardFolder, "Ignored, not selected...");
                        }
                    }
                }
            }
            foreach (var result in results)
            {
                cardString += "\n" + result.Key + ": " + result.Value;
            }
            cardString += "\n" + "Please restart the system now to apply the modes :)";
            MessageBox.Show(cardString, "Compute Time !", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RefreshList();  
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked) {
                PickedGPU.Add(checkedListBox1.Items[e.Index].ToString().Substring(0, 8));
            }
            if (e.NewValue == CheckState.Unchecked) {
                PickedGPU.Remove(checkedListBox1.Items[e.Index].ToString().Substring(0, 8));
            }
        }
    }
}
