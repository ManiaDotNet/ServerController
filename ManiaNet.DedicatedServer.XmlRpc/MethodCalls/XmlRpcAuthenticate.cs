using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.XmlRpc.MethodCalls
{
    public sealed class XmlRpcAuthenticate : XmlRpcMethodCall
    {
        public string Login { get; private set; }

        public override string Name
        {
            get { return "Authenticate"; }
        }

        public string Password { get; private set; }

        public XmlRpcAuthenticate(string login, string password)
        {
            Login = login;
            Password = password;
        }

        public override string GetXml()
        {
            return XmlRpcConstants.XmlDeclaration +
                XmlRpcConstants.MethodCallAndNameOpening + Name + XmlRpcConstants.MethodNameClosingAndParamsOpening +
                XmlRpcConstants.ParamOpening + XmlRpcConstants.StringValueOpening + Login + XmlRpcConstants.StringValueClosing + XmlRpcConstants.ParamClosing +
                XmlRpcConstants.ParamOpening + XmlRpcConstants.StringValueOpening + Password + XmlRpcConstants.StringValueClosing + XmlRpcConstants.ParamClosing +
                XmlRpcConstants.ParamsAndMethodCallClosing;
        }
    }
}