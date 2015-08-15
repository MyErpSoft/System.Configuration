using System.ComponentModel;

namespace System.Configuration.Core.Metadata {

    /// <summary>
    /// 描述配置对象的类型信息。
    /// </summary>
    public interface IType {

        /// <summary>
        /// 返回指定名称的属性。
        /// </summary>
        /// <param name="name">要检索的属性名称</param>
        /// <returns>如果找到此名称的属性将返回他，否则（找不到或类型不一致），将抛出异常。</returns>
        IProperty GetProperty(string name);

        /// <summary>
        /// 获取此类型的转换器，用于字符串到此类型的互相转换。
        /// </summary>
        /// <returns>一个转换器实例。</returns>
        TypeConverter GetConverter();
    }
}
