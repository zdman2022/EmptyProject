namespace GlueDynamicManager.DynamicInstances
{
    internal class HybridEntity : HybridGlueElement
    {
        public HybridEntity(object entity) : base(entity)
        {
        }

        public object Entity { get { return GlueElement; } }
    }
}
