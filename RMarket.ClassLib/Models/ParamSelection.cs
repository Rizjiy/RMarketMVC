﻿using Newtonsoft.Json;
using RMarket.ClassLib.Abstract;
using RMarket.ClassLib.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RMarket.ClassLib.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ParamSelection: ParamBase
    {
        [JsonProperty]  
        public override string FieldName { get; set; }

        [JsonProperty]
        public object ValueMin { get; set; }

        [JsonProperty]
        public object ValueMax { get; set; }

        public override void RepairValue(PropertyInfo prop, Type entityType)
        {
            ParameterAttribute attr = (ParameterAttribute)prop.GetCustomAttribute((Type)typeof(ParameterAttribute), (bool)false);

            FieldName = prop.Name;

            if (ValueMin != null &&
                ValueMin.GetType() != prop.PropertyType &&
                ValueMin.GetType() != typeof(string))
            {
                try
                {
                    //желательно сразу string
                    ValueMin = Serializer.Deserialize(ValueMin.ToString(), prop.PropertyType);
                }
                catch (Exception)
                {
                    ValueMin = null;
                }

                try
                {
                    //желательно сразу string
                    ValueMax = Serializer.Deserialize(ValueMax.ToString(), prop.PropertyType);
                }
                catch (Exception)
                {
                    ValueMax = null;
                }

            }

            DisplayName = (attr.Name == null) ? prop.Name : attr.Name;
            Description = (attr.Description == null) ? "" : attr.Description;
        }

        public override void RepairValue(PropertyInfo prop, object entity)
        {
            RepairValue(prop, entity.GetType());

            if (ValueMin == null)
                ValueMin = prop.GetValue(entity);

            if (ValueMax == null)
                ValueMax = prop.GetValue(entity);

        }

        public override void RepairValue(object entity)
        {
            PropertyInfo prop = entity.GetType().GetProperty(FieldName);

            if (ValueMin == null)
                ValueMin = prop.GetValue(entity);

            if (ValueMax == null)
                ValueMax = prop.GetValue(entity);
        }


    }
}
