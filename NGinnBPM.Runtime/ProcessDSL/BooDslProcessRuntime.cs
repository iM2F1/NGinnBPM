﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.Runtime;
using NGinnBPM.Runtime.Tasks;
using NGinnBPM.ProcessModel;

namespace NGinnBPM.Runtime.ProcessDSL
{
    internal class BooDslProcessRuntime : IProcessScriptRuntime
    {
        private ProcessDefDSLBase _pd;
        private ProcessDef _def;

        internal BooDslProcessRuntime(ProcessDefDSLBase pd)
        {
            _pd = pd;
            _def = pd.GetProcessDef();
        }

        public string ProcessDefinitionId
        {
            get { return _def.DefinitionId; }
        }

        public void InitializeNewTask(TaskInstance ti, Dictionary<string, object> inputData, ITaskExecutionContext ctx)
        {
            if (string.IsNullOrEmpty(ti.InstanceId) ||
                string.IsNullOrEmpty(ti.TaskId) ||
                string.IsNullOrEmpty(ti.ProcessDefinitionId) ||
                string.IsNullOrEmpty(ti.ProcessInstanceId))
                throw new Exception("Task not inited properly");
            _pd.SetTaskInstanceInfo(ti, ctx);
            
            TaskDef td = _def.GetRequiredTask(ti.TaskId);
            foreach (var vd in td.Variables)
            {
                if (vd.VariableDir == ProcessModel.Data.VariableDef.Dir.In ||
                    vd.VariableDir == ProcessModel.Data.VariableDef.Dir.InOut)
                {
                    if (inputData.ContainsKey(vd.Name))
                        ti.TaskData[vd.Name] = inputData[vd.Name];
                }
                if (!ti.TaskData.ContainsKey(vd.Name))
                {
                    var k = DslUtil.TaskVariableDefaultKey(td.Id, vd.Name);
                    if (_pd._variableBinds.ContainsKey(k))
                    {
                        ti.TaskData[vd.Name] = _pd._variableBinds[k]();
                    }
                    else if (!string.IsNullOrEmpty(vd.DefaultValueExpr))
                    {
                        ti.TaskData[vd.Name] = vd.DefaultValueExpr; //TODO: add type conversion
                    }
                    else if (vd.IsRequired)
                        throw new NGinnBPM.ProcessModel.Exceptions.DataValidationException("Required variable missing: " + vd.Name).SetTaskId(ti.TaskId).SetProcessDef(ti.ProcessDefinitionId);
                }
            }
            //now initialize task parameters
            foreach (var bd in td.InputParameterBindings)
            {
                var pi = ti.GetType().GetProperty(bd.Target);
                if (pi == null)
                {
                    throw new NGinnBPM.ProcessModel.Exceptions.TaskParameterInvalidException(bd.Target, "Property not found: " + bd.Target).SetTaskId(ti.TaskId);
                }
                string k = DslUtil.TaskParamInBindingKey(td.Id, bd.Target);
                if (bd.BindType == DataBindingType.Expr)
                {
                    pi.SetValue(ti, _pd._variableBinds[k](), null);
                }
                else if (bd.BindType == DataBindingType.CopyVar)
                {
                    pi.SetValue(ti, ti.TaskData.ContainsKey(bd.Source) ? ti.TaskData[bd.Source] : null, null);
                }
                else if (bd.BindType == DataBindingType.Literal)
                {
                    pi.SetValue(ti, Convert.ChangeType(bd.Source, pi.PropertyType), null);
                }
                else throw new Exception();
            }
            string ks = DslUtil.TaskScriptKey(ti.TaskId, "_paramInit");
            if (_pd._taskScripts.ContainsKey(ks))
                _pd._taskScripts[ks]();
        }

        public Dictionary<string, object> GatherOutputData(TaskInstance ti, ITaskExecutionContext ctx)
        {
            string ks = DslUtil.TaskScriptKey(ti.TaskId, "_variableUpdateOnComplete");
            if (_pd._taskScripts.ContainsKey(ks)) _pd._taskScripts[ks]();
            var td = _def.GetRequiredTask(ti.TaskId);

            foreach (var bd in td.OutputParameterBindings)
            {
                string k = DslUtil.TaskParamOutBindingKey(td.Id, bd.Target);
                if (bd.BindType == DataBindingType.Expr)
                {
                    ti.TaskData[bd.Target] = _pd._variableBinds[k]();
                }
                else if (bd.BindType == DataBindingType.CopyVar)
                {
                    var pi = ti.GetType().GetProperty(bd.Source);
                    if (pi == null) throw new NGinnBPM.ProcessModel.Exceptions.TaskParameterInvalidException(bd.Source, "Property not found: " + bd.Source).SetTaskId(ti.TaskId);
                    ti.TaskData[bd.Target] = pi.GetValue(ti, null);
                }
                else if (bd.BindType == DataBindingType.Literal)
                {
                    ti.TaskData[bd.Target] = bd.Source; //todo: type convert
                }
                else throw new Exception();
            }
            Dictionary<string, object> ret = new Dictionary<string, object>();
            foreach (var vd in td.Variables.Where(x => x.VariableDir == ProcessModel.Data.VariableDef.Dir.Out || x.VariableDir == ProcessModel.Data.VariableDef.Dir.InOut))
            {
                ret[vd.Name] = ti.TaskData[vd.Name];
            }
            return ret;
        }

        public bool EvalFlowCondition(TaskInstance ti, ProcessModel.FlowDef fd, ITaskExecutionContext ctx)
        {
            string k = DslUtil.FlowConditionKey(fd.Parent.Id, fd.From, fd.To);
            if (!_pd._flowConditions.ContainsKey(k)) throw new Exception("!no flow cond..");
            _pd.SetTaskInstanceInfo(ti, ctx);
            return _pd._flowConditions[k]();
        }


        
        public void SetTaskScriptContext(TaskInstance task, Dictionary<string, object> inputData, Dictionary<string, object> outputData)
        {
            throw new NotImplementedException();
        }
    }
}
