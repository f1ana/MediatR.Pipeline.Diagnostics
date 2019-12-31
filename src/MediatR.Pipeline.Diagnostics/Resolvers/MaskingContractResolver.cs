using System.Reflection;
using System.Text.RegularExpressions;
using MediatR.Pipeline.Diagnostics.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MediatR.Pipeline.Diagnostics.Resolvers {
    internal sealed class MaskingContractResolver : DefaultContractResolver {
        private readonly IOptions<MediatrDiagnostics> _options;

        public MaskingContractResolver(IOptions<MediatrDiagnostics> options) {
            _options = options;
        }
        
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var property = base.CreateProperty(member, memberSerialization);

            if (!(_options?.Value?.MaskedProperties?.Length > 0)) return property;
            
            foreach (var p in _options.Value.MaskedProperties) {
                if (Regex.Match(property.PropertyName, p, RegexOptions.Singleline).Success) {
                    property.ShouldSerialize = x => false;
                }
            }

            return property;
        }
    }
}