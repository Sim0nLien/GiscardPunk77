using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GiscardPunk77.AI.Behavior;
using GiscardPunk77.AI.Behavior.Guard;
using GiscardPunk77.AI.Behavior.Guard.Nodes;
using GiscardPunk77.AI.Behavior.Nodes;
using UnityEditor;
using UnityEngine;

namespace GiscardPunk77.Editor
{
    /// <summary>
    /// Creates only the P10 graph asset. Reflection is intentionally confined here because
    /// Unity Behavior 1.0.13 keeps its authoring model internal and Editor-only.
    /// </summary>
    public static class NpcBehaviorGraphAuthoring
    {
        public const string GraphPath = "Assets/_Project/Config/AI/P10 Minimal NPC Behavior.asset";
        public const string GuardGraphPath = "Assets/_Project/Config/AI/P11 Guard Routine.asset";

        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags AnyInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags AnyStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        [MenuItem("Tools/GiscardPunk77/P10/Create or Open Minimal Behavior Graph")]
        public static void CreateOrOpenMinimalGraph()
        {
            var createdAsset = false;
            try
            {
                var graphType = RequireType(
                    "Unity.Behavior.BehaviorAuthoringGraph",
                    "Unity.Behavior.Authoring");
                var existing = AssetDatabase.LoadMainAssetAtPath(GraphPath);
                if (existing != null)
                {
                    if (!graphType.IsInstanceOfType(existing))
                    {
                        Debug.LogError(
                            $"P10 cannot create its graph because another asset already exists at {GraphPath}.",
                            existing);
                        return;
                    }

                    Selection.activeObject = existing;
                    AssetDatabase.OpenAsset(existing);
                    Debug.Log("P10 minimal Behavior graph already exists; it was opened without being overwritten.", existing);
                    return;
                }

                EnsureFolder("Assets/_Project/Config/AI");
                var graph = ScriptableObject.CreateInstance(graphType);
                graph.name = "P10 Minimal NPC Behavior";
                AssetDatabase.CreateAsset(graph, GraphPath);
                createdAsset = true;
                InvokeRequired(graph, "ValidateAsset");

                var blackboard = graphType.GetField("Blackboard", PublicInstance)?.GetValue(graph)
                    ?? throw new InvalidOperationException("Behavior graph did not create its authoring blackboard.");
                var contextVariable = AddVariable(blackboard, "Context", typeof(NpcContext), true, null);
                var sourceVariable = AddVariable(blackboard, "Destination Source", typeof(GameObject), true, null);
                var destinationVariable = AddVariable(
                    blackboard,
                    "Destination",
                    typeof(Vector3),
                    false,
                    Vector3.zero);

                var nodes = GetEnumerableProperty(graph, "Nodes");
                var startNode = nodes.Cast<object>().FirstOrDefault(node => node.GetType().Name == "StartNodeModel")
                    ?? throw new InvalidOperationException("Behavior graph did not create its Start node.");
                object outputPort = GetFirstOutputPort(startNode);

                var choose = AddNode(graph, typeof(NpcChooseDestinationAction), new Vector2(0f, 180f), outputPort);
                LinkField(choose, "Source", sourceVariable, typeof(GameObject));
                LinkField(choose, "Destination", destinationVariable, typeof(Vector3));
                outputPort = GetFirstOutputPort(choose);

                var move = AddNode(graph, typeof(NpcMoveToDestinationAction), new Vector2(0f, 360f), outputPort);
                LinkField(move, "Context", contextVariable, typeof(NpcContext));
                LinkField(move, "Destination", destinationVariable, typeof(Vector3));
                outputPort = GetFirstOutputPort(move);

                AddNode(graph, typeof(NpcWaitAction), new Vector2(0f, 540f), outputPort);

                InvokeRequired(blackboard, "SetAssetDirty");
                InvokeRequired(graph, "SetAssetDirty", true);
                InvokeRequired(graph, "BuildRuntimeGraph", true);
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(GraphPath, ImportAssetOptions.ForceUpdate);

                var savedGraph = AssetDatabase.LoadMainAssetAtPath(GraphPath);
                Selection.activeObject = savedGraph;
                AssetDatabase.OpenAsset(savedGraph);
                Debug.Log(
                    "P10 graph created: assign Context and Destination Source on a BehaviorGraphAgent, then enter Play Mode.",
                    savedGraph);
            }
            catch (Exception exception)
            {
                if (createdAsset && AssetDatabase.LoadMainAssetAtPath(GraphPath) != null)
                {
                    AssetDatabase.DeleteAsset(GraphPath);
                }

                Debug.LogError(
                    "P10 could not create the minimal Behavior graph. " +
                    "Its partial asset was removed. Confirm that Behavior 1.0.13 compiles, " +
                    "then read the first inner error.\n" +
                    Unwrap(exception));
            }
        }

