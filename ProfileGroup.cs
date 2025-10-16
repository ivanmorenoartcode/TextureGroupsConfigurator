using System.Collections.Generic;

namespace TextureGroupsConfigurator
{
    internal class ProfileGroup
    {
        public bool IsNew { get; set; }
        public bool CanDelete { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string MinLod { get; set; }
        public string MaxLod { get; set; }
        public string LODBias { get; set; }
        public string NumMips { get; set; }
        public string MinMag { get; set; }
        public string MipFilter { get; set; }
        public string MipGen { get; set; }

        public string Default_MinLod { get; set; } = "1";
        public string Default_MaxLod { get; set; } = "16384";
        public string Default_LODBias { get; set; } = "0";
        public string Default_NumMips { get; set; } = "-1";
        public string Default_MinMag { get; set; } = "Aniso";
        public string Default_MipFilter { get; set; } = "Point";
        public string Default_MipGen { get; set; } = "TMGS_SimpleAverage";

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
            MinLod = minLod != null ? minLod : Default_MinLod;
            MaxLod = maxLod != null ? maxLod : Default_MaxLod;
            LODBias = lODBias != null ? lODBias : Default_LODBias;
            NumMips = numMips != null ? numMips : Default_NumMips;
            MinMag = minMag != null ? Capitalize(minMag) : Default_MinMag;
            MipFilter = mipFilter != null ? Capitalize(mipFilter) : Default_MipFilter;
            MipGen = mipGen != null ? mipGen : Default_MipGen;

            SetupOriginalValue();

            bool isCustom = name.StartsWith("TEXTUREGROUP_Project");
            IsNameReadOnly = !isCustom;
            CanDelete = isCustom;
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
