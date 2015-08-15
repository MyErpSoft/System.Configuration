namespace System.Configuration.Core.Metadata {

    /// <summary>
    /// 描述了配置对象中的一个属性。
    /// </summary>
    public interface IProperty {
        object DefaultValue { get; }
        string Name { get; }
        IType PropertyType { get; }
    }
}
