using System;

namespace Lucent.Common.Storage
{
    public interface IStorageEntity
    {
        string Id {get;set;}
        string ETag {get;set;}
        DateTime Updated {get;set;}
        int Version {get;set;}
    }
}