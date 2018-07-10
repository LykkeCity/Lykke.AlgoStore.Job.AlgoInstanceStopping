// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.AlgoStore.Job.Stopping.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class PodResponseModel
    {
        /// <summary>
        /// Initializes a new instance of the PodResponseModel class.
        /// </summary>
        public PodResponseModel()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the PodResponseModel class.
        /// </summary>
        public PodResponseModel(string name = default(string), string namespaceProperty = default(string), string phase = default(string))
        {
            Name = name;
            NamespaceProperty = namespaceProperty;
            Phase = phase;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Namespace")]
        public string NamespaceProperty { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Phase")]
        public string Phase { get; set; }

    }
}
