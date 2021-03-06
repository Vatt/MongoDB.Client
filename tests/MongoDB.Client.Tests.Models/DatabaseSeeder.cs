﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Tests.Models
{
    public class DatabaseSeeder
    {
        public IEnumerable<T> GenerateSeed<T>(int count = 1024)
        {
            if (typeof(T) == typeof(RootDocument))
            {
                return new RootDocumentSeeder().GenerateSeed(count).Select(d => (T)(object)d).ToArray();
            }
            if (typeof(T) == typeof(GeoIp))
            {
                return new GeoIpSeeder().GenerateSeed(count).Select(d => (T)(object)d).ToArray();
            }
            if (typeof(T) == typeof(MediumModel))
            {
                return new MediumModelSeeder().GenerateSeed(count).Select(d => (T)(object)d).ToArray();
            }
            throw new NotSupportedException();
        }
    }
}
