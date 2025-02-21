using System;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class ResourceData : IResourceData, IReadOnlyResourceData
    {
        public ResourceType Type;
        public long Ammount;

        ResourceType IReadOnlyResourceData.Type => Type;
        long IReadOnlyResourceData.Ammount => Ammount;

        ResourceType IResourceData.Type => Type;
        long IResourceData.Ammount => Ammount;



        public ResourceData(ResourceType resource, long ammount = 0)
        {
            Type = resource;
            Ammount = ammount;
        }

        public ResourceData(IReadOnlyResourceData template)
        {
            Type = template.Type;
            Ammount = template.Ammount;
        }
    }
}