using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TextureGroupsConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string defaultPath = "";
        private string enginePath = "";

        UnrealIniFile? defaultIniFile = null;
        UnrealIniFile? engineIniFile = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region RESET VALUES
        private void Reset_GroupName(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = GetRowItem(sender);
            pg.DisplayName = pg.Original_DisplayName;
            RefreshGrid();
        }
        private void Reset_MinLODSize(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = GetRowItem(sender);
            pg.MinLod = pg.Original_MinLod;
            RefreshGrid();
        }
        private void Reset_MaxLODSize(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = GetRowItem(sender);
            pg.MaxLod = pg.Original_MaxLod;
            RefreshGrid();
        }
        private void Reset_LODBias(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = GetRowItem(sender);
            pg.LODBias = pg.Original_LODBias;
            RefreshGrid();
        }
        private void Reset_NumStreamedMips(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = GetRowItem(sender);
            pg.NumMips = pg.Original_NumMips;
            RefreshGrid();
        }
        private void Reset_MinMagFilter(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = GetRowItem(sender);
            pg.MinMag = pg.Original_MinMag;
            RefreshGrid();
        }
        private void Reset_MipFilter(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = GetRowItem(sender);
            pg.MipFilter = pg.Original_MipFilter;
            RefreshGrid();
        }
        private void Reset_MipGenSettings(object sender, RoutedEventArgs e)
        {
            ProfileGroup pg = GetRowItem(sender);
            pg.MipGen = pg.Original_MipGen;
            RefreshGrid();
        }
        #endregion
        private ProfileGroup? GetRowItem(object sender)
        {
            var button = sender as Button;
            return button.Tag as ProfileGroup;
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

        private void RefreshGrid()
        {
            DG_ProfileGroupsTable.CommitEdit(DataGridEditingUnit.Cell, true);
            DG_ProfileGroupsTable.CommitEdit(DataGridEditingUnit.Row, true);
            CollectionViewSource.GetDefaultView(DG_ProfileGroupsTable.ItemsSource).Refresh();
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
            }
        }

        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;

            if(defaultIniFile == null || engineIniFile == null) { return; }

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

                profileGroup.SaveValues();
            }

            defaultIniFile.Save(defaultPath);
            engineIniFile.Save(enginePath);

            LoadProfiles();

            MessageBox.Show("Changes succesfully saved", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if(groups == null) { return; }
            int customProfiles = 0;
            foreach (ProfileGroup group in groups)
            {
                if (group.Name.StartsWith("TEXTUREGROUP_Project")) customProfiles++;
            }
            string projectName = $"Project{(customProfiles + 1).ToString("D2")}";
            groups.Add(new ProfileGroup(true, $"TEXTUREGROUP_{projectName}", projectName));
            DG_ProfileGroupsTable.ItemsSource = groups;
        }
    }
}
