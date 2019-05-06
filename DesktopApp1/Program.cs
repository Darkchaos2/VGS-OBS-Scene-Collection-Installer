using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VGSInstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        
        [STAThread]
        static void Main()
        {
            string SceneCollectionName = @"\LSUVGS.json";

            string streamPackPath = Directory.GetCurrentDirectory() + @"\Collection";
            string defaultOBSPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\obs-studio\";

            var userscenePath = string.Empty;

            #region Exist checks

            // Check scene collection
            if (!File.Exists(streamPackPath + SceneCollectionName))
            {
                MessageBox.Show("Cannot find profile JSON file.", "Missing profile!");
                System.Environment.Exit(1);
            }

            // Check assets
            if (!Directory.Exists(streamPackPath + @"\Assets"))
            {
                MessageBox.Show("Please make sure this application is in the root of the streaming pack, above the assets folder.", "Cannot find assets!");
                System.Environment.Exit(1);
            }

            // Check if there already is scene
            if (File.Exists(userscenePath + SceneCollectionName))
            {
                DialogResult result = MessageBox.Show("Are you sure you want to overwrite the previous LSUVGS scene?", "LSUVGS scene already exists!", MessageBoxButtons.YesNo);

                if (result == DialogResult.No)
                {
                    MessageBox.Show("Exiting");
                    System.Environment.Exit(1);
                }
            }

            #endregion

            #region User Input

            MessageBox.Show("Please select your OBS appdata folder\n\nThis can be found under %appdata%\\obs-studio\\", "Installation Directory");

            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog();
            folderBrowser.Description = "Select OBS roaming appdata folder...";
            folderBrowser.UseDescriptionForTitle = true;
            folderBrowser.SelectedPath = defaultOBSPath;

            if(folderBrowser.ShowDialog() == false)
            {
                MessageBox.Show("Did not install.", "Cancelled");
                System.Environment.Exit(1);
            }

            userscenePath = folderBrowser.SelectedPath + @"basic\scenes\";

            #endregion

            #region Copy assets
            
            var sceneJSONPath = string.Empty;
            
            foreach (string dirPath in Directory.GetDirectories(streamPackPath + @"\Assets", "*.*", SearchOption.AllDirectories))
            {
                //MessageBox.Show(dirPath);
                Directory.CreateDirectory(dirPath.Replace(streamPackPath, userscenePath.Replace(streamPackPath, userscenePath + @"\Assets")));
            }
            
            foreach (string newPath in Directory.GetFiles(streamPackPath + @"\Assets", "*.*", SearchOption.AllDirectories))
            {
                //MessageBox.Show(newPath);
                File.Copy(newPath, newPath.Replace(streamPackPath, userscenePath), true);
            }

            #endregion

            #region Editing JSON

            // Import and parse JSON
            JObject scene = JObject.Parse(File.ReadAllText(streamPackPath + SceneCollectionName));


            // Change all directories of local sources
            foreach (JObject source in scene["sources"])
            {
                if (source["settings"]["file"] == null)
                    continue;

                string sourcePath = (string)source["settings"]["file"];
                sourcePath = sourcePath.Replace("D:/Users/Louis/AppData/Roaming/obs-studio/basic/scenes", userscenePath);
                source["settings"]["file"] = sourcePath;
            }

            #endregion

            #region Write JSON

            // Try to write file
            try
            {
                System.Diagnostics.Debug.WriteLine(userscenePath + SceneCollectionName);
                //File.WriteAllText(scenePath + profileName, JsonConvert.SerializeObject(scene, Formatting.Indented), System.Text.Encoding.Unicode);

                using (StreamWriter file = File.CreateText(userscenePath + SceneCollectionName))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                    scene.WriteTo(writer);
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Cannot write to destination.", "Unauthorized Access!");
            }

            #endregion

            MessageBox.Show("You can find the LSUVGS scene in the Scene Collection tab at the top of OBS :)", "Installation Complete!");
        }

        // open obs
    }
}
