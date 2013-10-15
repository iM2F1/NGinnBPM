﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.ProcessModel;
using NGinnBPM.Runtime.Tasks;

namespace NGinnBPM.Runtime
{
    public class Class1
    {
    }

    public interface IProcessPackageRepo
    {
        IEnumerable<string> PackageNames { get; }
        IProcessPackage GetProcessPackage(string name);
        /// <summary>
        /// Shorthand for returning process definition
        /// </summary>
        /// <param name="definitionId"></param>
        /// <returns></returns>
        ProcessDef GetProcessDef(string definitionId);
        /// <summary>
        /// Shortcut method for getting script runtime
        /// Warning: script runtime is not thread-safe.
        /// </summary>
        /// <param name="definitionId"></param>
        /// <returns></returns>
        IProcessScriptRuntime GetScriptRuntime(string definitionId);
    }

    public interface IProcessPackage
    {
        string Name { get; }
        IEnumerable<string> ProcessNames { get; }
        PackageDef GetPackageDef();
        ProcessDef GetProcessDefinition(string definitionId);
        IProcessScriptRuntime GetScriptRuntime(string processDefinition);
    }

    public interface IProcessScriptRuntime
    {
        string ProcessDefinitionId { get; }


        /// <summary>
        /// Execute task input data binding and initialize parameters according to bindings
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="inputData"></param>
        /// <param name="ctx"></param>
        void InitializeNewTask(TaskInstance ti, Dictionary<string, object> inputData, ITaskExecutionContext ctx);

        /// <summary>
        /// Execute task output parameter bindings and collect task output data
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        Dictionary<string, object> GatherOutputData(TaskInstance ti, ITaskExecutionContext ctx);

        bool EvalFlowCondition(TaskInstance ti, FlowDef fd, ITaskExecutionContext ctx);

        /// <summary>
        /// Prepare input data for a child task by evaling task's input data bindings
        /// </summary>
        /// <param name="cti"></param>
        /// <param name="childTask"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        Dictionary<string, object> PrepareChildTaskInputData(CompositeTaskInstance cti, TaskDef childTask, ITaskExecutionContext ctx);
        /// <summary>
        /// Prepare input data for a multi-instance child task.
        /// </summary>
        /// <param name="cti"></param>
        /// <param name="childTask"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        IEnumerable<Dictionary<string, object>> PrepareMultiInstanceTaskInputData(CompositeTaskInstance cti, TaskDef childTask, ITaskExecutionContext ctx);
    }
}
