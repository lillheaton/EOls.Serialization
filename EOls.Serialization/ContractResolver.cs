﻿using EOls.Serialization.Attributes;
using EOls.Serialization.ValueProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EOls.Serialization
{
    public class ContractResolver : DefaultContractResolver
    {
        public Type[] ExtraOptInAttributes { get; set; }

        public ContractResolver()
        {
            this.NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = true
            };
        }

        private bool ShouldIgnore(JsonProperty jsonProperty, MemberSerialization memberSerialization)
        {
            if (ExtraOptInAttributes == null)
                return jsonProperty.Ignored;

            if (memberSerialization == MemberSerialization.OptIn)
            {
                return jsonProperty.AttributeProvider
                    .GetAttributes(false)
                    .Any(attr => 
                        ExtraOptInAttributes
                        .Any(x => attr.Equals(x)));
            }

            return jsonProperty.Ignored;
        }

        private IValueProvider GetValueProvider(JsonProperty jsonProperty, MemberInfo memberInfo)
        {
            var cacheAttribute = memberInfo.GetCustomAttribute<CacheAttribute>(false);
            if (cacheAttribute != null)
                return new CacheValueProvider(memberInfo, jsonProperty.ValueProvider);

            return jsonProperty.ValueProvider;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            jsonProperty.Ignored = ShouldIgnore(jsonProperty, memberSerialization);
            jsonProperty.ValueProvider = GetValueProvider(jsonProperty, member);
            return jsonProperty;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {            
            return base.CreateProperties(type, memberSerialization);
        }
    }
}