        [MenuItem("Tools/GiscardPunk77/P11/Create or Open Guard Behavior Graph")]
        public static void CreateOrOpenGuardGraph()
        {
            var createdAsset = false;
            try
            {
                var graphType = RequireType(
                    "Unity.Behavior.BehaviorAuthoringGraph",
                    "Unity.Behavior.Authoring");
                var existing = AssetDatabase.LoadMainAssetAtPath(GuardGraphPath);
                if (existing != null)
                {
                    if (!graphType.IsInstanceOfType(existing))
                    {
                        Debug.LogError(
                            $"P11 cannot create its graph because another asset already exists at {GuardGraphPath}.",
                            existing);
                        return;
                    }

                    Selection.activeObject = existing;
                    AssetDatabase.OpenAsset(existing);
                    Debug.Log("P11 Guard graph already exists; it was opened without being overwritten.", existing);
                    return;
                }

                EnsureFolder("Assets/_Project/Config/AI");
                var graph = ScriptableObject.CreateInstance(graphType);
                graph.name = "P11 Guard Routine";
                AssetDatabase.CreateAsset(graph, GuardGraphPath);
                createdAsset = true;
                InvokeRequired(graph, "ValidateAsset");

                var blackboard = graphType.GetField("Blackboard", PublicInstance)?.GetValue(graph)
                    ?? throw new InvalidOperationException("Guard graph did not create its authoring blackboard.");
                var guardContextVariable = AddVariable(blackboard, "Guard Context", typeof(GuardContext), true, null);

                var nodes = GetEnumerableProperty(graph, "Nodes");
                var startNode = nodes.Cast<object>().FirstOrDefault(node => node.GetType().Name == "StartNodeModel")
                    ?? throw new InvalidOperationException("Guard graph did not create its Start node.");
                var startOutput = GetFirstOutputPort(startNode);

                var routine = AddNode(graph, typeof(GuardRoutineComposite), new Vector2(0f, 180f), startOutput);
                LinkField(routine, "Context", guardContextVariable, typeof(GuardContext));
                var routineOutput = GetFirstOutputPort(routine);

                var idle = AddNode(graph, typeof(GuardIdleAction), new Vector2(-480f, 400f), routineOutput);
                LinkField(idle, "Context", guardContextVariable, typeof(GuardContext));

                var patrol = AddNode(graph, typeof(GuardPatrolAction), new Vector2(-160f, 400f), routineOutput);
                LinkField(patrol, "Context", guardContextVariable, typeof(GuardContext));

                var suspicious = AddNode(graph, typeof(GuardSuspiciousAction), new Vector2(160f, 400f), routineOutput);
                LinkField(suspicious, "Context", guardContextVariable, typeof(GuardContext));

                var investigate = AddNode(
                    graph,
                    typeof(GuardInvestigateLastKnownPositionAction),
                    new Vector2(480f, 400f),
                    routineOutput);
                LinkField(investigate, "Context", guardContextVariable, typeof(GuardContext));

                InvokeRequired(blackboard, "SetAssetDirty");
                InvokeRequired(graph, "SetAssetDirty", true);
                InvokeRequired(graph, "BuildRuntimeGraph", true);
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(GuardGraphPath, ImportAssetOptions.ForceUpdate);

                var savedGraph = AssetDatabase.LoadMainAssetAtPath(GuardGraphPath);
                Selection.activeObject = savedGraph;
                AssetDatabase.OpenAsset(savedGraph);
                Debug.Log(
                    "P11 Guard graph created with four ordered state actions and a global-alert interrupt.",
                    savedGraph);
            }
            catch (Exception exception)
            {
                if (createdAsset && AssetDatabase.LoadMainAssetAtPath(GuardGraphPath) != null)
                {
                    AssetDatabase.DeleteAsset(GuardGraphPath);
                }

                Debug.LogError(
                    "P11 could not create the Guard Behavior graph. Its partial asset was removed.\n" +
                    Unwrap(exception));
            }
        }

        private static object AddVariable(
            object blackboard,
            string name,
            Type valueType,
            bool isExposed,
            object value)
        {
            var genericModel = RequireType(
                "Unity.Behavior.GraphFramework.TypedVariableModel`1",
                "Unity.Behavior.GraphFramework");
            var variable = Activator.CreateInstance(genericModel.MakeGenericType(valueType));
            var variableType = variable.GetType();
            variableType.GetField("Name", PublicInstance)?.SetValue(variable, name);
            variableType.GetField("IsExposed", PublicInstance)?.SetValue(variable, isExposed);

            if (value != null)
            {
                variableType.GetProperty("ObjectValue", PublicInstance)?.SetValue(variable, value);
            }

            GetEnumerableProperty(blackboard, "Variables").Add(variable);
            return variable;
        }

