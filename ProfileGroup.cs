using System.Collections.Generic;

namespace TextureGroupsConfigurator
{
    internal class ProfileGroup
    {
        public bool IsNew { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string MinLod { get; set; }
        public string MaxLod { get; set; }
        public string LODBias { get; set; }
        public string NumMips { get; set; }
        public string MinMag { get; set; }
        public string MipFilter { get; set; }
        public string MipGen { get; set; }

        public string Original_Name { get; set; }
        public string Original_DisplayName { get; set; }
        public string Original_MinLod { get; set; }
        public string Original_MaxLod { get; set; }
        public string Original_LODBias { get; set; }
        public string Original_NumMips { get; set; }
        public string Original_MinMag { get; set; }
        public string Original_MipFilter { get; set; }
        public string Original_MipGen { get; set; }

        public bool IsNameReadOnly { get; set; }

        public ProfileGroup(bool isNew, string name, string displayName, string? minLod=null, string? maxLod = null, string? lODBias = null, string? numMips = null, string? minMag = null, string? mipFilter = null, string? mipGen = null)
        {
            IsNew = isNew;
            Name = name;
            DisplayName = displayName;
            MinLod = minLod != null ? minLod : "1";
            MaxLod = maxLod != null ? maxLod : "16384";
            LODBias = lODBias != null ? lODBias : "0";
            NumMips = numMips != null ? numMips : "-1";
            MinMag = minMag != null ? Capitalize(minMag) : "Aniso";
            MipFilter = mipFilter != null ? Capitalize(mipFilter) : "Point";
            MipGen = mipGen != null ? mipGen : "TMGS_SimpleAverage";

            SetupOriginalValue();

            IsNameReadOnly = !name.StartsWith("TEXTUREGROUP_Project");
        }

        private string Capitalize(string value)
        {
            value = value.Trim().ToLower();
            return char.ToUpper(value[0]) + value[1..];
        }

        private void SetupOriginalValue()
        {
            Original_Name = Name;
            Original_DisplayName = DisplayName;
            Original_MinLod = MinLod;
            Original_MaxLod = MaxLod;
            Original_LODBias = LODBias;
            Original_NumMips = NumMips;
            Original_MinMag = MinMag;
            Original_MipFilter = MipFilter;
            Original_MipGen = MipGen;
        }
        public void SaveValues()
        {
            Original_Name = Name;
            Original_DisplayName = DisplayName;
            Original_MinLod = MinLod;
            Original_MaxLod = MaxLod;
            Original_LODBias = LODBias;
            Original_NumMips = NumMips;
            Original_MinMag = MinMag;
            Original_MipFilter = MipFilter;
            Original_MipGen = MipGen;
        }

        public override string ToString()
        {
            var dict = new Dictionary<string, string>();
            dict["Group"] = Name;
            dict["MinLODSize"] = MinLod;
            dict["MaxLODSize"] = MaxLod;
            dict["LODBias"] = LODBias;
            dict["NumStreamedMips"] = NumMips;
            dict["MinMagFilter"] = MinMag;
            dict["MipFilter"] = MipFilter;
            dict["MipGenSettings"] = MipGen;
            string newValue = "(" + string.Join(",", dict.Select(kv => $"{kv.Key}={kv.Value}")) + ")";
            return newValue;
        }
    }
}
