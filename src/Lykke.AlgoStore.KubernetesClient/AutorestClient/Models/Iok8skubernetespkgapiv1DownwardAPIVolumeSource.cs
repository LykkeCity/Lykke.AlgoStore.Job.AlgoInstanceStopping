// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.AlgoStore.KubernetesClient.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// DownwardAPIVolumeSource represents a volume containing downward API
    /// info. Downward API volumes support ownership management and SELinux
    /// relabeling.
    /// </summary>
    public partial class Iok8skubernetespkgapiv1DownwardAPIVolumeSource
    {
        /// <summary>
        /// Initializes a new instance of the
        /// Iok8skubernetespkgapiv1DownwardAPIVolumeSource class.
        /// </summary>
        public Iok8skubernetespkgapiv1DownwardAPIVolumeSource()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// Iok8skubernetespkgapiv1DownwardAPIVolumeSource class.
        /// </summary>
        /// <param name="defaultMode">Optional: mode bits to use on created
        /// files by default. Must be a value between 0 and 0777. Defaults to
        /// 0644. Directories within the path are not affected by this setting.
        /// This might be in conflict with other options that affect the file
        /// mode, like fsGroup, and the result can be other mode bits
        /// set.</param>
        /// <param name="items">Items is a list of downward API volume
        /// file</param>
        public Iok8skubernetespkgapiv1DownwardAPIVolumeSource(int? defaultMode = default(int?), IList<Iok8skubernetespkgapiv1DownwardAPIVolumeFile> items = default(IList<Iok8skubernetespkgapiv1DownwardAPIVolumeFile>))
        {
            DefaultMode = defaultMode;
            Items = items;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets optional: mode bits to use on created files by
        /// default. Must be a value between 0 and 0777. Defaults to 0644.
        /// Directories within the path are not affected by this setting. This
        /// might be in conflict with other options that affect the file mode,
        /// like fsGroup, and the result can be other mode bits set.
        /// </summary>
        [JsonProperty(PropertyName = "defaultMode")]
        public int? DefaultMode { get; set; }

        /// <summary>
        /// Gets or sets items is a list of downward API volume file
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        public IList<Iok8skubernetespkgapiv1DownwardAPIVolumeFile> Items { get; set; }

    }
}
