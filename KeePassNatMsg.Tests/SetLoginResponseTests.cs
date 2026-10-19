using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using KeePassNatMsg.Protocol.Action;
using Microsoft.CSharp;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KeePassNatMsg.Tests
{
    // Execute the actual handler response block with the real Response/JsonBase
    // classes. No KeePass UI or user database is needed for this regression.
    [TestFixture]
    public class SetLoginResponseTests
    {
        [TestCase(true, "true", "success")]
        [TestCase(false, "false", "error")]
        public void SetLogin_ReturnsResultWithoutDuplicateProperty(bool saved, string success, string error)
        {
            string source = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Handlers.cs"));
            int handlerStart = source.IndexOf("private Response SetLogin(Request req)", StringComparison.Ordinal);
            Assert.That(handlerStart, Is.GreaterThanOrEqualTo(0));
            int start = source.IndexOf("var resp = req.GetResponse();", handlerStart, StringComparison.Ordinal);
            int end = source.IndexOf("return resp;", start, StringComparison.Ordinal) + "return resp;".Length;
            Assert.That(start, Is.GreaterThan(handlerStart));
            Assert.That(end, Is.GreaterThan(start));
            string block = source.Substring(start, end - start);
            string harness = "using KeePassNatMsg.Protocol.Action; using Newtonsoft.Json.Linq; " +
                "public static class SaveResponseProbe { public static Response Build(Request req, bool result) { " + block + " } }";
            var options = new CompilerParameters();
            options.GenerateInMemory = true;
            options.ReferencedAssemblies.Add(typeof(Response).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(JObject).Assembly.Location);
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            using (var compiler = new CSharpCodeProvider())
            {
                var compiled = compiler.CompileAssemblyFromSource(options, harness);
                Assert.That(compiled.Errors.HasErrors, Is.False, CompilerErrors(compiled));
                KeePassNatMsgExt.ExtInstance = new KeePassNatMsgExt();
                var request = Request.FromString("{\"action\":\"set-login\",\"clientID\":\"synthetic-test\",\"nonce\":\"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\"}");
                var build = compiled.CompiledAssembly.GetType("SaveResponseProbe").GetMethod("Build");
                Response response = null;
                Assert.DoesNotThrow(delegate { response = (Response)build.Invoke(null, new object[] { request, saved }); });
                Assert.That(response.GetString("action"), Is.EqualTo("set-login"));
                Assert.That(response.Message.GetString("success"), Is.EqualTo(success));
                Assert.That(response.Message.GetString("error"), Is.EqualTo(error));
                Assert.That(response.Message["count"].Type, Is.EqualTo(JTokenType.Null));
                Assert.That(response.Message["entries"].Type, Is.EqualTo(JTokenType.Null));
                Assert.That(response.Message.GetString("version"), Is.EqualTo("2.7.0"));
            }
        }

        private static string CompilerErrors(CompilerResults result)
        {
            var errors = new System.Text.StringBuilder();
            foreach (CompilerError error in result.Errors) errors.AppendLine(error.ToString());
            return errors.ToString();
        }
    }
}
