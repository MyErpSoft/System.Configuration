using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Core.Metadata {

    internal class ConfigInterfaceRealProxy  : RealProxy {
        private readonly ConfigurationObject _cObj;
        public ConfigInterfaceRealProxy(Type interfaceType, ConfigurationObject cObj)
            :base(interfaceType) {
            _cObj = cObj;
        }

        public override IMessage Invoke(IMessage msg) {
            IMethodCallMessage callMessage = (IMethodCallMessage)msg;
            var propertyName = callMessage.MethodName;
            if (propertyName.StartsWith("get_") || propertyName.StartsWith("set_")) {
                propertyName = propertyName.Substring(4);
            }
            var property = _cObj.Part.Type.GetProperty(propertyName);
            object ret = _cObj.GetSimpleValue(property);
            return new ReturnMessage(ret, null, 0, callMessage.LogicalCallContext, callMessage);
        }
    }
}
