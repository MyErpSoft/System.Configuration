using System.Collections.Generic;

namespace System.Configuration.Core.Metadata {

    /// <summary>
    /// 默认的配置对象绑定器。
    /// </summary>
    public interface IConfigurationObjectBinder {

        /// <summary>
        /// 返回此Binder的可以识别的提供者集合。
        /// </summary>
        IEnumerable<string> SupportedNames { get; }

        /// <summary>
        /// 尝试通过一个类型名称关联到一个类型信息。
        /// </summary>
        /// <param name="name">要检索的类型名称描述。</param>
        /// <param name="type">如果检索到此名称的类型，将返回他，否则返回null.</param>
        /// <returns>是否成功检索到此名称的类型。</returns>
        bool TryBindToType(ObjectTypeQualifiedName name, out IType type);
        
    }
    
}
