using PX.Data;
using PX.Data.WorkflowAPI;
using System;
using System.Linq.Expressions;

namespace PX.Objects.SO
{
    public class SOOrderEntry_ApprovalWorkflowExt : PXGraphExtension<SOOrderEntry_ApprovalWorkflow, SOOrderEntry_Workflow, SOOrderEntry>
    {
        public override void Configure(PXScreenConfiguration configuration)
        {
            base.Configure(configuration);

            configuration.GetScreenConfigurationContext<SOOrderEntry, SOOrder>().UpdateScreenConfigurationFor(
                (Func<BoundedTo<SOOrderEntry, SOOrder>.ScreenConfiguration.ConfiguratorScreen,
                      BoundedTo<SOOrderEntry, SOOrder>.ScreenConfiguration.ConfiguratorScreen>)
                (screen => screen.WithFlows((System.Action<BoundedTo<SOOrderEntry, SOOrder>.Workflow.ContainerAdjusterFlows>)
                           (flows =>
                           {
                               flows.Update<SOBehavior.sO>((Func<BoundedTo<SOOrderEntry, SOOrder>.Workflow.ConfiguratorFlow, BoundedTo<SOOrderEntry, SOOrder>.Workflow.ConfiguratorFlow>)
                                    (flow => flow.WithFlowStates((System.Action<BoundedTo<SOOrderEntry, SOOrder>.FlowState.ContainerAdjusterStates>)
                                    (flowstates => flowstates.Update<SOOrderStatus.pendingApproval>((Func<BoundedTo<SOOrderEntry, SOOrder>.FlowState.ConfiguratorState,
                                                                                                          BoundedTo<SOOrderEntry, SOOrder>.FlowState.ConfiguratorState>)
                                                   (flowstate => flowstate.WithActions((System.Action<BoundedTo<SOOrderEntry, SOOrder>.ActionState.ContainerAdjusterActions>)
                                                                (actions => actions.Add((Expression<Func<SOOrderEntry, PXAction<SOOrder>>>)(g => g.printSalesOrder))))))))));

                               flows.Update<SOBehavior.iN>((Func<BoundedTo<SOOrderEntry, SOOrder>.Workflow.ConfiguratorFlow, BoundedTo<SOOrderEntry, SOOrder>.Workflow.ConfiguratorFlow>)
                                    (flow => flow.WithFlowStates((System.Action<BoundedTo<SOOrderEntry, SOOrder>.FlowState.ContainerAdjusterStates>)
                                            (flowstates => flowstates.Update<SOOrderStatus.pendingApproval>((Func<BoundedTo<SOOrderEntry, SOOrder>.FlowState.ConfiguratorState,
                                                                                                                  BoundedTo<SOOrderEntry, SOOrder>.FlowState.ConfiguratorState>)
                                                           (flowstate => flowstate.WithActions((System.Action<BoundedTo<SOOrderEntry, SOOrder>.ActionState.ContainerAdjusterActions>)
                                                                         (actions => actions.Add((Expression<Func<SOOrderEntry, PXAction<SOOrder>>>)(g => g.printSalesOrder))))))))));
                           }
                           ))));
        }

        //public override void Configure(PXScreenConfiguration configuration)
        //{
        //    base.Configure(configuration);

        //    configuration.GetScreenConfigurationContext<SOOrderEntry, SOOrder>().UpdateScreenConfigurationFor(
        //        (Func<BoundedTo<SOOrderEntry, SOOrder>.ScreenConfiguration.ConfiguratorScreen, 
        //              BoundedTo<SOOrderEntry, SOOrder>.ScreenConfiguration.ConfiguratorScreen>)
        //        (screen => screen.WithFlows((System.Action<BoundedTo<SOOrderEntry, SOOrder>.Workflow.ContainerAdjusterFlows>)
        //                   (flows => flows.Update<SOBehavior.sO>((Func<BoundedTo<SOOrderEntry, SOOrder>.Workflow.ConfiguratorFlow, BoundedTo<SOOrderEntry, SOOrder>.Workflow.ConfiguratorFlow>)
        //                            (flow => flow.WithFlowStates((System.Action<BoundedTo<SOOrderEntry, SOOrder>.FlowState.ContainerAdjusterStates>)
        //                                    (flowstates => flowstates.Update<SOOrderStatus.pendingApproval>((Func<BoundedTo<SOOrderEntry, SOOrder>.FlowState.ConfiguratorState,
        //                                                                                                          BoundedTo<SOOrderEntry, SOOrder>.FlowState.ConfiguratorState>)
        //                                                   (flowstate => flowstate.WithActions((System.Action<BoundedTo<SOOrderEntry, SOOrder>.ActionState.ContainerAdjusterActions>)
        //                                                                 (actions => actions.Add((Expression<Func<SOOrderEntry, PXAction<SOOrder>>>)(g => g.printSalesOrder))))))))))))));
        //}
    }
}