        private static object AddNode(
            UnityEngine.Object graph,
            Type runtimeNodeType,
            Vector2 position,
            object connectedPort)
        {
            var registryType = RequireType("Unity.Behavior.NodeRegistry", "Unity.Behavior.Authoring");
            var nodeInfo = registryType.GetMethod("GetInfo", AnyStatic)?.Invoke(null, new object[] { runtimeNodeType })
                ?? throw new InvalidOperationException($"Behavior did not register node {runtimeNodeType.Name}.");
            var serializableModelType = nodeInfo.GetType().GetField("ModelType", AnyInstance)?.GetValue(nodeInfo)
                ?? throw new InvalidOperationException($"Behavior did not expose a model for {runtimeNodeType.Name}.");
            var modelType = serializableModelType.GetType().GetProperty("Type", PublicInstance)?.GetValue(serializableModelType) as Type
                ?? throw new InvalidOperationException($"Behavior model type for {runtimeNodeType.Name} is invalid.");

            var createNode = graph.GetType().GetMethod("CreateNode", PublicInstance)
                ?? throw new MissingMethodException(graph.GetType().FullName, "CreateNode");
            var node = createNode.Invoke(
                graph,
                new object[] { modelType, position, connectedPort, new[] { nodeInfo } });
            InvokeRequired(node, "OnValidate");
            return node;
        }

        private static void LinkField(object node, string fieldName, object variable, Type valueType)
        {
            var method = GetInstanceMethods(node.GetType())
                .FirstOrDefault(candidate =>
                {
                    if (candidate.Name != "SetField" || candidate.IsGenericMethod)
                    {
                        return false;
                    }

                    var parameters = candidate.GetParameters();
                    return parameters.Length == 3 && parameters[0].ParameterType == typeof(string);
                }) ?? throw new MissingMethodException(node.GetType().FullName, "SetField");
            method.Invoke(node, new[] { fieldName, variable, valueType });
        }

        private static object GetFirstOutputPort(object node)
        {
            var outputPorts = GetEnumerableProperty(node, "OutputPortModels");
            return outputPorts.Cast<object>().FirstOrDefault()
                ?? throw new InvalidOperationException($"Node {node.GetType().Name} has no output port.");
        }

        private static IList GetEnumerableProperty(object target, string propertyName)
        {
            var value = target.GetType().GetProperty(propertyName, PublicInstance)?.GetValue(target);
            if (value is IList list)
            {
                return list;
            }

            if (value is IEnumerable enumerable)
            {
                return new EnumerableListAdapter(enumerable);
            }

            throw new MissingMemberException(target.GetType().FullName, propertyName);
        }

        private static void InvokeRequired(object target, string methodName, params object[] arguments)
        {
            var method = GetInstanceMethods(target.GetType())
                .FirstOrDefault(candidate =>
                    candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length)
                ?? throw new MissingMethodException(target.GetType().FullName, methodName);
            method.Invoke(target, arguments);
        }

        private static IEnumerable<MethodInfo> GetInstanceMethods(Type type)
        {
            while (type != null)
            {
                foreach (var method in type.GetMethods(AnyInstance | BindingFlags.DeclaredOnly))
                {
                    yield return method;
                }

                type = type.BaseType;
            }
        }

        private static Type RequireType(string fullName, string assemblyName)
        {
            var type = Type.GetType($"{fullName}, {assemblyName}");
            if (type != null)
            {
                return type;
            }

            type = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == assemblyName)
                ?.GetType(fullName);
            return type ?? throw new TypeLoadException($"Missing type {fullName} from {assemblyName}.");
        }

        private static void EnsureFolder(string folderPath)
        {
            var segments = folderPath.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }

        private static string Unwrap(Exception exception)
        {
            while (exception is TargetInvocationException && exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            return exception.ToString();
        }

        /// <summary>Read-only adapter used only for inherited IEnumerable node-port properties.</summary>
        private sealed class EnumerableListAdapter : IList
        {
            private readonly object[] values;

            public EnumerableListAdapter(IEnumerable enumerable)
            {
                values = enumerable.Cast<object>().ToArray();
            }

            public IEnumerator GetEnumerator() => values.GetEnumerator();
            public int Count => values.Length;
            public object SyncRoot => this;
            public bool IsSynchronized => false;
            public bool IsReadOnly => true;
            public bool IsFixedSize => true;
            public object this[int index] { get => values[index]; set => throw new NotSupportedException(); }
            public int Add(object value) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Contains(object value) => values.Contains(value);
            public int IndexOf(object value) => Array.IndexOf(values, value);
            public void Insert(int index, object value) => throw new NotSupportedException();
            public void Remove(object value) => throw new NotSupportedException();
            public void RemoveAt(int index) => throw new NotSupportedException();
            public void CopyTo(Array array, int index) => values.CopyTo(array, index);
        }
    }
}
