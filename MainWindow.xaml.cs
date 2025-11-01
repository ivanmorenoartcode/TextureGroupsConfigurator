using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TextureGroupsConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string defaultPath = "";
        private string enginePath = "";
        private string scalabilityPath = "";

        UnrealIniFile? defaultIniFile = null;
        UnrealIniFile? engineIniFile = null;

        UnrealIniFile? scalabilityFile = null;

        public MainWindow()
        {
            InitializeComponent();

            LoadScalabilityTab();
        }

        #region RESET VALUES
        private void Reset_GroupName(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            pg.DisplayName = pg.Original_DisplayName;
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }
        private void Reset_MinLODSize(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            pg.MinLod = pg.Original_MinLod;
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }
        private void Reset_MaxLODSize(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            pg.MaxLod = pg.Original_MaxLod;
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }
        private void Reset_LODBias(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            pg.LODBias = pg.Original_LODBias;
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }
        private void Reset_NumStreamedMips(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            pg.NumMips = pg.Original_NumMips;
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }
        private void Reset_MinMagFilter(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            pg.MinMag = pg.Original_MinMag;
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }
        private void Reset_MipFilter(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            pg.MipFilter = pg.Original_MipFilter;
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }
        private void Reset_MipGenSettings(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            pg.MipGen = pg.Original_MipGen;
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }
        #endregion

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = Helpers.GetRowItem(sender) as ProfileGroup;
            DeleteProfile(pg);
            Helpers.RefreshGrid(DG_ProfileGroupsTable);
        }

        private void DeleteProfile(ProfileGroup profileGroup)
        {
            defaultIniFile.Sections["/Script/Engine.TextureLODSettings"].RemoveArrayValue("TextureLODGroups", profileGroup.ToString());
            List<ProfileGroup> groups = (DG_ProfileGroupsTable.ItemsSource as IEnumerable<ProfileGroup>)?.ToList();
            if (groups == null) { return; }
            groups.Remove(groups.First(n => n.Name == profileGroup.Name));
            DG_ProfileGroupsTable.ItemsSource = groups;

        }

        private void ConstructDefaultAndEnginePath()
        {
            string project = TB_Project.Text;
            if (string.IsNullOrEmpty(project)) return;
            string platform = (CB_Platform.SelectedItem as ComboBoxItem).Content.ToString();
            string configFolder = $"{project}\\Config\\Platforms\\{platform}\\Config";
            defaultPath = $"{configFolder}\\DefaultDeviceProfiles.ini";
            enginePath = $"{configFolder}\\DefaultEngine.ini";
        }

        private void LoadProfiles()
        {
            ConstructDefaultAndEnginePath();

            string readPath = defaultPath;
            if (!File.Exists(defaultPath))
            {
                readPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DefaultDeviceProfiles.ini");
            }

            defaultIniFile = UnrealIniFile.Load(readPath);
            var section = defaultIniFile.Sections["/Script/Engine.TextureLODSettings"];
            var entries = section.GetStructValues("TextureLODGroups");

            bool existRemaps = false;
            UnrealIniSection remapsSection = null;

            engineIniFile = UnrealIniFile.Load(enginePath);
            if (engineIniFile.Sections.ContainsKey("EnumRemap"))
            {
                var enumRemap = engineIniFile.Sections["EnumRemap"];
                existRemaps = engineIniFile.Sections.TryGetValue("EnumRemap", out remapsSection);
            }

            List<ProfileGroup> profileGroups = new List<ProfileGroup>();

            foreach (var entry in entries)
            {
                entry.TryGetValue("Group", out var group);
                entry.TryGetValue("MinLODSize", out var minLodSize);
                entry.TryGetValue("MaxLODSize", out var maxLodSize);
                entry.TryGetValue("LODBias", out var bias);
                entry.TryGetValue("NumStreamedMips", out var numMips);
                entry.TryGetValue("MinMagFilter", out var minMag);
                entry.TryGetValue("MipFilter", out var mipFilter);
                entry.TryGetValue("MipGenSettings", out var mipGen);

                string displayName = group.Split('_', 2, options: StringSplitOptions.TrimEntries)[1];

                if (existRemaps)
                {
                    string key = group + ".DisplayName";
                    string value = remapsSection.GetValue(key);
                    if (value != null)
                    {
                        displayName = value;
                    }
                }

                profileGroups.Add(new ProfileGroup(false, group, displayName, minLodSize, maxLodSize, bias, numMips, minMag, mipFilter, mipGen));
            }

            DG_ProfileGroupsTable.ItemsSource = profileGroups;
        }

        private void SelectProject_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFolderDialog dialog = new();

            dialog.Multiselect = false;
            dialog.Title = "Select a folder";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string fullPathToFolder = dialog.FolderName;
                TB_Project.Text = fullPathToFolder;

                LoadProfiles();

                scalabilityPath = $"{fullPathToFolder}\\Config\\DeviceProfile.ini";
                LoadScalabilitySettingsGrids();
            }
        }

        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            if (TC_Manager.SelectedIndex == 1)
            {
                ApplyChangesProfileGroups();
            }
            else
            {
                ApplyChangesScalability();
            }
        }

        private void ApplyChangesProfileGroups()
        {
            int index = 0;

            if (defaultIniFile == null || engineIniFile == null) { return; }

            var section = defaultIniFile.Sections["/Script/Engine.TextureLODSettings"];
            var entries = section.GetStructValues("TextureLODGroups");

            foreach (var item in DG_ProfileGroupsTable.Items)
            {
                ProfileGroup profileGroup = item as ProfileGroup;

                if (profileGroup.DisplayName != profileGroup.Original_DisplayName)
                {
                    string key = profileGroup.Name + ".DisplayName";
                    string value = profileGroup.DisplayName;
                    if (engineIniFile.Sections.ContainsKey("EnumRemap"))
                    {
                        engineIniFile.Sections["EnumRemap"].SetValue(key, value);
                    }
                    else
                    {
                        var newSection = engineIniFile.AddSection("EnumRemap");
                        newSection.AddEntry(key, value);
                    }
                }

                if (!profileGroup.IsNew)
                {
                    section.ReplaceArrayValueAt("TextureLODGroups", index, profileGroup.ToString());
                }
                else
                {
                    section.AddEntry("+TextureLODGroups", profileGroup.ToString());
                }
                index++;
            }

            defaultIniFile.Save(defaultPath);
            engineIniFile.Save(enginePath);

            LoadProfiles();

            MessageBox.Show("Changes succesfully saved", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplyChangesScalability()
        {
            if (scalabilityFile == null || string.IsNullOrEmpty(TB_Project.Text)) { return; }

            string category = CB_Category.SelectedItem.ToString();

            SetScalabilityGridValue(DG_ScalabilityLOW.ScalabilityGrid, category + "@0");
            SetScalabilityGridValue(DG_ScalabilityMEDIUM.ScalabilityGrid, category + "@1");
            SetScalabilityGridValue(DG_ScalabilityHIGH.ScalabilityGrid, category + "@2");
            SetScalabilityGridValue(DG_ScalabilityEPIC.ScalabilityGrid, category + "@3");
            SetScalabilityGridValue(DG_ScalabilityCINEMATOGRAPHIC.ScalabilityGrid, category + "@Cine");

            scalabilityFile.Save(scalabilityPath);
            
            LoadScalabilitySettingsGrids();
            MessageBox.Show("Changes succesfully saved", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetScalabilityGridValue(DataGrid dg, string sectionName)
        {
            foreach (var item in dg.Items)
            {
                ScalabilitySetting scalabilitySetting = item as ScalabilitySetting;
                scalabilityFile.Sections[sectionName].SetValue(scalabilitySetting.Command, scalabilitySetting.CurrentValue);
            }
        }

        private void CB_Platform_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TB_Project.Text))
            {
                LoadProfiles();
            }
        }

        private void CreateNewProfile_Click(object sender, RoutedEventArgs e)
        {
            List<ProfileGroup> groups = (DG_ProfileGroupsTable.ItemsSource as IEnumerable<ProfileGroup>)?.ToList();
            if (groups == null) { return; }
            int customProfiles = 0;
            foreach (ProfileGroup group in groups)
            {
                if (group.Name.StartsWith("TEXTUREGROUP_Project")) customProfiles++;
            }
            string projectName = $"Project{(customProfiles + 1).ToString("D2")}";
            groups.Add(new ProfileGroup(true, $"TEXTUREGROUP_{projectName}", projectName));
            DG_ProfileGroupsTable.ItemsSource = groups;
        }
        private void LoadCategoryComboBox()
        {
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeviceProfile.ini");
            UnrealIniFile defaultValues = UnrealIniFile.Load(defaultPath);
            List<string> itemsToAdd = new List<string>();
            foreach (KeyValuePair<string, UnrealIniSection> section in defaultValues.Sections)
            {
                string name = section.Key.Split("@")[0];
                if (!itemsToAdd.Contains(name))
                {
                    itemsToAdd.Add(name);
                }
            }
            CB_Category.ItemsSource = itemsToAdd;
        }

        private void CB_Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TC_Manager.SelectedIndex == 2)
                LoadScalabilitySettingsGrids();
        }

        private void LoadScalabilityTab()
        {
            LoadCategoryComboBox();
            LoadScalabilitySettingsGrids();
        }

        private void LoadScalabilitySettingsGrids()
        {
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeviceProfile.ini");
            UnrealIniFile defaultValues = UnrealIniFile.Load(defaultPath);
            string pathRead = scalabilityPath;
            if (!File.Exists(scalabilityPath))
            {
                pathRead = defaultPath;
            }
            scalabilityFile = UnrealIniFile.Load(pathRead);

            string category = CB_Category.SelectedItem.ToString();
            foreach (KeyValuePair<string, UnrealIniSection> section in defaultValues.Sections)
            {
                if (section.Key.StartsWith(category))
                {
                    List<ScalabilitySetting> scalabilitySettings = new List<ScalabilitySetting>();
                    foreach (string setting in section.Value.GetAllEntries())
                    {
                        string[] line = setting.Split('=');
                        string command = line[0];
                        string defaultValue = line[1];
                        string currentValue = scalabilityFile.Sections[section.Key].GetValue(command);
                        scalabilitySettings.Add(new ScalabilitySetting(command, defaultValue, currentValue));
                    }
                    string gridID = section.Key.ToUpper().Split('@')[1];
                    switch (gridID)
                    {
                        case "0":
                            DG_ScalabilityLOW.ScalabilityGrid.ItemsSource = scalabilitySettings;
                            break;
                        case "1":
                            DG_ScalabilityMEDIUM.ScalabilityGrid.ItemsSource = scalabilitySettings;
                            break;
                        case "2":
                            DG_ScalabilityHIGH.ScalabilityGrid.ItemsSource = scalabilitySettings;
                            break;
                        case "3":
                            DG_ScalabilityEPIC.ScalabilityGrid.ItemsSource = scalabilitySettings;
                            break;
                        case "CINE":
                            DG_ScalabilityCINEMATOGRAPHIC.ScalabilityGrid.ItemsSource = scalabilitySettings;
                            break;
                    }
                }
            }
        }
    }
}
