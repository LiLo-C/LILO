using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Lilo.Editor
{
    /// <summary>Runs the monster EditMode tests headlessly and logs the tally.
    /// Run via menu LILO/Run Monster Tests (edit mode only).</summary>
    public static class RunMonsterTestsMenu
    {
        private class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"[MonsterTests] Pass={result.PassCount} Fail={result.FailCount} " +
                          $"Skip={result.SkipCount} Inconclusive={result.InconclusiveCount} " +
                          $"Duration={result.Duration:0.00}s");
                foreach (var child in result.Children)
                    LogTree(child, 1);
            }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            private static void LogTree(ITestResultAdaptor node, int depth)
            {
                if (node.Test.IsSuite)
                {
                    foreach (var child in node.Children) LogTree(child, depth + 1);
                    return;
                }
                string line = $"[MonsterTests] {node.ResultState}: {node.Test.FullName}";
                if (node.ResultState == "Failed")
                    Debug.LogError(line + "\n" + node.Message);
                else
                    Debug.Log(line);
            }
        }

        private static Callbacks _callbacks = new Callbacks();

        [MenuItem("LILO/Run Monster Tests")]
        public static void Run()
        {
            AssetDatabase.Refresh();
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(_callbacks);
            var settings = new ExecutionSettings
            {
                runSynchronously = true,
            };
            settings.filters = new[] { new Filter { testMode = TestMode.EditMode } };
            Debug.Log("[MonsterTests] Running EditMode tests...");
            api.Execute(settings);
        }
    }
}
